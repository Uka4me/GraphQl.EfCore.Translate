using System.Linq;
using GraphQL.DotNet.Types;
using Microsoft.Extensions.DependencyInjection;
using Entity;
using GraphQL.Types;
using GraphQl.EfCore.Translate.DotNet;

namespace GraphQL.DotNet.Queries
{
	public partial class MainQuery
	{
		protected void InitializeEnrollmentQuery()
		{
			Field<ListGraphType<EnrollmentObject>>(
			  "Enrollments",
			  arguments: MainQuery.commonArguments,
			  resolve: context => {
				  using var scope = context.RequestServices.CreateScope();
				  var dbContext = scope.ServiceProvider.GetRequiredService<SchoolContext>();

				  var query = dbContext.Enrollments
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
