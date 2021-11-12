
using GraphQl.EfCore.Translate;
using HotChocolate.Types;

namespace GraphQl.EfCore.Translate.HotChocolate.Graphs
{

    public class WhereExpressionGraph : InputObjectType<WhereExpression>
    {
        protected override void Configure(IInputObjectTypeDescriptor<WhereExpression> descriptor)
        {
            descriptor.Field(t => t.Path).Type<NonNullType<StringType>>();
            descriptor.Field(t => t.Negate).Type<BooleanType>().DefaultValue(false);
            descriptor.Field(t => t.Value).Type<ListType<StringType>>();
            descriptor.Field(t => t.Comparison).Type<ComparisonGraph>().DefaultValue(Comparison.Equal);
            descriptor.Field(t => t.Case).Type<StringComparisonGraph>().DefaultValue(null);
            descriptor.Field(t => t.Connector).Type<ConnectorGraph>().DefaultValue(Connector.And);
            descriptor.Field(t => t.GroupedExpressions).Type<ListType<WhereExpressionGraph>>();
        }
        /*public WhereExpressionGraph()
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
        }*/
    }
}