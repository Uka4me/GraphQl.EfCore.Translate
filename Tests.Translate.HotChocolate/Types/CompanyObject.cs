using GraphQl.EfCore.Translate.HotChocolate;
using HotChocolate.Types;
using Tests.Translate.HotChocolate.Entity;

namespace Tests.Translate.HotChocolate.Types
{
    public class CompanyObject : ObjectType<Company>
    {
        protected override void Configure(IObjectTypeDescriptor<Company> descriptor)
        {
            descriptor.Field(t => t.Name).Type<StringType>();
            descriptor.Field(t => t.Employees).Type<ListType<PersonObject>>()
                .Argument("take", a => a.Type<IntType>())
                .Argument("skip", a => a.Type<IntType>())
                .Argument("orderBy", a => a.Type<StringType>())
                .Argument("where", a => a.Type<ListType<WhereExpressionGraph>>());
        }
    }
}
