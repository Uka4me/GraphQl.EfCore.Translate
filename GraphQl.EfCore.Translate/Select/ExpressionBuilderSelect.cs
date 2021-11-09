using GraphQl.EfCore.Translate.Select.Graphs;
using GraphQL;
using GraphQL.Language.AST;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GraphQl.EfCore.Translate
{
    public static class ExpressionBuilderSelect<T>
    {
		public static ConcurrentDictionary<string, Func<Expression, Expression>> CalculatedFields = new();
		const BindingFlags bindingFlagsPublic = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase | BindingFlags.FlattenHierarchy;

		public static Func<Expression, Expression> AddCalculatedField(string path, Func<Expression, Expression> func) {
			return CalculatedFields.GetOrAdd(path, x => func);
		}
		public static Expression<Func<T, T>> BuildPredicate(List<NodeGraph> fields)
        {
			var param = PropertyCache<T>.SourceParameter;
			var body = MakePredicateBody(typeof(T), param, fields.Select(x => x.Path).Select(m => m.Split('.')), fields);
			return Expression.Lambda<Func<T, T>>(body, param);
		}

		static Expression MakePredicateBody(Type targetType, Expression source, IEnumerable<string[]> memberPaths, List<NodeGraph> fields, int depth = 0)
		{
			var bindings = new List<MemberBinding>();
			var target = Expression.Constant(null, targetType);
			foreach (var memberGroup in memberPaths.GroupBy(path => path[depth]))
			{
				var memberName = memberGroup.Key;

				/*var m1 = typeof(PropertyCache<>).MakeGenericType(target.Type).GetMethod("GetProperty", new Type[] { typeof(string) });
                var targetMember = (MemberExpression)(typeof(Property<>).MakeGenericType(target.Type).GetProperty("Left")).GetValue(m1.Invoke(null, new[] { memberName }));
                var m2 = typeof(PropertyCache<>).MakeGenericType(source.Type).GetMethod("GetProperty", new Type[] { typeof(string) });
                var sourceMember = (MemberExpression)(typeof(Property<>).MakeGenericType(source.Type).GetProperty("Left")).GetValue(m2.Invoke(null, new[] { memberName }));
				*/

				var propertyOrFieldTarget = GetPropertyOrField(target.Type, memberName);
				var propertyOrFieldSource = GetPropertyOrField(source.Type, memberName);
				var targetMember = Expression.PropertyOrField(target, propertyOrFieldTarget.Name);
				var sourceMember = Expression.PropertyOrField(source, propertyOrFieldSource.Name);
				var childMembers = memberGroup.Where(path => depth + 1 < path.Length).ToList();

				var calculatedFields = (ConcurrentDictionary<string, Func<Expression, Expression>>)(typeof(ExpressionBuilderSelect<>).MakeGenericType(source.Type).GetField("CalculatedFields").GetValue(null));

				Expression targetValue = null;
				if (!childMembers.Any() || calculatedFields.ContainsKey(memberName))
				{
					if (calculatedFields.ContainsKey(memberName))
					{
						targetValue = calculatedFields[memberName].Invoke(source);
					}
					else
					{
						targetValue = sourceMember;
					}
				}
				else
				{
					if (IsEnumerableType(targetMember.Type, out var sourceElementType) &&
						IsEnumerableType(targetMember.Type, out var targetElementType))
					{
						// var sourceElementParam = Expression.Parameter(sourceElementType, "e");
						var sourceElementParam = (ParameterExpression)(typeof(PropertyCache<>).MakeGenericType(sourceElementType).GetField("SourceParameter").GetValue(null));
						targetValue = MakePredicateBody(targetElementType, sourceElementParam, childMembers, fields, depth + 1);

						var f = fields.FirstOrDefault(x => x.Path == memberName);
						Expression where = sourceMember;

						if (f != null)
						{
							try
							{
								if (f.Arguments.ContainsKey("where"))
								{
									string jsonString = JsonSerializer.Serialize(f.Arguments["where"]);
									var options = new JsonSerializerOptions();
									options.Converters.Add(new JsonStringEnumConverter());
									options.PropertyNameCaseInsensitive = true;
									IEnumerable<WhereExpression> w = JsonSerializer.Deserialize<IEnumerable<WhereExpression>>(jsonString, options);

									var m = typeof(ExpressionBuilderWhere<>).MakeGenericType(sourceElementType).GetMethod("BuildPredicate", new Type[] { typeof(IEnumerable<WhereExpression>) });
									Expression predicate = (Expression)m.Invoke(null, new[] { w });

									where = Expression.Call(
										typeof(Enumerable),
										nameof(Enumerable.Where),
										new Type[] { sourceElementType },
										where,
										predicate
									);
								}

							}
							catch
							{
								throw new($"Failed to execute Where on path \"{f.Path}\".");
							}

							try
							{
								if (f.Arguments.ContainsKey("orderBy"))
								{
									where = OrderBy(where, sourceElementType, (string)f.Arguments["orderBy"]);
								}

							}
							catch
							{
								throw new($"Failed to execute OrderBy on path \"{f.Path}\".");
							}

							try
							{
								if (f.Arguments.ContainsKey("skip"))
								{
									where = Skip(where, new Type[] { sourceElementType }, (int)f.Arguments["skip"]);
								}

							}
							catch
							{
								throw new($"Failed to execute Skip on path \"{f.Path}\".");
							}

							try
							{
								if (f.Arguments.ContainsKey("take"))
								{
									where = Take(where, new Type[] { sourceElementType }, (int)f.Arguments["take"]);
								}
							}
							catch
							{
								throw new($"Failed to execute Take on path \"{f.Path}\".");
							}
						}

						targetValue = Expression.Call(typeof(Enumerable), nameof(Enumerable.Select),
							new[] { sourceElementType, targetElementType }, where,
							Expression.Lambda(targetValue, sourceElementParam));

						targetValue = CorrectEnumerableResult(targetValue, targetElementType, targetMember.Type);
					}
					else
					{
						targetValue = Expression.Condition(
							Expression.Equal(sourceMember, Expression.Constant(null, sourceMember.Type)),
							Expression.Constant(null, sourceMember.Type),
							MakePredicateBody(targetMember.Type, sourceMember, childMembers, fields, depth + 1)
						);
					}
				}

				bindings.Add(Expression.Bind(targetMember.Member, targetValue));
			}
			return Expression.MemberInit(Expression.New(targetType), bindings);
		}

		static bool IsEnumerableType(Type type, out Type elementType)
		{
			foreach (var intf in type.GetInterfaces())
			{
				if (intf.IsGenericType && intf.GetGenericTypeDefinition() == typeof(IEnumerable<>))
				{
					elementType = intf.GetGenericArguments()[0];
					return true;
				}
			}

			elementType = null;
			return false;
		}

		static bool IsSameCollectionType(Type type, Type genericType, Type elementType)
		{
			var result = genericType.MakeGenericType(elementType).IsAssignableFrom(type);
			return result;
		}

		static Expression CorrectEnumerableResult(Expression enumerable, Type elementType, Type memberType)
		{
			if (memberType == enumerable.Type)
				return enumerable;

			if (memberType.IsArray)
				return Expression.Call(typeof(Enumerable), nameof(Enumerable.ToArray), new[] { elementType }, enumerable);

			if (IsSameCollectionType(memberType, typeof(List<>), elementType)
				|| IsSameCollectionType(memberType, typeof(ICollection<>), elementType)
				|| IsSameCollectionType(memberType, typeof(IReadOnlyList<>), elementType)
				|| IsSameCollectionType(memberType, typeof(IReadOnlyCollection<>), elementType))
				return Expression.Call(typeof(Enumerable), nameof(Enumerable.ToList), new[] { elementType }, enumerable);

			throw new NotImplementedException($"Not implemented transformation for type '{memberType.Name}'");
		}

		static Expression OrderBy(Expression source, Type type, string orderByProperty, bool desc, bool isThenBy = false)
		{
			var command = isThenBy ? (desc ? nameof(Enumerable.ThenByDescending) : nameof(Enumerable.ThenBy)) : (desc ? nameof(Enumerable.OrderByDescending) : nameof(Enumerable.OrderBy));

			var parameter = Expression.Parameter(type, "p");
			var propertyOrField = GetPropertyOrField(parameter.Type, orderByProperty);
			var sourceMember = Expression.PropertyOrField(parameter, propertyOrField.Name);

			return Expression.Call(
				typeof(Enumerable),
				command,
				new Type[] { type, sourceMember.Type },
				source,
				Expression.Lambda(sourceMember, parameter)
			);
		}

		static Expression OrderBy(Expression source, Type type, string sqlOrderByList)
		{
			var ordebyItems = sqlOrderByList.Trim().Split(',');
			Expression result = source;
			bool useThenBy = false;
			foreach (var item in ordebyItems)
			{
				var splt = item.Trim().Split(' ');
				result = OrderBy(result, type, splt[0].Trim(), (splt.Length > 1 && splt[1].Trim().ToLower() == "desc"), useThenBy);
				useThenBy = true;
			}
			return result;
		}

		static Expression Take(Expression source, Type[] types, int count)
		{
			return Expression.Call(
				typeof(Enumerable),
				nameof(Enumerable.Take),
				types,
				source,
				Expression.Constant(count, typeof(int))
			);
		}

		static Expression Skip(Expression source, Type[] types, int count)
		{
			return Expression.Call(
				typeof(Enumerable),
				nameof(Enumerable.Skip),
				types,
				source,
				Expression.Constant(count, typeof(int))
			);
		}

		static MemberInfo GetPropertyOrField(Type type, string name) {
			return type.GetProperty(name, bindingFlagsPublic) ?? (MemberInfo?)type.GetField(name, bindingFlagsPublic);
		}
	}
}
