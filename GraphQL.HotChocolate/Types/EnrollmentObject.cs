using Entity.Models;

namespace GraphQL.HotChocolate.Types
{
    public class EnrollmentObject : ObjectType<Enrollment>
    {
        protected override void Configure(IObjectTypeDescriptor<Enrollment> descriptor)
        {
            descriptor.Field(t => t.EnrollmentID).Type<IntType>();
            descriptor.Field(t => t.CourseID).Type<IntType>();
            descriptor.Field(t => t.StudentID).Type<IntType>();
            descriptor.Field(t => t.Grade).Type<GradeEnum>();
            descriptor.Field(t => t.Course).Type<CourseObject>();
            descriptor.Field(t => t.Student).Type<StudentObject>();
            descriptor.Field("test").Type<StringType>().Resolve(x => "Привет");
        }
    }
}
