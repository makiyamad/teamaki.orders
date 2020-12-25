using System;
using System.Net;
using System.Net.Http;
using AutoMapper;
using Consul;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TeaMaki.Persistence;
using TeaMaki.Orders.Controllers;
using Polly;
using Polly.Extensions.Http;
using TeaMaki.Orders.Services;

namespace TeaMaki.Orders
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
            services.AddControllers();
            services.AddAutoMapper(typeof(OrderProfiles));
            services.AddPersistenceLibrary(DatabaseEngine.RavenDb, Configuration.GetSection("Database"));
            services.AddSwaggerGen();

            services.AddSingleton<IConsulClient, ConsulClient>(serviceProvider =>
                new ConsulClient(config => {
                    config.Address = new Uri(Configuration["Consul:Address"]);
                }));

            services.AddSingleton<IConsulRegistryService, ConsulRegistryService>();

            services.AddHttpClient("product")
                .AddPolicyHandler(GetRetryPolicy())
                .AddTypedClient((client, serviceProvider) => {

                    var consulRegistryService = serviceProvider.GetRequiredService<IConsulRegistryService>();
                    client.BaseAddress = consulRegistryService.GetService();

                    return Refit.RestService.For<IProductRestService>(client);
                });

        }

        private IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            var jitter = new Random();

            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(6, sleepDuration => TimeSpan.FromSeconds(Math.Pow(2, sleepDuration))
                + TimeSpan.FromMilliseconds(jitter.Next(0, 100)));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("swagger/v1/swagger.json", "Orders V1");
                options.RoutePrefix = string.Empty;
            });
        }
    }
}
