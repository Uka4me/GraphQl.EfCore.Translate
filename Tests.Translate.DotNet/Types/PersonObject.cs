using GraphQL.Types;
using Tests.Translate.DotNet.Entity;

namespace Tests.Translate.DotNet.Types
{
    public sealed class PersonObject : ObjectGraphType<Person>
    {
        public PersonObject()
        {
            Name = nameof(Person);

            Field(m => m.Name, nullable: true);
            Field(m => m.Age);
            Field(m => m.DateOfBirth);
            Field(
                name: "Company",
                type: typeof(CompanyObject),
                resolve: m => m.Source.Company);
        }
    }
}
