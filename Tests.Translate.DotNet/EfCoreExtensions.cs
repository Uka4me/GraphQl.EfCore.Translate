using GraphQl.EfCore.Translate.DotNet;
using GraphQL;
using GraphQL.SystemTextJson;
using GraphQL.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tests.Translate.DotNet.Entity;
using Tests.Translate.DotNet.Types;

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
                _.Schema = new GraphQlSchema();
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
                _.Inputs = new Dictionary<string, object> {
                    { "where1", new List<object> {
                        new Dictionary<string, object>{
                            { "path", "Name"},
                            { "case", "IGNORE"},
                            { "value", new List<string> { "Company 1" } }
                        }
                    }},
                    { "where2", new List<object> {
                        new Dictionary<string, object>{
                            { "path", "Name"},
                            { "comparison", "STARTS_WITH"},
                            { "case", "IGNORE"},
                            { "value", new List<string> { "Person" }}
                        }
                    }}
                }.ToInputs();
            });

            Assert.AreEqual(executionResult.Errors?.Count ?? 0, 0);
        }

        private class GraphQlSchema : Schema
        {
            public GraphQlSchema()
            {
                Query = new Query();
            }
        }

        private class Query : ObjectGraphType<object>
        {
            public Query()
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
    }
}