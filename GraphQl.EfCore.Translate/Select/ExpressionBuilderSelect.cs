using GraphQl.EfCore.Translate.Select.Graphs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GraphQl.EfCore.Translate
{
    static class ExpressionBuilderSelect<T>
    {
		public static ConcurrentDictionary<string, Func<Expression, Expression>> CalculatedFields = new();
		
		public static Func<Expression, Expression> AddCalculatedField(string path, Func<Expression, Expression> func) {
			return CalculatedFields.GetOrAdd(path, x => func);
		}

		public static Expression<Func<T, T>> BuildPredicate(List<NodeGraph> fields)
        {
			var param = PropertyCache<T>.SourceParameter;
			var body = MakePredicateBody(typeof(T), param, fields.Select(x => x.Path).Select(m => m.Split('.')), fields);
			return Expression.Lambda<Func<T, T>>(body, param);
		}

		static Expression MakePredicateBody(Type targetType, Expression source, IEnumerable<string[]> memberPaths, List<NodeGraph> fields, int depth = 0, string? keys = null)
		{
			String pathSource = null;
			Type typeSource = null;
			if (source is MemberExpression)
			{
				pathSource = GetPathProperty((MemberExpression)source);
				typeSource = GetTypeProperty((MemberExpression)source);
			}

			var bindings = new List<MemberBinding>();
			var target = Expression.Constant(null, targetType);
			foreach (var memberGroup in memberPaths.GroupBy(path => path[depth]))
			{
				Expression targetValue = null;
				String memberName = memberGroup.Key;
				String memberNameFull = string.IsNullOrWhiteSpace(keys) ? memberName : $"{keys}.{memberName}";
				var childMembers = memberGroup.Where(path => depth + 1 < path.Length).ToList();

				MemberExpression targetMember = GetMemberFromProperty(target.Type, memberName);
				MemberExpression sourceMember = GetMemberFromProperty(
					typeSource is null ? source.Type : typeSource,
					pathSource is null ? memberName : $"{pathSource}.{memberName}"
				);

				if (sourceMember is null) {
					continue;
				}

				if (targetMember.Member.GetCustomAttribute(typeof(NotMappedAttribute)) is not null) {
					targetValue = MakeCalculated(source, memberName);
				} else if (!childMembers.Any()) {
					targetValue = sourceMember;
				}
				else if (IsEnumerableType(targetMember.Type, out var sourceElementType) && IsEnumerableType(targetMember.Type, out var targetElementType))
				{
					targetValue = MakeCollection(targetElementType, sourceElementType, targetMember, sourceMember, childMembers, fields, depth, memberNameFull);
				}
				else
				{
					targetValue = MakeObject(targetMember, sourceMember, childMembers, fields, depth, memberNameFull);
				}

				if (targetValue is not null) {
					bindings.Add(Expression.Bind(targetMember.Member, targetValue));
				}
			}
			return Expression.MemberInit(Expression.New(targetType), bindings);
		}

		static Expression MakeCalculated(Expression source, string memberName) {
			var calculatedFields = (ConcurrentDictionary<string, Func<Expression, Expression>>)(
						typeof(ExpressionBuilderSelect<>)
							.MakeGenericType(source.Type)
							.GetField("CalculatedFields")
							.GetValue(null)
						);
			return calculatedFields.ContainsKey(memberName.ToLower()) ? calculatedFields[memberName.ToLower()]?.Invoke(source) : null;
		}

		static Expression MakeObject(MemberExpression targetMember, MemberExpression sourceMember, List<string[]> childMembers, List<NodeGraph> fields, int depth, string memberNameFull)
		{
			return Expression.Condition(
				Expression.Equal(sourceMember, Expression.Constant(null, sourceMember.Type)),
				Expression.Constant(null, sourceMember.Type),
				MakePredicateBody(targetMember.Type, sourceMember, childMembers, fields, depth + 1, memberNameFull)
			);
		}

		static Expression MakeCollection(Type targetElementType, Type sourceElementType, MemberExpression targetMember, MemberExpression sourceMember, List<string[]> childMembers, List<NodeGraph> fields, int depth, string memberNameFull)
		{
			var sourceElementParam = (ParameterExpression)(typeof(PropertyCache<>).MakeGenericType(sourceElementType).GetField("SourceParameter").GetValue(null));
			Expression targetValue = MakePredicateBody(targetElementType, sourceElementParam, childMembers, fields, depth + 1, memberNameFull);

			var field = fields.FirstOrDefault(x => x.Path == memberNameFull);
			Expression where = sourceMember;

			if (field != null)
			{
				try
				{
					if (field.Arguments.ContainsKey("where"))
					{
						var wh = field.Arguments["where"];
						string jsonString = JsonSerializer.Serialize(wh);
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
					throw new($"Failed to execute Where on path \"{field.Path}\".");
				}

				try
				{
					if (field.Arguments.ContainsKey("orderby"))
					{
						var m = typeof(ExpressionBuilderOrderBy<>).MakeGenericType(sourceElementType).GetMethod("BuildPredicate", new Type[] { typeof(Expression), typeof(string) });
						where = (Expression)m.Invoke(null, new object[] { where, field.Arguments["orderby"].ToString() });
						// where = OrderBy(where, sourceElementType, field.Arguments["orderby"].ToString());
					}

				}
				catch
				{
					throw new($"Failed to execute OrderBy on path \"{field.Path}\".");
				}

				try
				{
					if (field.Arguments.ContainsKey("skip"))
					{
						where = Skip(where, sourceElementType, int.Parse(field.Arguments["skip"].ToString()));
					}

				}
				catch
				{
					throw new($"Failed to execute Skip on path \"{field.Path}\".");
				}

				try
				{
					if (field.Arguments.ContainsKey("take"))
					{
						where = Take(where, sourceElementType, int.Parse(field.Arguments["take"].ToString()));
					}
				}
				catch
				{
					throw new($"Failed to execute Take on path \"{field.Path}\".");
				}
			}

			targetValue = Expression.Call(typeof(Enumerable), nameof(Enumerable.Select),
				new[] { sourceElementType, targetElementType }, where,
				Expression.Lambda(targetValue, sourceElementParam));

			return CorrectEnumerableResult(targetValue, targetElementType, targetMember.Type);
		}

		static MemberExpression GetMemberFromProperty(Type type, string path)
		{
			var method = typeof(PropertyCache<>).MakeGenericType(type).GetMethod("GetProperty", new Type[] { typeof(string) });
			var property = method.Invoke(null, new[] { path });
			return property is not null ? (MemberExpression)(typeof(Property<>).MakeGenericType(type).GetProperty("Left")).GetValue(property) : null;
		}

		static string GetPathProperty(MemberExpression source)
		{
			return source.Expression is MemberExpression ? $"{GetPathProperty((MemberExpression)source.Expression)}.{source.Member.Name}" : source.Member.Name;
		}

		static Type GetTypeProperty(MemberExpression source)
		{
			return source.Expression is MemberExpression ? GetTypeProperty((MemberExpression)source.Expression) : source.Expression.Type;
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

		static Expression Take(Expression source, Type type, int count)
		{
			return Expression.Call(
				typeof(Enumerable),
				nameof(Enumerable.Take),
				new Type[] { type },
				source,
				Expression.Constant(count, typeof(int))
			);
		}

		static Expression Skip(Expression source, Type type, int count)
		{
			return Expression.Call(
				typeof(Enumerable),
				nameof(Enumerable.Skip),
				new Type[] { type },
				source,
				Expression.Constant(count, typeof(int))
			);
		}
	}
}
