using GraphQl.EfCore.Translate.Select.Graphs;
using GraphQl.EfCore.Translate.Where.Graphs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Tests.Translate.Select.Graphs
{
    [TestClass]
    public class NodeGraphsTests
    {
        [TestMethod]
        public void EqualsList() {

            NodeGraphs list1 = new NodeGraphs() {
                new NodeGraph{ 
                    Path = "User", 
                    Arguments = new ArgumentNodeGraph
                    {
                        Skip = 0,
                        Take = 30,
                        OrderBy = "Name",
                        Where = new List<WhereExpression>{
                            new WhereExpression{
                                Path = "Title",
                                Negate = true,
                                Case = GraphQl.EfCore.Translate.CaseString.Ignore,
                                Comparison = GraphQl.EfCore.Translate.Comparison.Equal,
                                Connector = GraphQl.EfCore.Translate.Connector.And,
                                Value = new List<string>{ "2" }
                            }
                        }
                    }
                }
            };

            NodeGraphs list2 = new NodeGraphs() {
                new NodeGraph{
                    Path = "User",
                    Arguments = new ArgumentNodeGraph
                    {
                        Skip = 0,
                        Take = 30,
                        OrderBy = "Name",
                        Where = new List<WhereExpression>{
                            new WhereExpression{
                                Path = "Title",
                                Negate = true,
                                Case = GraphQl.EfCore.Translate.CaseString.Ignore,
                                Comparison = GraphQl.EfCore.Translate.Comparison.Equal,
                                Connector = GraphQl.EfCore.Translate.Connector.And,
                                Value = new List<string> { "2" }
                            }
                        }
                    }
                }
            };

            Assert.AreEqual(list1.GetHashCode(), list2.GetHashCode());

            list1[0].Path = "Company";

            Assert.AreNotEqual(list1.GetHashCode(), list2.GetHashCode());

            list1[0].Path = "User";

            Assert.AreEqual(list1.GetHashCode(), list2.GetHashCode());

            list1[0].Arguments.Where[0].Value[0] = "3";

            Assert.AreNotEqual(list1.GetHashCode(), list2.GetHashCode());
        }
    }
}
