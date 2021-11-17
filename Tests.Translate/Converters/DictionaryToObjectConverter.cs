using GraphQl.EfCore.Translate;
using GraphQl.EfCore.Translate.Converters;
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

            Func<WhereExpression, WhereExpression, bool>? func = null;
            func = (x, y) =>
                x.Path == y.Path &&
                x.Negate == y.Negate &&
                x.Case == y.Case &&
                x.Comparison == y.Comparison &&
                x.Connector == y.Connector &&
                x.Value == x.Value &&
                ((x.GroupedExpressions == null && y.GroupedExpressions == null) ||
                CompareIEnumerable(x.GroupedExpressions!, y.GroupedExpressions!, func!));

            CompareIEnumerable(new List<WhereExpression> {
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
            }, result, func);
        }

        private static bool CompareIEnumerable<T>(IEnumerable<T> one, IEnumerable<T> two, Func<T, T, bool> comparisonFunction)
        {
            var oneArray = one as T[] ?? one.ToArray();
            var twoArray = two as T[] ?? two.ToArray();

            if (oneArray.Length != twoArray.Length)
            {
                Assert.Fail("Collections are not same length");
            }

            for (int i = 0; i < oneArray.Length; i++)
            {
                var isEqual = comparisonFunction(oneArray[i], twoArray[i]);
                Assert.IsTrue(isEqual);
            }

            return true;
        }
    }
}
