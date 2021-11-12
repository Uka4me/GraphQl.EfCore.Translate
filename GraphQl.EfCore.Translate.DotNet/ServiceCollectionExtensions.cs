using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphQl.EfCore.Translate.DotNet
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddGraphQLTranslate(this IServiceCollection services)
        {
            services.AddSingleton<StringComparisonGraph>();
            services.AddSingleton<WhereExpressionGraph>();
            //services.AddSingleton<OrderByGraph>();
            services.AddSingleton<ComparisonGraph>();
            services.AddSingleton<ConnectorGraph>();

            return services;
        }
    }
}
