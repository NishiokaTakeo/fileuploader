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
    public interface IMailClient
    {
        bool isFreaking();
		Mailer AddSubject(string subject);
		Mailer AddBody(string body);
		Mailer AddTo(string cc);
		Mailer AddTo(string[] cc);
        Mailer AddCc(string cc);

        Mailer AddCc(string[] cc);

        Mailer AddBcc(string bcc);

        Mailer AddBcc(string[] bcc);

        Mailer AddReplayTo( string reps);

        Mailer DisableThreadTreat();
        Mailer EnableDeleteAttachments();

        Mailer DisableDeleteAttachments();
        //Mailer EnableHtmlText();


        Mailer AddAttachmentPath(params string[] attachmentPath);


        Mailer Fire(string save_path = "");

        Mailer Notify4Invalid(string message = "");

    }
}