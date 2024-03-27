using GraphQl.EfCore.Translate.HotChocolate;
using GraphQl.EfCore.Translate.Where.Graphs;
using HotChocolate;
using HotChocolate.Execution;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using HotChocolate.Types.Relay;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tests.Translate.HotChocolate.Entity;
using Tests.Translate.HotChocolate.Types;

namespace Tests.Translate.HotChocolate
{
    [TestClass]
    public class EfCoreExtensions
    {
        private readonly IRequestExecutor documentExecuter;

        public EfCoreExtensions()
        {
            documentExecuter = new ServiceCollection()
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
                    .GetRequestExecutorAsync().GetAwaiter().GetResult();
        }

        [TestMethod]
        public async Task CommonQuery()
        {
            QueryResult executionResult = (QueryResult)await documentExecuter.ExecuteAsync(@"
                fragment personData on Person {
                  name
                  age
                }
                query($where1: [WhereExpression!], $where2: [WhereExpression!]) {
                    request(
                        skip: 0,
                        take: 2,
                        orderBy: ""Name desc""
                        where: $where1
                    ){
                        name
                        employees(
                            skip: 0,
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
                }", new Dictionary<string, object?> {
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

            AssertSuccessResult(
                executionResult,
                """
                {
                  "data": {
                    "request": [
                      {
                        "name": "Company 1",
                        "employees": [
                          {
                            "name": "Person 1",
                            "age": 12,
                            "company": null
                          }
                        ]
                      }
                    ]
                  }
                }
                """
            );
        }

        [TestMethod]
        public async Task EmptyQuery()
        {
            QueryResult executionResult = (QueryResult)await documentExecuter.ExecuteAsync(
                """
                    query {
                        request {
                            name
                        }
                    }
                """
                );

            AssertSuccessResult(
                executionResult,
                """
                {
                  "data": {
                    "request": [
                      {
                        "name": "Company 1"
                      },
                      {
                        "name": "Company 2"
                      }
                    ]
                  }
                }
                """
            );
        }

        private void AssertSuccessResult(QueryResult executionResult, string expected)
        {
            Assert.AreEqual(0, executionResult.Errors?.Count ?? 0);
            Assert.AreEqual(expected, executionResult.ToJson());
        }

        public class Query
        {
            public List<Company> GetRequest(IResolverContext context, int? take = null, int? skip = null, string? orderBy = null, List<WhereExpression>? where = default)
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
    }
}