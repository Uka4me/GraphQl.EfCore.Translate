using HotChocolate.Types;
using Tests.Translate.HotChocolate.Entity;

namespace Tests.Translate.HotChocolate.Types
{
    public class PersonObject : ObjectType<Person>
    {
        protected override void Configure(IObjectTypeDescriptor<Person> descriptor)
        {
            descriptor.Field(t => t.Name).Type<StringType>();
            descriptor.Field(t => t.Age).Type<IntType>();
            descriptor.Field(t => t.DateOfBirth).Type<DateTimeType>();
            descriptor.Field(t => t.Company).Type<CompanyObject>();
        }
    }
}
