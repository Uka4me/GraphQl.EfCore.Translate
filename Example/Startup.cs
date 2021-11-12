using Entity;
using GraphQL;
using GraphQL.DotNet;
using GraphQL.DotNet.Queries;
using GraphQL.HotChocolate.Queries;
using GraphQL.Server;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace GraphQl.EfCore.Translate.Example
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // services.AddDbContext<SchoolContext>(options => options.UseInMemoryDatabase("Test"));
            services.AddPooledDbContextFactory<SchoolContext>(options => {
                options.UseInMemoryDatabase("Test");
                options.LogTo(Console.WriteLine);
            });

            // GrapQL-dotnet ============================================
            services.AddSingleton<GraphQl.EfCore.Translate.DotNet.StringComparisonGraph>();
            services.AddSingleton<GraphQl.EfCore.Translate.DotNet.WhereExpressionGraph>();
            services.AddSingleton<GraphQl.EfCore.Translate.DotNet.ComparisonGraph>();
            services.AddSingleton<GraphQl.EfCore.Translate.DotNet.ConnectorGraph>();
            services.AddSingleton<MainQuery>();
            services.AddSingleton<GraphQLSchema>();

            services
                .AddGraphQL()
                .AddGraphTypes(ServiceLifetime.Scoped)
                .AddSystemTextJson()
                .AddGraphTypes(typeof(GraphQLSchema));
            // ===========================================================


            // HotChocolate ==============================================
            services.AddSingleton<GraphQl.EfCore.Translate.HotChocolate.StringComparisonGraph>();
            services.AddSingleton<GraphQl.EfCore.Translate.HotChocolate.WhereExpressionGraph>();
            services.AddSingleton<GraphQl.EfCore.Translate.HotChocolate.ComparisonGraph>();
            services.AddSingleton<GraphQl.EfCore.Translate.HotChocolate.ConnectorGraph>();

            services
                .AddGraphQLServer()
                .AddType<GraphQL.HotChocolate.Types.CourseObject>()
                .AddType<GraphQL.HotChocolate.Types.EnrollmentObject>()
                .AddType<GraphQL.HotChocolate.Types.StudentObject>()
                .AddQueryType<StudentQuery>();
            // ===========================================================
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGraphQL<GraphQLSchema>("/dotnet");
                endpoints.MapGraphQL("/hotchocolate");
            });
        }
    }
}
