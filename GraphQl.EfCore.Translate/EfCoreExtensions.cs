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

namespace GraphQl.EfCore.Translate
{
	public static class EfCoreExtensions
	{
		public static IQueryable<T> GraphQlSelect<T>(this IQueryable<T> queryable, IResolveFieldContext<object> context)
		{
			var items = ConvertFieldToNodeGraph(context.SubFields.Select(x => x.Value), context);
			var lambdaSelect = ExpressionBuilderSelect<T>.BuildPredicate(items);
			return queryable.Select(lambdaSelect);
		}

		public static IQueryable<T> GraphQlSelect<T>(this IQueryable<T> queryable, IResolveFieldContext<object> context, IEnumerable<object> fields)
		{
			var items = ConvertFieldToNodeGraph(fields, context);
			var lambdaSelect = ExpressionBuilderSelect<T>.BuildPredicate(items);
			return queryable.Select(lambdaSelect);
		}

		public static IQueryable<T> GraphQlWhere<T>(this IQueryable<T> queryable, IResolveFieldContext<object> context)
		{
			if (context.HasArgument("where"))
			{
				var wheres = context.GetArgument<List<WhereExpression>>("where")!;
				
				var predicate = ExpressionBuilderWhere<T>.BuildPredicate(wheres);
				queryable = queryable.Where(predicate);
			}

			return queryable;
		}

		public static IQueryable<T> GraphQlPagination<T>(this IQueryable<T> queryable, IResolveFieldContext<object> context)
		{
			if (context.HasArgument("take") || context.HasArgument("skip")) {
				var take = context.GetArgument<int>("take", 0);
				var skip = context.GetArgument<int>("skip", 0);

				queryable = queryable.Skip(skip);

				if (take > 0)
				{
					queryable = queryable.Take(take);
				}
			}

			return queryable;
		}

		public static IQueryable<T> GraphQlOrder<T>(this IQueryable<T> queryable, IResolveFieldContext<object> context, string defaultOrder = "")
		{
			if (context.HasArgument("orderBy"))
			{
				var orders = context.GetArgument<string>("orderBy", defaultOrder);

				queryable = queryable.OrderBy(orders);
			}
			else if (!string.IsNullOrWhiteSpace(defaultOrder)) {
				queryable = queryable.OrderBy(defaultOrder);
			}

			return queryable;
		}

		static IQueryable<T> OrderBy<T>(this IQueryable<T> source, string orderByProperty, bool desc, bool isThenBy = false)
		{
			string command = isThenBy ? (desc ? "ThenByDescending" : "ThenBy") : (desc ? "OrderByDescending" : "OrderBy");
			var parameter = PropertyCache<T>.SourceParameter;
			var property = PropertyCache<T>.GetProperty(orderByProperty);
			var propertyAccess = Expression.MakeMemberAccess(parameter, property.Info);
			var orderByExpression = Expression.Lambda(propertyAccess, parameter);
			var resultExpression = Expression.Call(typeof(Queryable), command, new Type[] { typeof(T), property.PropertyType },
										  source.Expression, Expression.Quote(orderByExpression));
			return source.Provider.CreateQuery<T>(resultExpression);
		}

		static IQueryable<T> OrderBy<T>(this IQueryable<T> source, string sqlOrderByList)
		{
			var ordebyItems = sqlOrderByList.Trim().Split(',');
			IQueryable<T> result = source;
			bool useThenBy = false;
			foreach (var item in ordebyItems)
			{
				var splt = item.Trim().Split(' ');
				result = result.OrderBy(splt[0].Trim(), (splt.Length > 1 && splt[1].Trim().ToLower() == "desc"), useThenBy);
				useThenBy = true;
			}
			return result;
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
