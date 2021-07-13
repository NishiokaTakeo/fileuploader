using System;
using NLog;
using SpiderDocsModule;

namespace fileuploader
{
	public class SpiderDocsConf : SpiderDocsWebAPIs.SpiderDoscWebClient.IConfiguration
	{
		Logger _logger = LogManager.GetCurrentClassLogger();
		Microsoft.Extensions.Configuration.IConfiguration _config;

		public SpiderDocsConf(Microsoft.Extensions.Configuration.IConfiguration conf)
		{
			_config = conf;
		}

		public Func<DbManager> GetDbManager()
		{
			try
            {
                SpiderDocsApplication.CurrentServerSettings = new ServerSettings();
                SpiderDocsApplication.CurrentServerSettings.server = _config["SDServerAddress"]; //"10.40.1.20";
                SpiderDocsApplication.CurrentServerSettings.port = 5322;
                SpiderDocsServer server = new SpiderDocsServer(SpiderDocsApplication.CurrentServerSettings);
                server.onConnected += new Action<ServerSettings, bool>((RetServerSettings, ConnectionChk) =>
                {
                    if (ConnectionChk)
                    {
                        SpiderDocsApplication.CurrentServerSettings = RetServerSettings;
                        SqlOperation.MethodToGetDbManager = new Func<DbManager>(() =>
                        {
                            return new DbManager(SpiderDocsApplication.CurrentServerSettings.conn, SpiderDocsApplication.CurrentServerSettings.svmode);
                        });
                        SpiderDocsApplication.CurrentPublicSettings = new PublicSettings();
                        SpiderDocsApplication.CurrentPublicSettings.Load();
                    }
                });


                server.Connect();

            }
            catch(Exception ex)
            {
                _logger.Error(ex);
            }

            return SqlOperation.MethodToGetDbManager;
		}

		public Logger GetLogger()
		{
			return _logger;
		}


		public string GetServerURL()
		{
			// var conf = _appconf.GetSection("DocAPI");
			// return _appconf.SDServerURL;
			return _config["SDServerURL"];
		}

		public string LoginID()
		{
			return  _config["SDLoginID"];
		}

		public string LoginPassword()
		{
			return _config["SDLoginPassword"];
		}

		public int NoAuth()
		{
			return Convert.ToInt32(_config["SDNoAuth"]);
		}
	}
}