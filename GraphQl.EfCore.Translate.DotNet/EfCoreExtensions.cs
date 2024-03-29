﻿using GraphQl.EfCore.Translate.Converters;
using GraphQl.EfCore.Translate.Select.Graphs;
using GraphQl.EfCore.Translate.Where.Graphs;
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
			var argument = GetNameArgument(context, "where");
			if (argument is not null)
			{
				var wheres = context.GetArgument<List<WhereExpression>>(argument)!;

				return EfCoreExtensions.GraphQlWhere(queryable, wheres);
			}

			return queryable;
		}

		public static IQueryable<T> GraphQlPagination<T>(this IQueryable<T> queryable, IResolveFieldContext<object> context)
		{
			var argumentTake = GetNameArgument(context, "take");
			var argumentSkip = GetNameArgument(context, "skip");
			if (argumentTake is not null || argumentSkip is not null) {
				var take = argumentTake is not null ? context.GetArgument<int?>(argumentTake, null) : null;
				var skip = argumentSkip is not null ? context.GetArgument<int>(argumentSkip, 0) : 0;

				return EfCoreExtensions.GraphQlPagination(queryable, skip, take);
			}

			return queryable;
		}

		public static IQueryable<T> GraphQlOrder<T>(this IQueryable<T> queryable, IResolveFieldContext<object> context, string defaultOrder = "")
		{
			var argument = GetNameArgument(context, "orderBy");
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

		static string? GetNameArgument(IResolveFieldContext<object> context, string name) {
			var key = context.Arguments?.Select(x => x.Key).FirstOrDefault(x => x.ToLower() == name.ToLower());

			// TODO: Without calling HasArgument, arguments from Variables are not obtained when calling GetArgument
			return key is not null && context.HasArgument(key) ? key : null;
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
						var args = new ArgumentNodeGraph();
						var arguments = (f.Arguments ?? new Arguments()).ToList();
						foreach (var arg in arguments.Where(x => keysArgs.Contains(x.Name.ToLower())))
						{
							var name = arg.Name.ToLower();

							var value = arg.Value.Value;
							var variable = arg.Value as VariableReference;
							if (variable != null)
							{
								value = context.Variables != null
										? context.Variables.ValueFor(variable.Name)
										: null;
							}

							if (value is null) {
								continue;
							}

							if (name == "skip")
							{
								args.Skip = (int?)value;
							}
							if (name == "take")
							{
								args.Take = (int?)value;
							}
							if (name == "orderby")
							{
								args.OrderBy = value?.ToString();
							}
							if (name == "where") {
								value = DictionaryToObjectConverter.Convert<WhereExpression>(value);
								args.Where = value is null ? null : (List<WhereExpression>)value;
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
