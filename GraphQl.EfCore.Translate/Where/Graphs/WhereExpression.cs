using GraphQl.EfCore.Translate.Converters;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GraphQl.EfCore.Translate.Where.Graphs
{
    public class WhereExpression : /*IComparable<WhereExpression>, */IEquatable<WhereExpression>
    {
        public string Path { get; set; } = string.Empty;
        public Comparison Comparison { get; set; } = Comparison.Equal;
        public CaseString? Case { get; set; } = null;
        [JsonConverter(typeof(SingleOrArrayConverter<string>))]
        public List<string>? Value { get; set; }
        public bool Negate { get; set; } = false;
        public Connector Connector { get; set; } = Connector.And;
        public List<WhereExpression>? GroupedExpressions { get; set; }

        private Values? _Value { 
            get {
                return Value is not null ? (Values)Value : null;
            } 
        }

        private WhereExpressions? _GroupedExpressions
        {
            get
            {
                return GroupedExpressions is not null ? (WhereExpressions)GroupedExpressions : null;
            }
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as WhereExpression);
        }
        public bool Equals(WhereExpression other)
        {
            return other != null && Path == other.Path && Comparison == other.Comparison && Case == other.Case && _Value == other._Value && Negate == other.Negate && Connector == other.Connector && _GroupedExpressions == other._GroupedExpressions;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(Path, Comparison, Case, _Value, Negate, Connector, _GroupedExpressions);
        }

        public static bool operator ==(WhereExpression model1, WhereExpression model2)
        {
            return EqualityComparer<WhereExpression>.Default.Equals(model1, model2);
        }
        public static bool operator !=(WhereExpression model1, WhereExpression model2)
        {
            return !(model1 == model2);
        }
    }
}