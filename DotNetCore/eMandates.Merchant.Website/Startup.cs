using eMandates.Merchant.Library;
using eMandates.Merchant.Library.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace eMandates.Merchant.Website
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services
                .AddOptions()
                .Configure<Configuration>(_configuration);

            services
                .AddSingleton<CoreCommunicator>()
                .AddSingleton<B2BCommunicator>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app
                .UseStaticFiles()
                .UseMvcWithDefaultRoute();
        }
    }
}