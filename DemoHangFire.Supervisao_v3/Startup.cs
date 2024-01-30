using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;

namespace DemoHangFire.Supervisao_v3
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IConfigurationBuilder Builder { get; }
        public Startup(IWebHostEnvironment env)
        {
            Builder = new ConfigurationBuilder().SetBasePath(Path.Combine(env.ContentRootPath, "Settings"))
                                                .AddJsonFile($"connectionstrings.{env.EnvironmentName}.json", true, true)
                                                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true)
                                                .AddEnvironmentVariables();

            Configuration = Builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Configurando o intervalo de votação
            var options = new SqlServerStorageOptions
            {
                SchemaName = "supervisao_v3",
                SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                QueuePollInterval = TimeSpan.Zero
            };

            services.AddHangfire(op => op
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseRecommendedSerializerSettings()
                    .UseSqlServerStorage(Configuration.GetConnectionString("DefaultConnectionHangFire"), options));

            services.AddHangfireServer();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        [Obsolete]
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var options = new BackgroundJobServerOptions
            {
                ServerName = String.Format("{0}.{1}", Environment.MachineName, Guid.NewGuid().ToString()),
                WorkerCount = Environment.ProcessorCount * 15
            };

            app.UseHangfireServer(options);
            app.UseHangfireDashboard("/supervisao_v3");

            string nomeServico = "Job_1";
            RecurringJob.AddOrUpdate("supervisao_v3 - " + nomeServico, () => Console.WriteLine(nomeServico), Cron.Minutely());

            nomeServico = "Job_2";
            RecurringJob.AddOrUpdate("supervisao_v3 - " + nomeServico, () => Console.WriteLine(nomeServico), Cron.Hourly(3));
        }

    }
}
