using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Currency.Common.LogManange;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Crm.WebApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //var config = new ConfigurationBuilder().AddCommandLine(args).Build();
            //String ip = config["ip"];
            //String port = config["port"];

            //Console.WriteLine(ip + ":" + port);

            CreateWebHostBuilder(args)
                   //.UseUrls($"http://{ip}:{port}")
                   .Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                   .UseStartup<Startup>();

    }
}
