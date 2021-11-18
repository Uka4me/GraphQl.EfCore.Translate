using GraphQl.EfCore.Translate.DotNet;
using GraphQL.Types;
using System.Collections.Generic;
using Tests.Translate.DotNet.Entity;

namespace Tests.Translate.DotNet.Types
{
    public sealed class CompanyObject : ObjectGraphType<Company>
    {
        public CompanyObject()
        {
            Name = nameof(Company);

            Field(m => m.Name, nullable: true);
            Field(
                name: "Employees",
                type: typeof(ListGraphType<PersonObject>),
                arguments: new QueryArguments(new List<QueryArgument>
                {
                        new QueryArgument<IntGraphType>
                        {
                            Name = "take"
                        },
                        new QueryArgument<IntGraphType>
                        {
                            Name = "skip"
                        },
                        new QueryArgument<StringGraphType>
                        {
                            Name = "orderBy"
                        },
                        new QueryArgument<ListGraphType<WhereExpressionGraph>>
                        {
                            Name = "where"
                        }
                }),
                resolve: m => m.Source.Employees);
        }
    }
}
