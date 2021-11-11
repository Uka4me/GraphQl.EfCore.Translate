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
    public sealed class CourseObject : ObjectGraphType<Course>
    {
        public CourseObject()
        {
            Name = nameof(Course);

            Field(m => m.CourseID);
            Field(m => m.Title);
            Field(m => m.Credits);
            Field<StringGraphType>("Test", resolve: m => "Привет");
            Field(
                name: "Enrollments",
                type: typeof(ListGraphType<EnrollmentObject>),
                arguments: MainQuery.commonArguments,
                resolve: m => m.Source.Enrollments);
        }
    }
}
