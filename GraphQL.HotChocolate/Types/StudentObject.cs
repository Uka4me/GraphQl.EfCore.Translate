using Entity.Models;
using GraphQl.EfCore.Translate.HotChocolate;

namespace GraphQL.HotChocolate.Types
{
    public class StudentObject : ObjectType<Student>
    {
        protected override void Configure(IObjectTypeDescriptor<Student> descriptor)
        {
            descriptor.Field(t => t.ID).Type<IntType>();
            descriptor.Field(t => t.LastName).Type<StringType>();
            descriptor.Field(t => t.FirstMidName).Type<StringType>();
            descriptor.Field(t => t.EnrollmentDate).Type<DateTimeType>();
            descriptor.Field("test").Type<StringType>().Resolve(x => "Привет");
            descriptor.Field(t => t.Enrollments).Type<ListType<EnrollmentObject>>()
                .Argument("take", a => a.Type<IntType>())
                .Argument("skip", a => a.Type<IntType>())
                .Argument("orderBy", a => a.Type<StringType>())
                .Argument("where", a => a.Type<ListType<WhereExpressionGraph>>());
        }
    }
}
