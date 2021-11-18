using GraphQl.EfCore.Translate.HotChocolate;
using GraphQl.EfCore.Translate.Where.Graphs;
using HotChocolate;
using HotChocolate.Execution;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tests.Translate.HotChocolate
{
    [TestClass]
    public class EfCoreExtensions
    {
        [TestMethod]
        public async Task CommonQuery()
        {
            IRequestExecutor executor =
                await new ServiceCollection()
                    .AddGraphQL()
                    .AddQueryType<Query>()
                    .AddType<CaseStringGraph>()
                    .AddType<WhereExpressionGraph>()
                    .AddType<ComparisonGraph>()
                    .AddType<ConnectorGraph>()
                    .AddType<CompanyObject>()
                    .AddType<PersonObject>()
                    .Services
                    .BuildServiceProvider()
                    .GetRequestExecutorAsync();

            IExecutionResult executionResult = await executor
                .ExecuteAsync(@"
                fragment personData on Person {
                  name
                  age
                }
                query($where1: [WhereExpression!], $where2: [WhereExpression!]) {
                    request(
                        skip: 1,
                        take: 2,
                        orderBy: ""Name desc""
                        where: $where1
                    ){
                        name
                        employees(
                            skip: 2,
                            take: 1,
                            orderBy: ""Name""
                            where: $where2
                        ){
                            ...personData
                            company{
                                name
                            }
                        }
                    }
                }", (IReadOnlyDictionary<string, object?>)new Dictionary<string, object?> {
                    { "where1", new List<object> { 
                        new Dictionary<string, object?>{
                            { "path", "Name"},
                            { "case", "IGNORE"},
                            { "value", new List<string> { "Company 1" } }
                        }
                    }},
                    { "where2", new List<object> {
                        new Dictionary<string, object?>{
                            { "path", "Name"},
                            { "comparison", "STARTS_WITH"},
                            { "case", "IGNORE"},
                            { "value", new List<string> { "Person" }}
                        }
                    }}
                });

            Assert.AreEqual(executionResult.Errors?.Count ?? 0, 0);
        }

        public class Query
        {
            public List<Company> GetRequest(IResolverContext context, int take = 0, int skip = 0, string orderBy = "", List<WhereExpression>? where = default)
            {
                return companies.AsQueryable().GraphQl(context).ToList();
            }

            readonly List<Company> companies = new()
            {
                new()
                {
                    Name = "Company 1",
                    Employees = new List<Person>
                    {
                        new()
                        {
                            Name = "Person 1",
                            Age = 12,
                            DateOfBirth = new(1999, 10, 10, 10, 10, 10, DateTimeKind.Utc)
                        },
                        new()
                        {
                            Name = "Person 2",
                            Age = 12,
                            DateOfBirth = new(2001, 10, 10, 10, 10, 10, DateTimeKind.Utc)
                        },
                        new()
                        {
                            Name = null,
                            Age = 11,
                            DateOfBirth = new(2000, 10, 10, 10, 10, 10, DateTimeKind.Utc)
                        }
                    }
                },

                new()
                {
                    Name = "Company 2",
                    Employees = new List<Person>
                    {
                        new()
                        {
                            Name = "Person 3",
                            Age = 34,
                            DateOfBirth = new(1977, 10, 11, 10, 10, 10, DateTimeKind.Utc)
                        },
                        new()
                        {
                            Name = "Person 3",
                            Age = 31,
                            DateOfBirth = new(1980, 10, 11, 10, 10, 10, DateTimeKind.Utc)
                        },
                    }
                }
            };
        }

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

        public class Company
        {
            public string? Name { get; set; }
            public IList<Person> Employees { get; set; } = null!;
        }

        public class Person
        {
            public string? Name { get; set; }
            public int Age { get; set; }
            public Company? Company { get; set; }
            public DateTime DateOfBirth { get; set; }
        }
    }
}