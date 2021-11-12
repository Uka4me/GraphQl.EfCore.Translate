using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.Classes
{
    public class PageInfo<T>
    {
        public int Total { get; set; }
        public List<T> Data { get; set; }
        public int CurrentPage { get; set; }
    }
}
