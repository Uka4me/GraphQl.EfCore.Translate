using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphQl.EfCore.Translate.Where.Graphs
{
    public class WhereExpressions : Collection<WhereExpression>, IList<WhereExpression>
    {
        public WhereExpressions() : base() { }
        public WhereExpressions(IList<WhereExpression> objs) : base(objs) { }
        public WhereExpressions(IList<object> objs) : base((IList<WhereExpression>)objs) { }
        public override bool Equals(object obj)
        {
            WhereExpressions other = (WhereExpressions)obj;
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

        public static implicit operator WhereExpressions(List<WhereExpression> v)
        {
            return new WhereExpressions(v);
        }

        public static explicit operator List<WhereExpression>(WhereExpressions v)
        {
            return v.ToList();
        }
    }
}
