﻿using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace eMandates.Merchant.Website
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder<Startup>(args);
    }
}