using System.Linq;
using GraphQL.DotNet.Types;
using Microsoft.Extensions.DependencyInjection;
using Entity;
using GraphQL.Types;
using System.Collections.Generic;
using Entity.Models;
using GraphQl.EfCore.Translate.DotNet;
using Microsoft.EntityFrameworkCore;

namespace GraphQL.DotNet.Queries
{
	public partial class MainQuery
	{
		protected void InitializeStudentQuery()
		{
			Field<ListGraphType<StudentObject>>(
			  "students",
			  arguments: MainQuery.commonArguments,
			  resolve: context => {
				  using var scope = context.RequestServices.CreateScope();
				  var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<SchoolContext>>();
				  SchoolContext dbContext = dbContextFactory.CreateDbContext();

				  var query = dbContext.Students
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
