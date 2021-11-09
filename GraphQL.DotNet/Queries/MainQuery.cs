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
				Name = "Take"
			},
			new QueryArgument<IntGraphType>
			{
				Name = "Skip"
			},
			new QueryArgument<StringGraphType>
			{
				Name = "OrderBy"
			},
			new QueryArgument<ListGraphType<WhereExpressionGraph>>
			{
				Name = "Where"
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
