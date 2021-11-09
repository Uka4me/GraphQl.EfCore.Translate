using System.Linq;
using GraphQl.EfCore.Translate;
using GraphQL.DotNet.Types;
using Microsoft.Extensions.DependencyInjection;
using Entity;
using GraphQL.Types;

namespace GraphQL.DotNet.Queries
{
	public partial class MainQuery
	{
		protected void InitializeCourseQuery()
		{
			Field<ListGraphType<CourseObject>>(
			  "Courses",
			  arguments: MainQuery.commonArguments,
			  resolve: context => {
				  using var scope = context.RequestServices.CreateScope();
				  var dbContext = scope.ServiceProvider.GetRequiredService<SchoolContext>();

				  var query = dbContext.Courses
							  .GraphQlWhere(context)
							  .GraphQlOrder(context)
							  .GraphQlPagination(context)
							  .GraphQlSelect(context);

				  return query.ToList();
			  }
			);
		}
	}
}
