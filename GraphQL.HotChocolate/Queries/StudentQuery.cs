using Entity;
using Entity.Models;
using GraphQl.EfCore.Translate;
using HotChocolate;
using HotChocolate.Data;
using HotChocolate.Resolvers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphQL.HotChocolate.Queries
{
    public class StudentQuery
    {
        [UseDbContext(typeof(SchoolContext))]
        public List<Student> GetStudents([ScopedService] SchoolContext dbContext, IResolverContext context) {
            var n = dbContext.Students.ToList();
            var m = context.Selection;


            return n;
        }
    }
}
