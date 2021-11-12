using Entity;
using GraphQl.EfCore.Translate.DotNet;
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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

            services.AddGraphQLTranslate();
            services.AddSingleton<MainQuery>();
            services.AddSingleton<GraphQLSchema>();

            services
                .AddGraphQL()
                .AddGraphTypes(ServiceLifetime.Scoped)
                .AddSystemTextJson()
                .AddGraphTypes(typeof(GraphQLSchema));

            services
                .AddGraphQLServer()
                .AddType<GraphQL.HotChocolate.Types.CourseObject>()
                .AddType<GraphQL.HotChocolate.Types.EnrollmentObject>()
                .AddType<GraphQL.HotChocolate.Types.StudentObject>()
                .AddType<GraphQl.EfCore.Translate.HotChocolate.Graphs.WhereExpressionGraph>()
                .AddQueryType<StudentQuery>();
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
                endpoints.MapGraphQL<GraphQLSchema>();
                endpoints.MapGraphQL("/hotchocolate");
            });
        }
    }
}
