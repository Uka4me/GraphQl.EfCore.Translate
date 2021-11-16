using GraphQl.EfCore.Translate;
using GraphQl.EfCore.Translate.Select.Graphs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests.Translate.Select
{
    [TestClass]
    public class ExpressionBuilderSelect
    {
        [TestMethod]
        [DataRow("Name")]
        [DataRow("Name", "Employees.Name", "Employees.Age")]
        [DataRow("Name", "Employees.Name", "Employees.Age", "Employees.Company.Name")]
        public void ListMembers(params string[] fields) {
            List<Company> companies = new()
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
                        Name = "Person 4",
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

            foreach (var company in companies)
            {
                foreach (var employee in company.Employees)
                {
                    employee.Company = company;
                }
            }

            List<NodeGraph> paramsFields = new List<NodeGraph>();

            foreach (var field in fields)
            {
                paramsFields.Add(new NodeGraph { 
                    Path = field
                });
            }

            try
            {
                var result = companies.AsQueryable()
                    .Select(ExpressionBuilderSelect<Company>.BuildPredicate(paramsFields))
                    .ToList();
            }
            catch {
                Assert.Fail();
            }
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
