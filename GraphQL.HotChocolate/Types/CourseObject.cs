using Entity.Models;
using GraphQl.EfCore.Translate.HotChocolate.Graphs;

namespace GraphQL.HotChocolate.Types
{
    public class CourseObject : ObjectType<Course>
    {
        protected override void Configure(IObjectTypeDescriptor<Course> descriptor)
        {
            descriptor.Field(t => t.CourseID).Type<IntType>();
            descriptor.Field(t => t.Title).Type<StringType>();
            descriptor.Field(t => t.Credits).Type<IntType>();
            descriptor.Field(t => t.Enrollments).Type<ListType<EnrollmentObject>>()
                .Argument("take", a => a.Type<IntType>())
                .Argument("skip", a => a.Type<IntType>())
                .Argument("orderBy", a => a.Type<StringType>())
                .Argument("where", a => a.Type<ListType<WhereExpressionGraph>>());
        }
    }
}
