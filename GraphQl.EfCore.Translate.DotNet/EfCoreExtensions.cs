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
		public static IQueryable<T> GraphQl<T>(this IQueryable<T> queryable, IResolveFieldContext<object> context, string? path = null, string defaultOrder = "")
		{
			queryable = queryable
				.GraphQlWhere(context)
				.GraphQlOrder(context, defaultOrder)
				.GraphQlPagination(context)
				.GraphQlSelect(context, path);

			return queryable;
		}

		public static IQueryable<T> GraphQlSelect<T>(this IQueryable<T> queryable, IResolveFieldContext<object> context, string? path = null)
		{
			var items = ConvertFieldToNodeGraph(context.SubFields.Select(x => x.Value), context, path is not null ? path.Split('.') : null);
			return EfCoreExtensions.GraphQlSelect(queryable, items);
		}

		public static IQueryable<T> GraphQlWhere<T>(this IQueryable<T> queryable, IResolveFieldContext<object> context)
		{
			var argument = GetNameArgument(context, "where", "Where");
			if (argument is not null)
			{
				var wheres = context.GetArgument<List<WhereExpression>>(argument)!;

				return EfCoreExtensions.GraphQlWhere(queryable, wheres);
			}

			return queryable;
		}

		public static IQueryable<T> GraphQlPagination<T>(this IQueryable<T> queryable, IResolveFieldContext<object> context)
		{
			var argumentTake = GetNameArgument(context, "take", "Take");
			var argumentSkip = GetNameArgument(context, "skip", "Skip");
			if (argumentTake is not null || argumentSkip is not null) {
				var take = argumentTake is not null ? context.GetArgument<int?>(argumentTake, null) : null;
				var skip = argumentSkip is not null ? context.GetArgument<int>(argumentSkip, 0) : 0;

				return EfCoreExtensions.GraphQlPagination(queryable, skip, take);
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
			}
			else if (!string.IsNullOrWhiteSpace(defaultOrder)) {
				return EfCoreExtensions.GraphQlOrder(queryable, defaultOrder);
			}

			return queryable;
		}

		public static void AddCalculatedField<T>(string path, Func<Expression, Expression> func)
		{
			EfCoreExtensions.AddCalculatedField<T>(path, func);
		}

		static string? GetNameArgument(IResolveFieldContext<object> context, params string[] names) {
			return names.FirstOrDefault(name => context.HasArgument(name));
		}

		static List<NodeGraph> ConvertFieldToNodeGraph(IEnumerable<object> fields, IResolveFieldContext<object> context, string[]? path = null)
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
						Dictionary<string, object> args = new Dictionary<string, object>();
						foreach (var arg in f.Arguments ?? new Arguments())
						{
							var name = arg.Name.ToLower();
							if (!keysArgs.Contains(name.ToLower()))
							{
								continue;
							}

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
								args.Add(name, value);
							}
						}

						list.Add(new NodeGraph
						{
							Path = key,
							Arguments = args
						});
					}

					if (f.SelectionSet.Children.Count() > 0)
					{
						TransformationFieldToNodeGraph(f.SelectionSet.Children, key, depth + 1);
					}
				}
			};

			TransformationFieldToNodeGraph(fields, "", 0);
			return list.ToList();
		}
	}
}
