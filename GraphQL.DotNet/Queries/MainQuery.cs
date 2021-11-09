using GraphQl.EfCore.Translate;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Linq;
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
		}
	}
}
