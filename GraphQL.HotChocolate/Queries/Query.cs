using Entity;
using Entity.Models;
using HotChocolate.Resolvers;
using GraphQl.EfCore.Translate.HotChocolate;
using GraphQl.EfCore.Translate;
using Entity.Classes;

namespace GraphQL.HotChocolate.Queries
{
    public class Query
    {
        [UseDbContext(typeof(SchoolContext))]
        public PageInfo<Student> GetPageStudents([ScopedService] SchoolContext dbContext, IResolverContext context, int take = 0, int skip = 0, string orderBy = "", List<WhereExpression>? where = default)
        {
            var query = dbContext.Students
                .GraphQlWhere(context).AsQueryable();

            var total = query.Count();

            query = query.GraphQlOrder(context)
                .GraphQlPagination(context)
                .GraphQlSelect(context, "Data");


            return new PageInfo<Student> {
                Total = total,
                Data = query.ToList(),
                CurrentPage = 1/*skip / take*/ //((int)Math.Ceiling((decimal)total / (decimal)take))
            };
        }

        [UseDbContext(typeof(SchoolContext))]
        public List<Student> GetStudents([ScopedService] SchoolContext dbContext, IResolverContext context, int take = 0, int skip = 0, string orderBy = "", List<WhereExpression>? where = default) {
            var n = dbContext.Students
                .GraphQlWhere(context)
                .GraphQlOrder(context)
                .GraphQlPagination(context)
                .GraphQlSelect(context)
                .ToList();


            return n;
        }

        [UseDbContext(typeof(SchoolContext))]
        public List<Course> GetCourses([ScopedService] SchoolContext dbContext, IResolverContext context, int take = 0, int skip = 0, string orderBy = "", List<WhereExpression>? where = default)
        {
            var n = dbContext.Courses
                .GraphQlWhere(context)
                .GraphQlOrder(context)
                .GraphQlPagination(context)
                .GraphQlSelect(context)
                .ToList();


            return n;
        }

        [UseDbContext(typeof(SchoolContext))]
        public List<Enrollment> GetEnrollments([ScopedService] SchoolContext dbContext, IResolverContext context, int take = 0, int skip = 0, string orderBy = "", List<WhereExpression>? where = default)
        {
            var n = dbContext.Enrollments
                .GraphQlWhere(context)
                .GraphQlOrder(context)
                .GraphQlPagination(context)
                .GraphQlSelect(context)
                .ToList();


            return n;
        }
    }
}
