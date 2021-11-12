using Entity;
using Entity.Models;
using HotChocolate.Resolvers;
using GraphQl.EfCore.Translate.HotChocolate;

namespace GraphQL.HotChocolate.Queries
{
    public class StudentQuery
    {
        [UseDbContext(typeof(SchoolContext))]
        public List<Student> GetStudents([ScopedService] SchoolContext dbContext, IResolverContext context, int take = 0, int skip = 0, string orderBy = "", List<GraphQl.EfCore.Translate.WhereExpression> where = null) {
            var n = dbContext.Students
                .GraphQlWhere(context)
                .GraphQlOrder(context)
                .GraphQlPagination(context)
                .GraphQlSelect(context)
                .ToList();


            return n;
        }
    }
}
