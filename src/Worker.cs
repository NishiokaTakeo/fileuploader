using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using fileuploader.Dao.Controllers;
using fileuploader.Dao.Interfaces;

namespace fileuploader
{


    public class Worker : BackgroundService
    {
		private static IServiceProvider serviceProvider;
		static Microsoft.Extensions.Configuration.IConfiguration _config;

        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger, Microsoft.Extensions.Configuration.IConfiguration configuration)
        {

            _logger = logger;
			_config = configuration;
        }
		public override async Task StartAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Service has been started {0}", _config["TargetPath"]);

			ConfigureServices();

			await base.StartAsync(cancellationToken);
		}

		public override async Task StopAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Service has been stopped");

			await base.StopAsync(cancellationToken);
		}

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
			while (!stoppingToken.IsCancellationRequested)
            {

				var mailClient = serviceProvider.GetService<fileuploader.Helpers.IMailClient>();
				var client = serviceProvider.GetService<SpiderDocsWebAPIs.ISDWebClient>();
				var tablectr = serviceProvider.GetService<IAppFfoundController>();
				var tablectr2 = serviceProvider.GetService<IAppStudentsController>();
				var tablectr3 = serviceProvider.GetService<IAppClassListController>();

				var saver = new CoversheetManager(client, _config, tablectr, mailClient, _logger, tablectr2, tablectr3);

				// saver.Start();
				await saver.Run();

                // _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000 * 15, stoppingToken);
            }
        }

		private static void ConfigureServices()
		{

			var services = new ServiceCollection();

			// services.AddTransient<SpiderDocsWebAPIs.SpiderDoscWebClient.IConfiguration,fileuploader.SpiderDocsConf>();
			services.AddTransient<SpiderDocsWebAPIs.ISDWebClient, SpiderDocsWebAPIs.SpiderDoscWebClient>();
			services.AddScoped<fileuploader.Helpers.IMailClient, fileuploader.Helpers.Mailer>();
			services.AddScoped<IAppFfoundController>(provider => new AppFfoundController( _config["DB"] ) );
			services.AddScoped<IAppStudentsController>(provider => new AppStudentsController( _config["DB"] ));
			services.AddScoped<IAppClassListController>(provider => new AppClassListController( _config["DB"] ));
			services.AddTransient<SpiderDocsWebAPIs.SpiderDoscWebClient.IConfiguration>(provider => new fileuploader.SpiderDocsConf( _config ));
			services.AddScoped<fileuploader.Helpers.IMailerClientConf>(provider => new fileuploader.Helpers.MailerClientConf( _config.GetSection("SMTP") ));
			// services.AddScoped<fileuploader.Helpers.IMailerClientConf, fileuploader.Helpers.MailerClientConf>();
			// services.AddTransient<SpiderDocsWebAPIs.SpiderDoscWebClient.IConfiguration,fileuploader.SpiderDocsConf>();

			serviceProvider = services.BuildServiceProvider();
		}

		public override void Dispose()
		{

		}
    }

}
