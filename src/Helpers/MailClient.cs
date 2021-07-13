using System;
using System.Collections.Generic;
using System.Linq;
using Spider.Net;
using System.Configuration;
using NLog;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace fileuploader.Helpers
{

    public interface IMailerClientConf
    {
        string GetServer();
        string GetAPIUserName();
        string GetAPIPassword();
        string GetBcc();
        string GetFrom();
        bool IsTestMode();
        SMTPSettings GetSMTPSettings();

        List<string> GetTestRecipient();

        List<string> GetNotifyTo();

    }


    public class Mailer: fileuploader.Helpers.IMailClient
    {
        Logger logger = LogManager.GetCurrentClassLogger();
        List<string> _unsent  = new List<string>();

        IMailerClientConf _iConf ;

        Spider.Net.Email _email ;
        char[] splitwords = new char[] { ';', ',' };
		public Mailer(IMailerClientConf iconf)
		{
 			_iConf = iconf;
		}
        public Mailer(IMailerClientConf iconf, String subject, String Body, String from, String to)
        {
            _iConf = iconf;

            _email = new Spider.Net.Email(iconf.GetSMTPSettings());

            //_email.DeleteAttachements = true;
            //_email.PlainText = true;
            //_email.from = (from.Contains("@cet.asn.au")) ? from : _iConf.GetFrom();
            _email.from = (!string.IsNullOrEmpty(from)) ? from : _iConf.GetFrom();
            _email.subject = subject;
            _email.body = Body;
            _email.to.AddRange(to.Split(splitwords).Select(a => a.Trim()).ToArray());
            _email.bcc.AddRange(_iConf.GetBcc().Split(splitwords).Select(a => a.Trim()).ToArray());
        }

        public bool isFreaking()
        {

            if (_email.to.Count() == 0)
                return true;

            return false;
        }
        public Mailer AddCc(string cc)
        {
            _email.cc.AddRange(cc.Split(splitwords).Select(a => a.Trim()).ToArray());

            return this;
        }

        public Mailer AddCc(string[] cc)
        {
            _email.cc.AddRange(cc);

            return this;
        }

        public Mailer AddBcc(string bcc)
        {
            _email.bcc.AddRange(bcc.Split(splitwords).Select(a => a.Trim()).ToArray());

            return this;
        }

        public Mailer AddBcc(string[] bcc)
        {
            _email.bcc.AddRange(bcc);

            return this;
        }

        public Mailer AddReplayTo( string reps)
        {
            _email.reply = reps;
            return this;
        }
        public Mailer DisableThreadTreat()
        {
            _email.MultiThread = false;
            return this;
        }
        List<string> ToBK{get;set;} = new List<string>();
        List<string> ccBK{get;set;} = new List<string>();
        void Override4Test()
        {

            String dest_to = _iConf.GetTestRecipient().FirstOrDefault();
            if (_iConf.IsTestMode())
            {
                ToBK = _email.to;
                ccBK = _email.cc;

                _email.subject += " (To: " + string.Join(";", ToBK) + ")";
                _email.to = new List<string>(){dest_to};

                _email.subject += " (Cc: " + string.Join(";", ccBK) + ")";
                _email.cc  = new List<string>(){dest_to};

            }
        }
        void RestoreAfterTest()
        {
            if (_iConf.IsTestMode())
            {
                _email.to = ToBK;
                _email.cc = ccBK;
            }
        }

        public Mailer EnableDeleteAttachments()
        {
            _email.DeleteAttachements = true;
            return this;
        }
        public Mailer DisableDeleteAttachments()
        {
            _email.DeleteAttachements = false;
            return this;
        }
        // public Mailer EnableHtmlText()
        // {
        //     _email.IsConvertBodyToHTML = true;
        //     return this;
        // }

        public Mailer AddAttachmentPath(params string[] attachmentPath)
        {
            _email.attachments.AddRange(attachmentPath);
            return this;
        }


        public Mailer Fire(string save_path = "")
        {
            logger.Info("Start sending email");

            string current = string.Empty;

            try{
                if (isFreaking())
                    throw new ArgumentException("HeyHey! Check blockEmailTest  in web.config and recipients");

                logger.Debug("To: {0}, cc: {1}, from: {2}, bcc: {3}, reply: {4}",
                        string.Join(";", _email.to.ToArray()),
                        string.Join(";", _email.cc.ToArray()),
                        _email.from,
                        string.Join(";", _email.bcc.ToArray()),
                        _email.reply);

                Override4Test();

                PushErrorAddress();

                _email.body = _email.body.Replace("\r\n", "").Replace("\t", "").Replace("\n", "");

                _email.Send(save_path);

                RestoreAfterTest();

                if ( _email.MultiThread )
                    logger.Info("Email has been sent ( thread )");
                else
                    logger.Info("Email has been sent");
            }
            catch(Exception ex)
            {
                logger.Error(ex,"failed to send email, To: {0}, cc: {1}, from: {2}, bcc: {3}, repley: {4}",
                        string.Join(";",_email.to.ToArray()),
                        string.Join(";",_email.cc.ToArray()),
                        _email.from,
                        string.Join(";",_email.bcc.ToArray()),
                        _email.reply);
            }
            finally
            {

            }

            return this;
        }

        void PushErrorAddress()
        {
            _unsent =  _email.to.Where( x=>{
                bool okay = false;

                try
                {
                    okay = (new System.Net.Mail.MailAddress(x).Address != x) || !new EmailAddressAttribute().IsValid(x);
                }
                catch
                {
                    okay = true;
                }

                return okay;

            }).ToList();

            if( _unsent.Count() > 0 )
                logger.Info("Error address : {0}",_unsent.Count());
        }

        public Mailer Notify4Invalid(string message = "")
        {

            if( _unsent.Count() == 0 ) return this;

            try{
                string body = string.Format("The System found invalid email recipent(s)<br /><br />{0}<br /><br />{1}<br /><br />Please check it.", string.Join("<br />",_unsent), message);

                SMTPSettings smtp = new SMTPSettings();
                smtp.Password = _iConf.GetAPIPassword();
                smtp.ServerAddress = _iConf.GetServer();
                smtp.User = _iConf.GetAPIUserName();

                var email = new Spider.Net.Email(smtp);
                email.subject = "[SMS System] Notification invalid address found";
                email.body = body;
                email.MultiThread = false;

                email.to.AddRange(_iConf.GetNotifyTo());

                email.Send();

                logger.Info("Email has been sent ( thread )");

                _unsent = new List<string>();

                }
                catch(Exception ex)
                {
                    logger.Error(ex);
                }

            return this;
        }

		public Mailer AddSubject(string subject)
		{
			_email.subject = subject;
			return this;
		}

		public Mailer AddBody(string body)
		{
			_email.body = body;

			return this;
		}

		public Mailer AddTo(string to)
		{
			 _email.to.AddRange(to.Split(splitwords).Select(a => a.Trim()).ToArray());

			 return this;
		}

		public Mailer AddTo(string[] to)
		{
			_email.to.AddRange(to);

			return this;
		}
	}
}