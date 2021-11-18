using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphQl.EfCore.Translate.Select.Graphs
{
    public class NodeGraphs : Collection<NodeGraph>, IList<NodeGraph>
    {
        public NodeGraphs() : base() { }
        public NodeGraphs(IList<NodeGraph> strings) : base(strings) { }

        public override bool Equals(object obj)
        {
            NodeGraphs other = (NodeGraphs)obj;
            return this.All(x => other.Contains(x)) && other.All(y => this.Contains(y));
        }

        public override int GetHashCode()
        {
            int res = 0;
            foreach (var item in this)
            {
                res = res + (item == null ? 0 : item.GetHashCode());
            }
            return res;
        }

        public static implicit operator NodeGraphs(List<NodeGraph> v)
        {
            return new NodeGraphs(v);
        }
    }
}
