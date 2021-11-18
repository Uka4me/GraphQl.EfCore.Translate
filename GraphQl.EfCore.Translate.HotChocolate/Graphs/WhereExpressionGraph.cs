
using GraphQl.EfCore.Translate.Where.Graphs;
using HotChocolate.Types;

namespace GraphQl.EfCore.Translate.HotChocolate
{

    public class WhereExpressionGraph : InputObjectType<WhereExpression>
    {
        protected override void Configure(IInputObjectTypeDescriptor<WhereExpression> descriptor)
        {
            descriptor.Name(nameof(WhereExpression));
            descriptor.Field(t => t.Path).Type<NonNullType<StringType>>();
            descriptor.Field(t => t.Negate).Type<BooleanType>().DefaultValue(false);
            descriptor.Field(t => t.Value).Type<ListType<StringType>>();
            descriptor.Field(t => t.Comparison).Type<ComparisonGraph>().DefaultValue(Comparison.Equal);
            descriptor.Field(t => t.Case).Type<CaseStringGraph>().DefaultValue(null);
            descriptor.Field(t => t.Connector).Type<ConnectorGraph>().DefaultValue(Connector.And);
            descriptor.Field(t => t.GroupedExpressions).Type<ListType<WhereExpressionGraph>>();
        }
    }
}