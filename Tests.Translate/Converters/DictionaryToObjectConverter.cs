using GraphQl.EfCore.Translate;
using GraphQl.EfCore.Translate.Converters;
using GraphQl.EfCore.Translate.Where.Graphs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests.Translate.Converters
{
    [TestClass]
    public class DictionaryToObjectConverterTests
    {
        [TestMethod]
        public void ConvertList()
        {
            var list = new List<object> {
                new Dictionary<string, object>{
                    { "Path", "Title"},
                    { "Negate", "true"},
                    { "Case", "IGNORE"},
                    { "Comparison", "LESS_THAN_OR_EQUAL"},
                    { "Connector", "AND"},
                    { "Value", new List<object>{ "1", "2", "3" } },
                    { "GroupedExpressions", new List<object> {
                                                new Dictionary<string, object>{
                                                    { "Path", "Title"},
                                                    { "Negate", "false"},
                                                    { "Case", "ORIGINAL"},
                                                    { "Comparison", "CONTAINS"},
                                                    { "Connector", "OR"},
                                                    { "Value", "1" },
                                                    { "GroupedExpressions", new List<object> {
                                                                                new Dictionary<string, object>{
                                                                                    { "Path", "Title"},
                                                                                    { "Negate", "true"},
                                                                                    { "Case", "IGNORE"},
                                                                                    { "Comparison", "lessThan"},
                                                                                    { "Connector", "AND"},
                                                                                    { "Value", 2 }
                                                                                }
                                                                            }
                                                    }
                                                }
                                            }
                    }
                }
            };
            var result = DictionaryToObjectConverter.Convert<WhereExpression>(list);

            Assert.AreNotEqual(null, result, "Return null");
            Assert.AreEqual(typeof(List<WhereExpression>), result.GetType(), "Return other type");

            Assert.AreEqual(expectedList.GetHashCode(), ((WhereExpressions)result!).GetHashCode(), "Not equels objects");

        }

        [TestMethod]
        public void ConvertObject()
        {
            var obj = new Dictionary<string, object>{
                { "Path", "Title"},
                { "Negate", "true"},
                { "Case", "IGNORE"},
                { "Comparison", "LESS_THAN_OR_EQUAL"},
                { "Connector", "AND"},
                { "Value", new List<object>{ "1", "2", "3" } },
                { "GroupedExpressions", new List<object> {
                                            new Dictionary<string, object>{
                                                { "Path", "Title"},
                                                { "Negate", "false"},
                                                { "Case", "ORIGINAL"},
                                                { "Comparison", "CONTAINS"},
                                                { "Connector", "OR"},
                                                { "Value", "1" },
                                                { "GroupedExpressions", new List<object> {
                                                                            new Dictionary<string, object>{
                                                                                { "Path", "Title"},
                                                                                { "Negate", "true"},
                                                                                { "Case", "IGNORE"},
                                                                                { "Comparison", "lessThan"},
                                                                                { "Connector", "AND"},
                                                                                { "Value", 2 }
                                                                            }
                                                                        }
                                                }
                                            }
                                        }
                }
            };
            var result = DictionaryToObjectConverter.Convert<WhereExpression>(obj);

            Assert.AreNotEqual(null, result, "Return null");
            Assert.AreEqual(typeof(List<WhereExpression>), result.GetType(), "Return other type");

            Assert.AreEqual(expectedList.GetHashCode(), ((WhereExpressions)result!).GetHashCode(), "Not equels objects");

        }

        [TestMethod]
        public void ConvertClass()
        {
            var list = new List<WhereExpression> {
                new WhereExpression{
                    Path = "Title",
                    Negate = true,
                    Case = CaseString.Ignore,
                    Comparison = Comparison.LessThanOrEqual,
                    Connector = Connector.And,
                    Value = new List<string> { "1", "2", "3" },
                    GroupedExpressions = new List<WhereExpression> {
                                            new WhereExpression{
                                                Path = "Title",
                                                Negate = false,
                                                Case = CaseString.Original,
                                                Comparison = Comparison.Contains,
                                                Connector = Connector.Or,
                                                Value = new List<string> { "1" },
                                                GroupedExpressions = new List<WhereExpression> {
                                                    new WhereExpression{
                                                        Path = "Title",
                                                        Negate = true,
                                                        Case = CaseString.Ignore,
                                                        Comparison = Comparison.LessThan,
                                                        Connector = Connector.And,
                                                        Value = new List<string> { "2" }
                                                    }
                                                }
                                            }
                                        }
                }
            };
            var result = DictionaryToObjectConverter.Convert<WhereExpression>(list);

            Assert.AreNotEqual(null, result, "Return null");
            Assert.AreEqual(typeof(List<WhereExpression>), result.GetType(), "Return other type");

            Assert.AreEqual(expectedList.GetHashCode(), ((WhereExpressions)result!).GetHashCode(), "Not equels objects");

        }

        [TestMethod]
        public void ConvertTypeValue()
        {
            var list = new List<object> {
                new Dictionary<string, object>{
                    { "Value", "1" }
                },
                new Dictionary<string, object>{
                    { "Value", 3 }
                },
                new Dictionary<string, object>{
                    { "Value", new List<object>{ "1", "2", "3" } }
                },
                new Dictionary<string, object>{
                    { "Value", new List<string>{ "1", "2", "3" } }
                },
                new Dictionary<string, object>{
                    { "Value", new List<object>{ 1, 2, 3 } }
                }
            };
            var result = DictionaryToObjectConverter.Convert<WhereExpression>(list);

            Assert.AreNotEqual(null, result, "Return null");
            Assert.AreEqual(typeof(List<WhereExpression>), result.GetType(), "Return other type");

            var list1 = new WhereExpressions {
                new WhereExpression{
                    Value = new List<string> { "1" }
                },
                new WhereExpression{
                    Value = new List<string> { "3" }
                },
                new WhereExpression{
                    Value = new List<string> { "1", "2", "3" }
                },
                new WhereExpression{
                    Value = new List<string> { "1", "2", "3" }
                },
                new WhereExpression{
                    Value = new List<string> { "1", "2", "3" }
                }
            };

            Assert.AreEqual(list1.GetHashCode(), ((WhereExpressions)result!).GetHashCode(), "Not equels objects");

        }

        WhereExpressions expectedList = new WhereExpressions {
                new WhereExpression{
                    Path = "Title",
                    Negate = true,
                    Case = CaseString.Ignore,
                    Comparison = Comparison.LessThanOrEqual,
                    Connector = Connector.And,
                    Value = new List<string> { "1", "2", "3" },
                    GroupedExpressions = new List<WhereExpression> {
                                            new WhereExpression{
                                                Path = "Title",
                                                Negate = false,
                                                Case = CaseString.Original,
                                                Comparison = Comparison.Contains,
                                                Connector = Connector.Or,
                                                Value = new List<string> { "1" },
                                                GroupedExpressions = new List<WhereExpression> {
                                                    new WhereExpression{
                                                        Path = "Title",
                                                        Negate = true,
                                                        Case = CaseString.Ignore,
                                                        Comparison = Comparison.LessThan,
                                                        Connector = Connector.And,
                                                        Value = new List<string> { "2" }
                                                    }
                                                }
                                            }
                                        }
                }
            };
    }
}
