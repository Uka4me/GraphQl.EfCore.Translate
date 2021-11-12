using System.Linq;
using GraphQL.DotNet.Types;
using Microsoft.Extensions.DependencyInjection;
using Entity;
using GraphQL.Types;
using GraphQl.EfCore.Translate.DotNet;
using Microsoft.EntityFrameworkCore;

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
				  var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<SchoolContext>>();
				  SchoolContext dbContext = dbContextFactory.CreateDbContext();

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
