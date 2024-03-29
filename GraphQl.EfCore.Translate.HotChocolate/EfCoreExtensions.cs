﻿using GraphQl.EfCore.Translate.Converters;
using GraphQl.EfCore.Translate.Select.Graphs;
using GraphQl.EfCore.Translate.Where.Graphs;
using HotChocolate;
using HotChocolate.Execution.Processing;
using HotChocolate.Language;
using HotChocolate.Resolvers;
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
                /*queryable = queryable.OrderBy(orders);*/
            }
            else if (!string.IsNullOrWhiteSpace(defaultOrder))
            {
                return EfCoreExtensions.GraphQlOrder(queryable, defaultOrder);
                /*queryable = queryable.OrderBy(defaultOrder);*/
            }

            return queryable;
        }

        static string? GetNameArgument(IResolverContext context, string name)
        {
            var arguments = context.GetType().GetProperty("Arguments")?.GetValue(context);

            if (arguments is not null)
            {
                return ((IDictionary<NameString, ArgumentValue>)arguments).Select(x => x.Key.Value).FirstOrDefault(x => x.ToLower() == name.ToLower());
            }
            return null;
        }

        static List<NodeGraph> ConvertFieldToNodeGraph(IEnumerable<object> fields, IResolverContext context, string[]? path = null)
        {
            HashSet<NodeGraph> list = new HashSet<NodeGraph>();

            Func<IEnumerable<object>, IEnumerable<FieldNode>> UnpackFragments = null;
            UnpackFragments = (objects) =>
            {
                List<FieldNode> listFields = new List<FieldNode>();
                foreach (var obj in objects)
                {
                    if (obj is FragmentSpreadNode)
                    {
                        var h = context.Document.Definitions
                        .Single(x => (x is FragmentDefinitionNode) && x.Kind == SyntaxKind.FragmentDefinition && (x as FragmentDefinitionNode).Name.Value == (obj as FragmentSpreadNode).Name.Value);
                        listFields.AddRange(UnpackFragments((h as FragmentDefinitionNode).SelectionSet.Selections));
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
                        foreach (var arg in arguments.Where(x => keysArgs.Contains(x.Name.Value.ToLower())))
                        {
                            var name = arg.Name.Value.ToLower();

                            object value = arg.Value;

                            if (arg.Value is VariableNode)
                            {
                                context.Variables.TryGetVariable(arg.Value.Value.ToString(), out value);
                            }

                            if (value is ListValueNode)
                            {
                                var converter = new ObjectValueToDictionaryConverter();
                                value = converter.Convert((ListValueNode)value);
                            }

                            if (value is null)
                            {
                                continue;
                            }

                            if (name == "skip")
                            {
                                args.Skip = (value is IValueNode ? int.Parse((value as IValueNode)?.Value?.ToString()) : null);
                            }
                            if (name == "take")
                            {
                                args.Take = (int?)(value is IValueNode ? int.Parse((value as IValueNode)?.Value?.ToString()) : null);
                            }
                            if (name == "orderby")
                            {
                                args.OrderBy = (value as IValueNode)?.Value?.ToString();
                            }
                            if (name == "where")
                            {
                                value = Converters.DictionaryToObjectConverter.Convert<WhereExpression>(value);
                                args.Where = value is null ? null : (List<WhereExpression>)value;
                            }
                        }

                        list.Add(new NodeGraph
                        {
                            Path = key,
                            Arguments = args
                        });
                    }

                    if (f.SelectionSet is not null && f.SelectionSet.Selections.Count() > 0)
                    {
                        TransformationFieldToNodeGraph(f.SelectionSet!.Selections.AsEnumerable(), key, depth + 1);
                    }
                }
            };

            TransformationFieldToNodeGraph(fields, "", 0);
            return list.ToList();
        }
    }
}
