using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace GraphQl.EfCore.Translate
{
    static class ExpressionBuilderOrderBy<T>
    {
        public static Expression BuildPredicate(Expression source, string orderByProperty, bool desc, bool isThenBy = false)
        {
			var command = isThenBy ? (desc ? nameof(Enumerable.ThenByDescending) : nameof(Enumerable.ThenBy)) : (desc ? nameof(Enumerable.OrderByDescending) : nameof(Enumerable.OrderBy));

			ParameterExpression parameter = (ParameterExpression)(typeof(PropertyCache<>).MakeGenericType(typeof(T)).GetField("SourceParameter").GetValue(null));
			MemberExpression sourceMember = (MemberExpression)PropertyCache<T>.GetProperty(orderByProperty).Left;

			return Expression.Call(
				typeof(Enumerable),
				command,
				new Type[] { typeof(T), sourceMember.Type },
				source,
				Expression.Lambda(sourceMember, parameter)
			);
		}

		public static Expression BuildPredicate(Expression source, string sqlOrderByList)
		{
			var ordebyItems = sqlOrderByList.Trim().Split(',');
			Expression result = source;
			bool useThenBy = false;
			foreach (var item in ordebyItems)
			{
				var splt = item.Trim().Split(' ');
				result = BuildPredicate(result, splt[0].Trim(), (splt.Length > 1 && splt[1].Trim().ToLower() == "desc"), useThenBy);
				useThenBy = true;
			}
			return result;
		}
	}
}
