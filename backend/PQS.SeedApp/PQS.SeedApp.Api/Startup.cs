using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using PQS.SeedApp.Business;

namespace PQS.SeedApp.Api
{

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });

            services.AddSeedAppBusiness(Configuration.GetSection("Business"));

            services.AddControllers(options =>
            {
                options.Filters.Add(typeof(HttpGlobalExceptionFilter));
            })
            .AddJsonOptions(options =>
            {
                // arregla un problema con los enums en swagger 
                // https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/1269#issuecomment-586284629
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            }); ;



            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Version = "v1", Title = "PQS SeedApp API" });


            });


        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "ASP.NET Core uses this")]
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseForwardedHeaders();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

           
            app.UseSwagger(c =>
            {
                //c.PreSerializeFilters.Add((swaggerDoc, httpReq) => swaggerDoc.b = "/seed");
                c.PreSerializeFilters.Add((swaggerDoc, httpRequest) =>
                {
                    if (!httpRequest.Headers.ContainsKey("Referer")) return;
                  

                    var serverUrl = httpRequest.Headers["Referer"].ToString().Replace("/swagger/index.html", "", StringComparison.InvariantCultureIgnoreCase);
                    swaggerDoc.Servers.Clear();

                    swaggerDoc.Servers = new List<OpenApiServer>() { new OpenApiServer { Url = serverUrl } };
                });
            });

            app.UseSwaggerUI(c =>
            {
               
                c.SwaggerEndpoint("v1/swagger.json", "PQS.SeedApp");

                c.EnableDeepLinking();


            });


            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

        }
    }
}
