﻿using Entity;
using Entity.Models;
using HotChocolate.Resolvers;
using GraphQl.EfCore.Translate.HotChocolate;
using Entity.Classes;
using GraphQl.EfCore.Translate.Where.Graphs;
using GraphQl.EfCore.Translate;
using Microsoft.EntityFrameworkCore;

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
            var query = dbContext.Students.AsNoTracking()
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
            return dbContext.Students.AsNoTracking()
                .GraphQlWhere(context)
                .GraphQlOrder(context)
                .GraphQlPagination(context)
                .GraphQlSelect(context)
                .ToList();
        }

        [UseDbContext(typeof(SchoolContext))]
        public List<Course> GetCourses([ScopedService] SchoolContext dbContext, IResolverContext context, int take = 0, int skip = 0, string orderBy = "", List<WhereExpression>? where = default)
        {
            return dbContext.Courses.AsNoTracking()
                .GraphQlWhere(context)
                .GraphQlOrder(context)
                .GraphQlPagination(context)
                .GraphQlSelect(context)
                .ToList();
        }

        [UseDbContext(typeof(SchoolContext))]
        public List<Enrollment> GetEnrollments([ScopedService] SchoolContext dbContext, IResolverContext context, int take = 0, int skip = 0, string orderBy = "", List<WhereExpression>? where = default)
        {
            return dbContext.Enrollments.AsNoTracking()
                .GraphQlWhere(context)
                .GraphQlOrder(context)
                .GraphQlPagination(context)
                .GraphQlSelect(context)
                .ToList();
        }
    }
}
