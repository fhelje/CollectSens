using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MeasurementApi.Configuration;
using MeasurementApi.Services.Sensors;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MeasurementApi
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();

        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials() );
            });
            // Add framework services.
            services.AddOptions();
            services.Configure<MeasurementOptions>(Configuration);


            services.AddTransient<ISensorService, SensorService>();
            var csv = SensorService.Create(Configuration["sensorDatabase"]);
            var task = csv.GetAll();
            Task.WhenAny(task);
            var dict = task.Result.ToDictionary(x => x.DeviceId, x => new Device(x) {Status = DeviceStatus.Initializing});
            services.AddSingleton(dict);
            services.AddTransient<ICurrentSensorValues, CurrentSensorValues>();
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            app.UseCors("CorsPolicy");
            app.UseMvc();
        }
    }
}
