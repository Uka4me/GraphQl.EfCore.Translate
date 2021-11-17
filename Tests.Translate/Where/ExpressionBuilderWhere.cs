using GraphQl.EfCore.Translate;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests.Translate.Where
{
    [TestClass]
    public class ExpressionBuilderWhere
    {
        [TestMethod]
        [DataRow("Employees.Count", Comparison.Equal, "3", "Company 1")]
        [DataRow("Employees.Count", Comparison.LessThan, "3", "Company 2")]
        [DataRow("Employees.Count", Comparison.LessThanOrEqual, "2", "Company 2")]
        [DataRow("Employees.Count", Comparison.GreaterThan, "2", "Company 1")]
        [DataRow("Employees.Count", Comparison.GreaterThanOrEqual, "3", "Company 1")]
        [DataRow("Employees[Name]", Comparison.Equal, "Person 1", "Company 1")]
        [DataRow("Employees[Name]", Comparison.Equal, "Person 3", "Company 1", true)]
        [DataRow("Employees[Name]", Comparison.Contains, "son 2", "Company 1")]
        [DataRow("Employees[Name]", Comparison.StartsWith, "Person 2", "Company 1")]
        [DataRow("Employees[Name]", Comparison.EndsWith, "son 2", "Company 1")]
        [DataRow("Employees[Name]", Comparison.EndsWith, "person 2", "Company 1", false, CaseString.Ignore)]
        [DataRow("Employees[Age]", Comparison.Equal, "12", "Company 1")]
        [DataRow("Employees[Age]", Comparison.GreaterThan, "12", "Company 2")]
        [DataRow("Employees[Age]", Comparison.Equal, "12", "Company 2", true)]
        [DataRow("Employees[Age]", Comparison.GreaterThanOrEqual, "31", "Company 2")]
        [DataRow("Employees[Age]", Comparison.LessThan, "13", "Company 1")]
        [DataRow("Employees[Age]", Comparison.LessThanOrEqual, "12", "Company 1")]
        [DataRow("Employees[DateOfBirth]", Comparison.Equal, "2001-10-10T10:10:10+00:00", "Company 1")]
        [DataRow("Employees[DateOfBirth.Day]", Comparison.Equal, "11", "Company 2")]
        [DataRow("Employees[Company.Employees[Name]]", Comparison.Contains, "son 2", "Company 1")]
        public void ListComparison(string name, Comparison expression, string value, string expectedName, bool negate = false, CaseString? stringComparison = null) {
            ListMembers(name, expression, value, expectedName, negate, stringComparison);
        }

        [TestMethod]
        [DataRow("Employees[Name]", Comparison.In, "Person 1", "Company 1", false, CaseString.Original)]
        [DataRow("Employees[Name]", Comparison.In, "PeRSoN 1", "Company 1", false, CaseString.Ignore)]
        [DataRow("Employees[Name]", Comparison.Equal, "Person 3", "Company 1", true, CaseString.Original)]
        [DataRow("Employees[Name]", Comparison.Equal, "PeRSoN 3", "Company 1", true, CaseString.Ignore)]
        [DataRow("Employees[Name]", Comparison.StartsWith, "Person 2", "Company 1", false, CaseString.Original)]
        [DataRow("Employees[Name]", Comparison.StartsWith, "PeRSoN 2", "Company 1", false, CaseString.Ignore)]
        [DataRow("Employees[Name]", Comparison.EndsWith, "son 2", "Company 1", false, CaseString.Original)]
        [DataRow("Employees[Name]", Comparison.EndsWith, "SoN 2", "Company 1", false, CaseString.Ignore)]
        [DataRow("Employees[Name]", Comparison.Contains, "son 2", "Company 1", false, CaseString.Original)]
        [DataRow("Employees[Name]", Comparison.Contains, "SoN 2", "Company 1", false, CaseString.Ignore)]
        [DataRow("Employees[Name]", Comparison.IndexOf, "son 2", "Company 1", false, CaseString.Original)]
        [DataRow("Employees[Name]", Comparison.IndexOf, "SoN 2", "Company 1", false, CaseString.Ignore)]
        [DataRow("Employees[Name]", Comparison.Equal, null, "Company 1", false, CaseString.Original)]
        [DataRow("Employees[Name]", Comparison.Equal, null, "Company 1", false, CaseString.Ignore)]
        public void ListCase(string name, Comparison expression, string? value, string expectedName, bool negate = false, CaseString? stringComparison = null)
        {
            ListMembers(name, expression, value, expectedName, negate, stringComparison);
        }

        void ListMembers(string name, Comparison expression, string? value, string expectedName, bool negate = false, CaseString? stringComparison = null) {
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

            foreach (var company in companies)
            {
                foreach (var employee in company.Employees)
                {
                    employee.Company = company;
                }
            }

            var result = companies.AsQueryable()
                .Where(ExpressionBuilderWhere<Company>.BuildPredicate(name, expression, value is null ? null : new[] { value }, negate, stringComparison))
                .Single();
            Assert.AreEqual(expectedName, result.Name);
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
