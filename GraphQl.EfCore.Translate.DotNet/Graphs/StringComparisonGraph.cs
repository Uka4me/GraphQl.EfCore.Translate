using GraphQL.Language.AST;
using GraphQL.Types;
using System;

namespace GraphQl.EfCore.Translate.DotNet
{
    public class StringComparisonGraph : EnumerationGraphType<StringComparison> //<StringComparison>
    {
        /*public StringComparisonGraph()
        {
            Name = nameof(StringComparison);
            AddValue("Ordinal", null, StringComparison.Ordinal);
            AddValue("OrdinalIgnoreCase", null, StringComparison.OrdinalIgnoreCase);
        }*/

        public override bool CanParseLiteral(IValue value)
        {
            value = value.TryToEnumValue();
            return base.CanParseLiteral(value);
        }

        public override object? ParseLiteral(IValue value)
        {
            //Name = nameof(StringComparison);
            //var literal = base.ParseLiteral(value);
            var literal = base.ParseLiteral(value.TryToEnumValue());
            if (literal is not null)
            {
                return literal;
            }

            if (value is StringValue str)
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