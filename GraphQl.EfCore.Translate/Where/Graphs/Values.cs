using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphQl.EfCore.Translate.Where.Graphs
{
    public class Values : Collection<string>, IList<string>
    {
        public Values() : base() { }
        public Values(IList<string> strings) : base(strings) { }
        public override bool Equals(object obj)
        {
            Values other = (Values)obj;
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

        public static implicit operator Values(List<string> v)
        {
            return new Values(v);
        }
    }
}
