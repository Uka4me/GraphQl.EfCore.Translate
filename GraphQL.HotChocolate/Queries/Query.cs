using Entity;
using Entity.Models;
using HotChocolate.Resolvers;
using GraphQl.EfCore.Translate.HotChocolate;
using Entity.Classes;
using System.Linq.Expressions;
using GraphQl.EfCore.Translate.Where.Graphs;
using GraphQl.EfCore.Translate;

namespace GraphQL.HotChocolate.Queries
{
    public class Query
    {
        public Query() {

            EfCoreExtensions.AddCalculatedField<Student, string>(
                x => x.CalculatedField,
                x => @"The ""calculatedField2"" field contains the number of evaluations equal to A for all its subjects"
            );
            EfCoreExtensions.AddCalculatedField<Student, int>(
                x => x.CalculatedField2,
                x => x.Enrollments.Count(e => e.Grade == Grade.A)
            );
        }

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
                CurrentPage = 1
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
