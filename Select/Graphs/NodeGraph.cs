﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphQl.EfCore.Translate.Select.Graphs
{
    public class NodeGraph : IComparable<NodeGraph>, IEquatable<NodeGraph>
    {
        public string Path { get; set; }
        public Dictionary<string, object> Arguments { get; set; }
        public int CompareTo(NodeGraph node)
        {
            return this.Path.CompareTo(node.Path);
        }
        public override bool Equals(object obj)
        {
            return Equals(obj as NodeGraph);
        }
        public bool Equals(NodeGraph other)
        {
            return other != null && Path == other.Path;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(Path);
        }
    }
}
