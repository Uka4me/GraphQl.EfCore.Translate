using Microsoft.Extensions.DependencyInjection;

namespace GraphQl.EfCore.Translate.HotChocolate
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEFCoreGraphQLHotChocolate(this IServiceCollection services)
        {
            services.AddSingleton<StringComparisonGraph>();
            services.AddSingleton<WhereExpressionGraph>();
            services.AddSingleton<ComparisonGraph>();
            services.AddSingleton<ConnectorGraph>();

            return services;
        }
    }
}
