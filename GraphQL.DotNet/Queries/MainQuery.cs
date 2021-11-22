using Entity.Models;
using GraphQl.EfCore.Translate.DotNet;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using GraphQl.EfCore.Translate;

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

			EfCoreExtensions.AddCalculatedField<Student, string>(
				x => x.CalculatedField,
				x => @"The ""calculatedField2"" field contains the number of evaluations equal to A for all its subjects"
			);
			EfCoreExtensions.AddCalculatedField<Student, int>(
				x => x.CalculatedField2,
				x => x.Enrollments.Count(e => e.Grade == Grade.A)
			);
		}
	}
}
