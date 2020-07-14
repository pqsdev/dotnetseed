using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PQS.SeedApp.Business;
using PQS.SeedApp.Business.HostedServices;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SeedAppServiceExtensions
    {
        /// <summary>
        /// Registra lso servicios de la capa de negocio
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configSection">Seccion que contiene la configuraiocn del backend</param>
        /// <returns></returns>
        /// <example>
        ///     This example shows how to use <see cref="AddSeedAppBusiness"/>.
        ///     <code>
        ///         public class Startup
        ///         {
        ///             public Startup(IConfiguration configuration)
        ///             {
        ///                 Configuration = configuration;
        ///             }
        ///             public IConfiguration Configuration { get; }
        ///         
        ///             public void ConfigureServices(IServiceCollection services)
        ///             {
        ///                  services.AddSeedAppBusiness(Configuration.GetSection("PQS.SeedApp.Business"));
        ///             }
        ///         }
        ///     </code>
        /// </example>
        public static IServiceCollection AddSeedAppBusiness(this IServiceCollection services, IConfigurationSection configSection)
        {

            if (configSection == null)
            {
                throw new ArgumentNullException(nameof(configSection));
            }

            services.AddOptions<SeedAppOptions>()
                .Bind(configSection)
                
                .Validate(config =>
                {
                    return !String.IsNullOrWhiteSpace( config.Messaging.BootstrapServers);
                }, "Invalid MessageBootstrapServers in config");



            services.AddSeedAppHostedServices();
            return services;
        }

        /// <summary>
        /// Registra los HostedService del sistema
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        /// public class Program
        ///   {
        ///         public static void Main(string[] args)
        ///         {
        ///             CreateHostBuilder(args).Build().Run();
        ///         }
        /// 
        ///         public static IHostBuilder CreateHostBuilder(string[] args) =>
        ///             Host.CreateDefaultBuilder(args)
        ///                 .ConfigureWebHostDefaults(webBuilder =>
        ///                 {
        ///                     webBuilder.UseStartup<Startup>();
        ///                 }).ConfigureServices(services => {
        ///                     services.AddSeedAppHostedServices();
        ///                 });
        ///     }
        /// <example>
        ///     This example shows how to use <see cref="AddSeedAppHostedServices"/>.
        ///     <code>
        ///         public class Startup
        ///         {
        ///             public Startup(IConfiguration configuration)
        ///             {
        ///                 Configuration = configuration;
        ///             }
        ///             public IConfiguration Configuration { get; }
        ///         
        ///             public void ConfigureServices(IServiceCollection services)
        ///             {
        ///                  services.AddSeedAppBusiness(options => services.Configure<SeedAppOptions>(Configuration.GetSection("PQS.SeedApp.Business")));
        ///             }
        ///         }
        ///     </code>
        /// </example>
        public static IServiceCollection AddSeedAppHostedServices(this IServiceCollection services)
        {
            services.AddHostedService<ImportFileBackgroundService>();
            return services;
        }

    }



}
