using GraphQl.EfCore.Translate.Where.Graphs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphQl.EfCore.Translate.Select.Graphs
{
    public class ArgumentNodeGraph : /*IComparable<ArgumentNodeGraph>, */IEquatable<ArgumentNodeGraph>
    {
        public int? Skip { get; set; }
        public int? Take { get; set; }
        public string OrderBy { get; set; }
        public List<WhereExpression> Where { get; set; }

        private WhereExpressions? _Where
        {
            get
            {
                return Where is not null ? (WhereExpressions)Where : null;
            }
        }
        /*public int CompareTo(ArgumentNodeGraph node)
        {
            return this.Skip.CompareTo(node.Skip);
        }*/
        public override bool Equals(object obj)
        {
            return Equals(obj as ArgumentNodeGraph);
        }
        public bool Equals(ArgumentNodeGraph other)
        {
            return other != null && Skip == other.Skip && Take == other.Take && OrderBy == other.OrderBy && _Where == other._Where;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(Skip, Take, OrderBy, _Where);
        }

        public static bool operator ==(ArgumentNodeGraph model1, ArgumentNodeGraph model2)
        {
            return EqualityComparer<ArgumentNodeGraph>.Default.Equals(model1, model2);
        }
        public static bool operator !=(ArgumentNodeGraph model1, ArgumentNodeGraph model2)
        {
            return !(model1 == model2);
        }
    }
}
