using System.Linq;
using GraphQl.EfCore.Translate;
using GraphQL.DotNet.Types;
using Microsoft.Extensions.DependencyInjection;
using Entity;
using GraphQL.Types;
using System.Collections.Generic;
using Entity.Models;

namespace GraphQL.DotNet.Queries
{
	public partial class MainQuery
	{
		protected void InitializeStudentQuery()
		{
			Field<ListGraphType<StudentObject>>(
			  "Students",
			  arguments: MainQuery.commonArguments,
			  resolve: context => {
				  using var scope = context.RequestServices.CreateScope();
				  var dbContext = scope.ServiceProvider.GetRequiredService<SchoolContext>();

				  var query = dbContext.Students
							  .GraphQlWhere(context)
							  .GraphQlOrder(context)
							  .GraphQlPagination(context)
							  .GraphQlSelect(context);

				  /*List<Grade?> t = new List<Grade?>() { Grade.A, Grade.B };
				  var query1 = dbContext.Enrollments.Where(x => t.Contains(x.Grade));*/

				  return query.ToList();
			  }
			);
		}
	}
}
