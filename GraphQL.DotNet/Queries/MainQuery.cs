using Entity.Models;
using GraphQl.EfCore.Translate.DotNet;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace GraphQL.DotNet.Queries
{
    public partial class MainQuery : ObjectGraphType
    {
		public static QueryArguments commonArguments =
		new QueryArguments(new List<QueryArgument>
		{
			new QueryArgument<IntGraphType>
			{
				Name = "take"
			},
			new QueryArgument<IntGraphType>
			{
				Name = "skip"
			},
			new QueryArgument<StringGraphType>
			{
				Name = "orderBy"
			},
			new QueryArgument<ListGraphType<WhereExpressionGraph>>
			{
				Name = "where"
			}
		});

		public MainQuery()
		{
			Name = "MainQuery";

			InitializeCourseQuery();
			InitializeEnrollmentQuery();
			InitializeStudentQuery();

			EfCoreExtensionsDotNet.AddCalculatedField<Student>("CalculatedField", (source) => {
				return Expression.Constant("The \"calculatedField2\" field contains the number of evaluations equal to A for all its subjects", typeof(string));
			});
			EfCoreExtensionsDotNet.AddCalculatedField<Student>("CalculatedField2", (source) => {
				var parameter = (ParameterExpression)source;
				Expression<Func<Student, int>> func = x => x.Enrollments.Count(e => e.Grade == Grade.A);
				return Expression.Lambda(Expression.Invoke(func, parameter), parameter).Body;
			});
		}
	}
}
