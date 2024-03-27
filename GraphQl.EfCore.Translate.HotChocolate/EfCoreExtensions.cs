using GraphQl.EfCore.Translate.Converters;
using GraphQl.EfCore.Translate.Select.Graphs;
using GraphQl.EfCore.Translate.Where.Graphs;
using HotChocolate;
using HotChocolate.Execution.Processing;
using HotChocolate.Language;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using HotChocolate.Utilities;
using System.Linq;
using System.Linq.Expressions;

namespace GraphQl.EfCore.Translate.HotChocolate
{
	public static class EfCoreExtensionsHotChocolate
	{
        public static IQueryable<T> GraphQl<T>(this IQueryable<T> queryable, IResolverContext context, string? path = null, string defaultOrder = "")
        {
            queryable = queryable
                .GraphQlWhere(context)
                .GraphQlOrder(context, defaultOrder)
                .GraphQlPagination(context)
                .GraphQlSelect(context, path);

            return queryable;
        }

        public static IQueryable<T> GraphQlWhere<T>(this IQueryable<T> queryable, IResolverContext context)
        {
            var argument = GetNameArgument(context, "where");
            if (argument is not null)
            {
                var wheres = context.ArgumentValue<List<WhereExpression>>(argument) ?? new List<WhereExpression>();

                return EfCoreExtensions.GraphQlWhere(queryable, wheres);
            }

            return queryable;
        }

        public static IQueryable<T> GraphQlSelect<T>(this IQueryable<T> queryable, IResolverContext context, string? path = null)
        {
            var root = ((Selection)context.Selection).SelectionSet?.Selections.AsEnumerable();
            var items = ConvertFieldToNodeGraph(root, context, path is not null ? path.Split('.') : null);
            return EfCoreExtensions.GraphQlSelect(queryable, items);
        }

        public static IQueryable<T> GraphQlPagination<T>(this IQueryable<T> queryable, IResolverContext context)
        {
            var argumentTake = GetNameArgument(context, "take");
            var argumentSkip = GetNameArgument(context, "skip");
            if (argumentTake is not null || argumentSkip is not null)
            {
                var take = argumentTake is not null ? context.ArgumentValue<int?>(argumentTake) : null;
                var skip = argumentSkip is not null ? context.ArgumentValue<int?>(argumentSkip) ?? 0 : 0;

                return EfCoreExtensions.GraphQlPagination(queryable, skip, take);
            }

            return queryable;
        }

        public static IQueryable<T> GraphQlOrder<T>(this IQueryable<T> queryable, IResolverContext context, string defaultOrder = "")
        {
            var argument = GetNameArgument(context, "orderBy");
            if (argument is not null)
            {
                var orders = context.ArgumentValue<string?>(argument) ?? defaultOrder;

                return EfCoreExtensions.GraphQlOrder(queryable, orders);
            }
            else if (!string.IsNullOrWhiteSpace(defaultOrder))
            {
                return EfCoreExtensions.GraphQlOrder(queryable, defaultOrder);
            }

            return queryable;
        }

        static string? GetNameArgument(IResolverContext context, string name)
        {
            IReadOnlyDictionary<string, ArgumentValue>? arguments = (IReadOnlyDictionary<string, ArgumentValue>?)context.GetType().GetProperty("Arguments")?.GetValue(context);

            var key = arguments?.Select(x => x.Key).FirstOrDefault(x => x.ToLower() == name.ToLower());

            return key is not null && arguments is not null && context.ArgumentKind(key) is not ValueKind.Null ? key : null;
        }

        static List<NodeGraph> ConvertFieldToNodeGraph(IEnumerable<object> fields, IResolverContext context, string[]? path = null)
        {
            HashSet<NodeGraph> list = new HashSet<NodeGraph>();

            Func<IEnumerable<object>, IEnumerable<FieldNode>> UnpackFragments = null;
            UnpackFragments = (objects) =>
            {
                List<FieldNode> listFields = new List<FieldNode>();
                foreach (NamedSyntaxNode obj in objects)
                {
                    if (obj is FragmentSpreadNode)
                    {
                        var fragment = context.Operation.Document.Definitions.OfType<FragmentDefinitionNode>().FirstOrDefault(t => t.Name.Value.EqualsOrdinal(obj.Name.Value));
                        if (fragment is not null) {
                            listFields.AddRange(UnpackFragments!(fragment.SelectionSet.Selections));
                        }
                        
                        continue;
                    }

                    listFields.Add(obj as FieldNode);
                }

                return listFields;
            };

            Action<IEnumerable<object>, string, int> TransformationFieldToNodeGraph = null;
            TransformationFieldToNodeGraph = (t, keys, depth) =>
            {
                foreach (var f in UnpackFragments(t))
                {
                    var key = "";

                    if (path is not null && depth < path.Length && path[depth].ToLower() != f.Name.ToString().ToLower()) 
                    {
                        continue;
                    }

                    if (path is null || depth >= path.Length) 
                    {
                        key = string.IsNullOrWhiteSpace(keys) ? f.Name.ToString() : $"{keys}.{f.Name}";

                        if (list.Any(x => x.Path == key))
                        {
                            continue;
                        }

                        string[] keysArgs = new string[] { "where", "take", "skip", "orderby" };
                        var args = new ArgumentNodeGraph();
                        var arguments = (f.Arguments ?? new List<ArgumentNode>()).ToList();

                        arguments.ForEach(arg => {

                            string name = arg.Name.Value.ToLower();
                            IValueNode? value = arg.Value;

                            if (!keysArgs.Contains(name) || value is null) {
                                return;
                            }

                            if (value is VariableNode)
                            {
                                context.Variables.TryGetVariable(value.Value.ToString(), out value);
                            }

                            if (name == "skip")
                            {
                                args.Skip = (value as IntValueNode)?.ToInt32();
                                return;
                            }
                            if (name == "take")
                            {
                                args.Take = (value as IntValueNode)?.ToInt32();
                                return;
                            }
                            if (name == "orderby")
                            {
                                args.OrderBy = (value as StringValueNode)?.Value.ToString();
                                return;
                            }
                            if (name == "where")
                            {
                                object? v = value;
                                if (value is ListValueNode)
                                {
                                    v = new ObjectValueToDictionaryConverter().Convert(value as ListValueNode);
                                }

                                args.Where = Converters.DictionaryToObjectConverter.Convert<WhereExpression>(v);
                                return;
                            }
                        });

                        list.Add(new NodeGraph
                        {
                            Path = key,
                            Arguments = args
                        });
                    }

                    if (f.SelectionSet is not null && f.SelectionSet.Selections.Count() > 0)
                    {
                        TransformationFieldToNodeGraph!(f.SelectionSet.Selections.AsEnumerable(), key, depth + 1);
                    }
                }
            };

            TransformationFieldToNodeGraph(fields, "", 0);
            return list.ToList();
        }
    }
}
