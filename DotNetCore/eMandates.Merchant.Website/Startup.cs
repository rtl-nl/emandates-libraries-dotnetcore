using eMandates.Merchant.Library;
using eMandates.Merchant.Library.Configuration;
using eMandates.Merchant.Library.Logging;
using eMandates.Merchant.Library.XML;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
                .Configure<Configuration>(_configuration.GetSection("eMandates"));

            services
                .AddSingleton<eMandates.Merchant.Library.Configuration.IConfiguration>(sp => sp.GetRequiredService<IOptions<Configuration>>().Value)
                .AddSingleton<ICertificateLoader, CertificateStoreLoader>()
                .AddSingleton<IXmlLogger, XmlLogger>()
                .AddSingleton<IXmlProcessor, XmlProcessor>()
                .AddSingleton<ICoreCommunicator, CoreCommunicator>()
                .AddSingleton<IB2BCommunicator, B2BCommunicator>();
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