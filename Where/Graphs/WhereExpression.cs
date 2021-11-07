using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GraphQl.EfCore.Translate
{

    public class WhereExpression
    {
        public string Path { get; set; } = string.Empty;
        public Comparison Comparison { get; set; } = Comparison.Equal;
        public StringComparison? Case { get; set; } = null;
        [JsonConverter(typeof(SingleOrArrayConverter<string>))]
        public List<string>? Value { get; set; }
        public bool Negate { get; set; } = false;
        public Connector Connector { get; set; } = Connector.And;
        public WhereExpression[]? GroupedExpressions { get; set; }
    }
}