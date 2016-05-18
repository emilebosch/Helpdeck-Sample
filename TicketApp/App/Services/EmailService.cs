using System;
using System.Collections.Generic;
using System.Net.Security;
using Limilabs.Mail;
using Limilabs.Client.SMTP;
using Limilabs.Client.IMAP;
using Limilabs.Client;

namespace TicketApp
{
    public interface EmailService
    {
        IEnumerable<IMail> Read();
        void Send(IMail mail);
        void Clear();
    }

    public class ConcreteEmailService : EmailService
    {
        public ConcreteEmailService()
        {
            SMTPPassword = "XXX";
            SMTPUser = "XX";
            SMTPServer = "smtp.gmail.com";
            IMAPServer = "imap.gmail.com";
            IMAPUser = "sprintotesto";
            IMAPPassword = "XXX!";
            EmailAdres = "XXX@gmail.com";
        }

        public IEnumerable<IMail> Read()
        {
            var mails = new List<IMail>();
            using (var client = new Imap())
            {
                client.ServerCertificateValidate += (object sender, ServerCertificateValidateEventArgs e) =>
                {
                    const SslPolicyErrors ignoredErrors = SslPolicyErrors.RemoteCertificateChainErrors | SslPolicyErrors.RemoteCertificateNameMismatch;
                    if ((e.SslPolicyErrors & ~ignoredErrors) == SslPolicyErrors.None)
                    {
                        e.IsValid = true;
                        return;
                    };
                    e.IsValid = false;
                };

                client.ConnectSSL(IMAPServer);
                client.Login(IMAPUser, IMAPPassword);
                client.SelectInbox();

                var uids = client.SearchFlag(Flag.Unseen);
                foreach (long uid in uids)
                {
                    string message = client.GetMessageByUID(uid);
                    var mail = new MailBuilder().CreateFromEml(message);
                    mails.Add(mail);
                }
                client.Close();
            }
            return mails;
        }

        public void Send(IMail email)
        {
            email.From.Add(new Limilabs.Mail.Headers.MailBox(EmailAdres));
            using (var smtp = new Smtp())
            {
                smtp.Connect(SMTPServer);
                smtp.UseBestLogin(SMTPUser, SMTPPassword);
                smtp.SendMessage(email);
                smtp.Close();
            }
        }

        public void Clear()
        {
            //Only used for testing
        }

        public string SMTPPassword { get; set; }
        public string SMTPUser { get; set; }
        public string SMTPServer { get; set; }
        public string IMAPServer { get; set; }
        public string IMAPUser { get; set; }
        public string IMAPPassword { get; set; }
        public string EmailAdres { get; set; }
    }
}
