using Entity.Models;
using GraphQL.DotNet.Queries;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphQL.DotNet.Types
{
    public sealed class StudentObject : ObjectGraphType<Student>
    {
        public StudentObject()
        {
            Name = nameof(Student);

            Field(m => m.ID);
            Field(m => m.LastName);
            Field(m => m.FirstMidName);
            Field(m => m.EnrollmentDate);
            Field(
                name: "Enrollments",
                type: typeof(ListGraphType<EnrollmentObject>),
                arguments: MainQuery.commonArguments,
                resolve: m => m.Source.Enrollments);
        }

    }
}
