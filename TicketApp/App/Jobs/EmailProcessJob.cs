using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Net.Security;
using System.Data.Entity;
using Limilabs.Mail;
using Limilabs.Client.IMAP;
using Limilabs.Client;
using Limilabs.Mail.Fluent;
using Limilabs.Client.SMTP;
using Catalyst;

namespace TicketApp
{
    public class EmailProcessJob : IJob
    {
        public override string ToString()
        {
            return "Processing incoming e-mails";
        }

        public void Execute()
        {
            var mailer = App.Get<EmailService>();
            var mails = mailer.Read();

            using (var db = new Db())
            {
                foreach (var mail in mails)
                {
                    var messageParts = Regex.Match(mail.Text, "\\[Ticket #(\\d+)\\]");
                    var message = ParseLastReply(mail);
                    Ticket ticket;

                    var email = mail.From.FirstOrDefault();
                    var requestee = db.Users.FirstOrDefault(a => a.Email == email.Address);
                    if (requestee == null)
                    {
                        //We don't know this person, let's add him i.e. register this person
                        requestee = db.Users.Add(new User { Name = String.IsNullOrEmpty(email.Name) ? email.Address : email.Name, Email = email.Address, Password = "" });
                        db.SaveChanges();
                    }

                    if (messageParts.Success)
                    {
                        int id = Convert.ToInt32(messageParts.Groups[1].Value);
                        ticket = db.Tickets.Include(t => t.Comments).FirstOrDefault(a => a.Id == id);
                        ticket.Updated = DateTime.Now;
                    }
                    else
                    {
                        message = mail.GetTextFromHtml();
                        ticket = new Ticket
                        {
                            Subject = mail.Subject,
                            RequestedBy = requestee,
                            Updated = DateTime.Now,
                            Created = DateTime.Now
                        };

                        mail.Attachments.ForEach
                            (a => ticket.Attachments.Add(new Attachment { Contents = a.Data, Name = a.FileName }));
                       
                        db.Tickets.Add(ticket);
                    }
                    
                    var comment = new Comment { Created = DateTime.Now, Description = message, CommentedBy = requestee };
                    mail.Attachments.ForEach
                        (a => comment.Attachments.Add(new Attachment { Contents = a.Data, Name = a.FileName })); 
                    ticket.Comments.Add(comment);
                    db.SaveChanges();
                }
            }
        }

        private string ParseLastReply(IMail mail)
        {
            string message = mail.Text;
            var splittedOn = Regex.Split(mail.Text, "\n(.*?)\\<(.*?)\\>(.*?):(.*?)\n(.*?)");
            if (splittedOn.Count() > 1)
            {
                message = splittedOn[0];
            }
            else
            {
                splittedOn = Regex.Split(mail.Text, "\n(.*?):\\w(.*?)\n");
                message = splittedOn[0];
            }
            return message.Trim();
        }
    }
}