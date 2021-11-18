using GraphQl.EfCore.Translate.DotNet;
using GraphQL;
using GraphQL.Server;
using GraphQL.SystemTextJson;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tests.Translate.DotNet
{
    [TestClass]
    public class EfCoreExtensions
    {
        [TestMethod]
        public async Task CommonQuery()
        {
            var documentExecuter = new DocumentExecuter();
            var executionResult = await documentExecuter.ExecuteAsync(_ =>
            {
                _.Schema = new Issue2275Schema();
                _.Query = @"
                fragment personData on Person {
                  name
                  age
                }
                query($where1: [WhereExpression], $where2: [WhereExpression]) {
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
                }";
                _.Inputs = @" {
                    ""where1"": [
                        { ""path"": ""Name"", ""case"": ""IGNORE"", ""value"": ""Company 1"" }
                    ],
                    ""where2"": [
                        { ""path"": ""Name"", ""comparison"": ""STARTS_WITH"", ""case"": ""IGNORE"", ""value"": ""Person"" }
                    ]
                }".ToInputs();
            });

            Assert.AreEqual(executionResult.Errors?.Count ?? 0, 0);
        }

        private class Issue2275Schema : Schema
        {
            public Issue2275Schema()
            {
                Query = new Issue2275Query();
            }
        }

        private class Issue2275Query : ObjectGraphType<object>
        {
            public Issue2275Query()
            {
                Field<ListGraphType<CompanyObject>>(
                  "request",
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
                  resolve: context => {
                      return companies.AsQueryable().GraphQl(context).ToList();
                  }
              );
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
                    // arguments: MainQuery.commonArguments,
                    resolve: m => m.Source.Company);
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