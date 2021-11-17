using Microsoft.Extensions.DependencyInjection;

namespace GraphQl.EfCore.Translate.DotNet
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEFCoreGraphQLDotNet(this IServiceCollection services)
        {
            services.AddSingleton<CaseStringGraph>();
            services.AddSingleton<WhereExpressionGraph>();
            services.AddSingleton<ComparisonGraph>();
            services.AddSingleton<ConnectorGraph>();

            return services;
        }
    }
}
