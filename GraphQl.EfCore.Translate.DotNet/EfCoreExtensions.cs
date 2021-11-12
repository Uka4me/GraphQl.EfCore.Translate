using GraphQl.EfCore.Translate.Select.Graphs;
using GraphQL;
using GraphQL.Language.AST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GraphQl.EfCore.Translate.DotNet
{
	public static class EfCoreExtensionsDotNet
	{
		public static IQueryable<T> GraphQl<T>(this IQueryable<T> queryable, IResolveFieldContext<object> context, IEnumerable<object> fields = null, string defaultOrder = "")
		{
			queryable = queryable
				.GraphQlWhere(context)
				.GraphQlOrder(context, defaultOrder)
				.GraphQlPagination(context)
				.GraphQlSelect(context, fields);

			return queryable;
		}

		public static IQueryable<T> GraphQlSelect<T>(this IQueryable<T> queryable, IResolveFieldContext<object> context, IEnumerable<object> fields = null)
		{
			var items = ConvertFieldToNodeGraph(fields is null ? context.SubFields.Select(x => x.Value) : fields, context);
			return EfCoreExtensions.GraphQlSelect(queryable, items);
			//var lambdaSelect = ExpressionBuilderSelect<T>.BuildPredicate(items);
			//return queryable.Select(lambdaSelect);
		}

		public static IQueryable<T> GraphQlWhere<T>(this IQueryable<T> queryable, IResolveFieldContext<object> context)
		{
			var argument = GetNameArgument(context, "where", "Where");
			if (argument is not null)
			{
				var wheres = context.GetArgument<List<WhereExpression>>(argument)!;

				return EfCoreExtensions.GraphQlWhere(queryable, wheres);
				//var predicate = ExpressionBuilderWhere<T>.BuildPredicate(wheres);
				//queryable = queryable.Where(predicate);
			}

			return queryable;
		}

		public static IQueryable<T> GraphQlPagination<T>(this IQueryable<T> queryable, IResolveFieldContext<object> context)
		{
			var argumentTake = GetNameArgument(context, "take", "Take");
			var argumentSkip = GetNameArgument(context, "skip", "Skip");
			if (argumentTake is not null || argumentSkip is not null) {
				var take = context.GetArgument<int>(argumentTake, 0);
				var skip = context.GetArgument<int>(argumentSkip, 0);

				return EfCoreExtensions.GraphQlPagination(queryable, skip, take);
				/*queryable = queryable.Skip(skip);

				if (take > 0)
				{
					queryable = queryable.Take(take);
				}*/
			}

			return queryable;
		}

		public static IQueryable<T> GraphQlOrder<T>(this IQueryable<T> queryable, IResolveFieldContext<object> context, string defaultOrder = "")
		{
			var argument = GetNameArgument(context, "orderby", "orderBy", "OrderBy");
			if (argument is not null)
			{
				var orders = context.GetArgument<string>(argument, defaultOrder);

				return EfCoreExtensions.GraphQlOrder(queryable, orders);
				/*queryable = queryable.OrderBy(orders);*/
			}
			else if (!string.IsNullOrWhiteSpace(defaultOrder)) {
				return EfCoreExtensions.GraphQlOrder(queryable, defaultOrder);
				/*queryable = queryable.OrderBy(defaultOrder);*/
			}

			return queryable;
		}

		static string GetNameArgument(IResolveFieldContext<object> context, params string[] names) {
			return names.FirstOrDefault(name => context.HasArgument(name));
		}

		static List<NodeGraph> ConvertFieldToNodeGraph(IEnumerable<object> fields, IResolveFieldContext<object> context)
		{
			HashSet<NodeGraph> list = new HashSet<NodeGraph>();

			Func<IEnumerable<object>, IEnumerable<Field>> UnpackFragments = null;
			UnpackFragments = (objects) => {
				List<Field> listFields = new List<Field>();
				foreach (var obj in objects)
				{
					if (obj is FragmentSpread)
					{
						var h = context.Document.Fragments.FindDefinition((obj as FragmentSpread).Name);
						listFields.AddRange(UnpackFragments(h.SelectionSet.Children));
						continue;
					}

					listFields.Add(obj as Field);
				}

				return listFields;
			};

			Action<IEnumerable<object>, string> TransformationFieldToNodeGraph = null;
			TransformationFieldToNodeGraph = (t, keys) =>
			{
				foreach (var f in UnpackFragments(t))
				{
					var key = $"{(!string.IsNullOrWhiteSpace(keys) ? keys + "." : "")}{f.Name}";

                    if (list.Any(x => x.Path == key))
                    {
                        continue;
                    }

                    Dictionary<string, object> args = new Dictionary<string, object>();
					foreach (var arg in f.Arguments ?? new Arguments())
					{
						var value = arg.Value.Value;
						var variable = arg.Value as VariableReference;
						if (variable != null)
						{
							value = context.Variables != null
									? context.Variables.ValueFor(variable.Name)
									: null;
						}

						if (value != null)
						{
							args.Add(arg.Name, value);
						}
					}

					list.Add(new NodeGraph
					{
						Path = key,
						Arguments = args
					});

					if (f.SelectionSet.Children.Count() > 0)
					{
						TransformationFieldToNodeGraph(f.SelectionSet.Children, key);
					}
				}
			};

			TransformationFieldToNodeGraph(fields, "");
			return list.ToList();
		}
	}
}
