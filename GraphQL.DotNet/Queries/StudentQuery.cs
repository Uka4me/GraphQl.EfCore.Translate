using System.Linq;
using GraphQL.DotNet.Types;
using Microsoft.Extensions.DependencyInjection;
using Entity;
using GraphQL.Types;
using System.Collections.Generic;
using Entity.Models;
using GraphQl.EfCore.Translate.DotNet;
using Microsoft.EntityFrameworkCore;
using Entity.Classes;

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

			Field<PageInfoObject<StudentObject, Student>>(
			  "pageStudents",
			  arguments: MainQuery.commonArguments,
			  resolve: context => {
				  using var scope = context.RequestServices.CreateScope();
				  var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<SchoolContext>>();
				  SchoolContext dbContext = dbContextFactory.CreateDbContext();

				  var query = dbContext.Students
							  .GraphQlWhere(context).AsQueryable();

				  var total = query.Count();

				  query = query.GraphQlOrder(context)
							  .GraphQlPagination(context)
							  .GraphQlSelect(context, "Data");

				  return new PageInfo<Student> { 
					Total = total,
					CurrentPage = 1,
					Data = query.ToList()
				  };
			  }
			);
		}
	}
}
