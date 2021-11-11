using Entity.Models;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphQL.DotNet.Types
{
    public sealed class EnrollmentObject : ObjectGraphType<Enrollment>
    {
        public EnrollmentObject()
        {
            Name = nameof(Enrollment);

            Field(m => m.EnrollmentID);
            Field(m => m.CourseID);
            Field(m => m.StudentID);
            Field<StringGraphType>("Test", resolve: m => "Привет");
            Field<GradeEnum>("Grade", resolve: w => w.Source.Grade);
            Field(
                    name: "Course",
                    type: typeof(CourseObject),
                    resolve: m => m.Source.Course);
            Field(
                    name: "Student",
                    type: typeof(StudentObject),
                    resolve: m => m.Source.Student);
        }
        
    }
}
