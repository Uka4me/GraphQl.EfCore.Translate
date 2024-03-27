using GraphQLParser.AST;
using GraphQL.Types;
using System;

namespace GraphQl.EfCore.Translate.DotNet
{
    public class CaseStringGraph : EnumerationGraphType<CaseString> //<StringComparison>
    {
        /*public StringComparisonGraph()
        {
            Name = nameof(StringComparison);
            AddValue("Ordinal", null, StringComparison.Ordinal);
            AddValue("OrdinalIgnoreCase", null, StringComparison.OrdinalIgnoreCase);
        }*/

        public override bool CanParseLiteral(GraphQLValue value)
        {
            value = value.TryToEnumValue();
            return base.CanParseLiteral(value);
        }

        public override object? ParseLiteral(GraphQLValue value)
        {
            //Name = nameof(StringComparison);
            //var literal = base.ParseLiteral(value);
            var literal = base.ParseLiteral(value.TryToEnumValue());
            if (literal is not null)
            {
                return literal;
            }

            if (value is GraphQLStringValue str)
            {
                if (Enum.TryParse(str.Value, true, out StringComparison comparison))
                {
                    return comparison;
                }
            }

            return null;
        }
    }
}