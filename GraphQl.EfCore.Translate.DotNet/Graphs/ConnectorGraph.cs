using GraphQLParser.AST;
using GraphQL.Types;
using GraphQl.EfCore.Translate;
using System;

namespace GraphQl.EfCore.Translate.DotNet
{
    public class ConnectorGraph : EnumerationGraphType<Connector>
    {
        /*public ConnectorGraph()
        {
            Name = nameof(Connector);
            AddValue("and", null, Connector.And);
            AddValue("or", null, Connector.Or);
        }*/

        public override bool CanParseLiteral(GraphQLValue value)
        {
            value = value.TryToEnumValue();
            return base.CanParseLiteral(value);
        }

        public override object? ParseLiteral(GraphQLValue value)
        {
            var literal = base.ParseLiteral(value.TryToEnumValue());

            if (literal is not null)
            {
                return literal;
            }

            if (value is GraphQLStringValue str)
            {
                var strValue = str.Value;
                if (Enum.TryParse(strValue, true, out Connector comparison))
                {
                    return comparison;
                }
            }

            return null;
        }
    }
}