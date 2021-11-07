using GraphQL.Types;

namespace GraphQl.EfCore.Translate
{

    public class WhereExpressionGraph :
        InputObjectGraphType<WhereExpression>
    {
        public WhereExpressionGraph()
        {
            Name = nameof(WhereExpression);
            Field(x => x.Path, true);
            Field<ComparisonGraph>("Comparison", null, null, _ => _.Source!.Comparison);
            Field(x => x.Negate, true);
            Field<StringComparisonGraph>("Case", null, null, _ => _.Source!.Case);
            Field(x => x.Value, true);
            Field<ConnectorGraph>("Connector", null, null, _ => _.Source!.Connector);
            Field<ListGraphType<WhereExpressionGraph>>(
                name: "GroupedExpressions");
        }
    }
}