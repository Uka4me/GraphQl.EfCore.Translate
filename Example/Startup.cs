using Entity;
using Entity.Models;
using GraphQl.EfCore.Translate.DotNet;
/*using GraphQl.EfCore.Translate.HotChocolate;
using GraphQl.EfCore.Translate.Where.Graphs;*/
using GraphQL;
using GraphQL.DotNet;
using GraphQL.DotNet.Queries;
/*using GraphQL.Execution;
using GraphQL.HotChocolate;
using GraphQL.HotChocolate.Queries;
using GraphQL.Server;*/
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;

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
            services.AddPooledDbContextFactory<SchoolContext>(options => {
                options.UseInMemoryDatabase("Test");
            });

            // GrapQL-dotnet ============================================
            services.AddEFCoreGraphQLDotNet();
            services.AddSingleton<MainQuery>();
            services.AddSingleton<GraphQLSchema>();

            /*services
                .AddGraphQL()
                //.AddGraphTypes(ServiceLifetime.Scoped)
                .AddSystemTextJson()
                .AddGraphTypes(typeof(GraphQLSchema));*/
            services.AddGraphQL(b => b
                .AddSystemTextJson()
                .AddSchema<GraphQLSchema>()
            );
            // ===========================================================


            // HotChocolate ==============================================
            /*services.AddEFCoreGraphQLHotChocolate();

            services
                .AddGraphQLServer()
                .AddErrorFilter<ErrorFilter>()
                .AddType<GraphQL.HotChocolate.Types.CourseObject>()
                .AddType<GraphQL.HotChocolate.Types.EnrollmentObject>()
                .AddType<GraphQL.HotChocolate.Types.StudentObject>()
                .AddType<GraphQL.HotChocolate.Types.PageInfoObject<GraphQL.HotChocolate.Types.StudentObject, Student>>()
                .AddQueryType<Query>();*/
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
                /*endpoints.MapGraphQL("/hotchocolate");*/
            });
        }
    }
}
