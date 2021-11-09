using GraphQL.Conversion;
using GraphQL.DotNet.Queries;
using GraphQL.Types;
using GraphQL.Utilities;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace GraphQL.DotNet
{
    public class GraphQLSchema : Schema
    {
        public GraphQLSchema(IServiceProvider services) : base(services)
        {
            Query = services.GetRequiredService<MainQuery>();
            NameConverter = new DefaultNameConverter();
        }
    }
}
