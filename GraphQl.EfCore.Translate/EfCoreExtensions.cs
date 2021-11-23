using GraphQl.EfCore.Translate.Select.Graphs;
using GraphQl.EfCore.Translate.Where.Graphs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace GraphQl.EfCore.Translate
{
	public static class EfCoreExtensions
	{
		public static void AddCalculatedField<TSource>(Expression<Func<TSource, object>> selector, Func<Expression, Expression> func)
		{
			string name = GetNameProperty<TSource>(selector);
			ExpressionBuilderSelect<TSource>.AddCalculatedField(name.ToLower(), func);
		}

		public static void AddCalculatedField<TSource, TValue>(
			Expression<Func<TSource, TValue>> selector,
			Expression<Func<TSource, TValue>> func
		) where TSource : class
		{
			string name = GetNameProperty<TSource>(selector);

			ExpressionBuilderSelect<TSource>.AddCalculatedField(name.ToLower(), (source) => {
				var parameter = (ParameterExpression)source;
				return Expression.Lambda(Expression.Invoke(func, parameter), parameter).Body;
			});
		}

		static string GetNameProperty<TSource>(Expression selector) {
			var body = (selector as LambdaExpression).Body as MemberExpression;
			string name = body.Member.Name;

			if (body.Member.ReflectedType != typeof(TSource))
			{
				throw new ArgumentException($@"AddCalculatedField: The property ({name}) must be of class ""{typeof(TSource).Name}""");
			}

			return name;
		}

		public static IQueryable<T> GraphQlSelect<T>(IQueryable<T> queryable, List<NodeGraph> fields)
		{
			var lambdaSelect = ExpressionBuilderSelect<T>.BuildPredicate(fields);
			return queryable.Select(lambdaSelect);
		}

		public static IQueryable<T> GraphQlWhere<T>(IQueryable<T> queryable, List<WhereExpression> wheres)
		{
			var predicate = ExpressionBuilderWhere<T>.BuildPredicate(wheres);
			return queryable.Where(predicate);
		}

		public static IQueryable<T> GraphQlPagination<T>(IQueryable<T> queryable, int skip = 0, int? take = null)
		{
			queryable = queryable.Skip(skip);

			if (take is not null)
			{
				queryable = queryable.Take((int)take);
			}

			return queryable;
		}

		public static IQueryable<T> GraphQlOrder<T>(IQueryable<T> queryable, string order = "")
		{
			if (!string.IsNullOrWhiteSpace(order))
			{
				queryable = queryable.OrderBy(order);
			}

			return queryable;
		}

		static IQueryable<T> OrderBy<T>(this IQueryable<T> source, string orderByProperty, bool desc, bool isThenBy = false)
		{
			string command = isThenBy ? (desc ? "ThenByDescending" : "ThenBy") : (desc ? "OrderByDescending" : "OrderBy");
			var parameter = PropertyCache<T>.SourceParameter;
			var property = PropertyCache<T>.GetProperty(orderByProperty);
			var orderByExpression = Expression.Lambda(property.Left, parameter);
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
	}
}
