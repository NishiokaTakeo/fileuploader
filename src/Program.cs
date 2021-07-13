using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog.Web;
using NLog.Extensions.Logging;
using Microsoft.Extensions.Logging;


namespace fileuploader
{
    public class Program
    {

        public static void Main(string[] args)
        {

            CreateHostBuilder(args).Build().Run();

        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)

				.UseWindowsService(options =>
				{
					options.ServiceName = "Spider File Uploader Service";
				})			

				// .UseContentRoot( System.IO.Path.GetDirectoryName(Environment.CurrentDirectory) + "\\src")
				.UseContentRoot(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\src")

				// .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)

                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();
                })

				.ConfigureLogging(logging =>
                {
                    // logging.ClearProviders();
                    // logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information);
                })
                .UseNLog();

    }
}
