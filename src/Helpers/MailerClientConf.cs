using System;
using System.Collections.Generic;
using System.Configuration;
using Spider.Net;

namespace fileuploader.Helpers
{
    public class MailerClientConf : IMailerClientConf
    {
		Microsoft.Extensions.Configuration.IConfiguration _config;
		public MailerClientConf(Microsoft.Extensions.Configuration.IConfiguration conf)
		{
			_config = conf;
		}

        public string GetServer()
        {
            return _config["Server"];
        }

        public string GetAPIPassword()
        {
            return _config["Pass"];
        }

        public string GetAPIUserName()
        {
            return _config["UserID"];
        }

        public string GetFrom()
        {
            return _config["From"];
        }
        public string GetBcc()
        {
            return _config["Bcc"];;
        }



        public List<string> GetTestRecipient()
        {
            List<string> numbers = new List<string>();
            numbers.Add(_config["To"] /*ConfigurationManager.AppSettings["MailApi.TestRecipient"]*/);
            return numbers;
        }

        public List<string> GetNotifyTo()
        {
            List<string> notifies = new List<string>();

            notifies.Add( _config["To"]/*ConfigurationManager.AppSettings["MailApi.NotifyTo"]*/);

            // try
            // {
            //     for (int i = 1; i <= 10; i++)
            //     {
            //         string addr = ConfigurationManager.AppSettings["MailApi.NotifyTo" + i.ToString()];

            //         if (!string.IsNullOrWhiteSpace(addr))
            //             notifies.Add(addr);
            //     }
            // }
            // catch { }

            return notifies;
        }


        public bool IsTestMode()
        {
            return true;
        }

        public SMTPSettings GetSMTPSettings()
        {
            SMTPSettings smtp = new SMTPSettings();

            smtp.Password = this.GetAPIPassword();
            smtp.ServerAddress = this.GetServer();
            smtp.User = this.GetAPIUserName();
            smtp.SSL = true;
            smtp.Port = 25;
            smtp.Timeout = 1000 * 120;
            return smtp;
        }
    }
}








