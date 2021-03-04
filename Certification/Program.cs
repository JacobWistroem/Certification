using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Certification
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    HostConfig.CertPath = context.Configuration["CertPath"];

                    HostConfig.CertPassword = context.Configuration["CertPassword"];

                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    var host = System.Net.Dns.GetHostEntry("h5todo.dk");
                    webBuilder.ConfigureKestrel(opt =>
                    {
                        opt.Listen(host.AddressList[0], 80);
                        opt.Listen(host.AddressList[0], 443, listOpt =>
                        {
                            listOpt.UseHttps(HostConfig.CertPath, HostConfig.CertPassword);
                        });
                    });
                    webBuilder.UseStartup<Startup>();
                });
    }

    public static class HostConfig
    {
        public static string CertPath { get; set; }
        public static string CertPassword { get; set; }
    }

}
