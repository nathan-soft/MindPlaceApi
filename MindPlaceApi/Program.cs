using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MindPlaceApi.Data.DataInitializer;
using MindPlaceApi.Services;

namespace MindPlaceApi
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            //using (var scope = host.Services.CreateScope())
            //{
            //    var serviceProvider = scope.ServiceProvider;
            //    try
            //    {
            //        var userService = serviceProvider.GetRequiredService<IUserService>();
            //        var followService = serviceProvider.GetRequiredService<IFollowService>();
            //        var mapper = serviceProvider.GetRequiredService<IMapper>();
            //        var dbs = new DbSeeder(userService, followService);
            //        await dbs.SeedDataAsync();
            //    }
            //    catch (Exception ex)
            //    {
            //        Debug.WriteLine(ex.Message);
            //    }
            //}
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseUrls("https://localhost:4000/");
                });
    }
}
