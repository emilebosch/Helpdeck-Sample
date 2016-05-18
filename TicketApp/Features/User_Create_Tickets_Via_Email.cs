using Limilabs.Mail.Fluent;
using Catalyst;

namespace TicketApp
{
    public class Manage_Tickets_Via_Email_Feature : Feature
    {
        public Manage_Tickets_Via_Email_Feature()
        {
            Background(() =>
            {
                using (var db = new Db())
                {
                    db.Rebuild();
                }
                App.Get<EmailService>().Clear();
            });

            Scenario("Create a new ticket via e-mail with attachment", () =>
            {
                CreateEmail("test@gmail.com", "Ticket via e-mail", "Test description", @"Features\Support\Assets\banner.jpg");
                new EmailProcessJob().Execute();

                this.Visit("/tickets/1");                
                this.HasText("Ticket via e-mail");
                this.HasText("banner.jpg");
            });

            Scenario("Append to an existing ticket via email with attachments", () =>
            {
                CreateEmail("test@gmail.com", "RE: Ticket via e-mail", "Test comment [Ticket #1]", @"Features\Support\Assets\banner2.jpg");
                new EmailProcessJob().Execute();
                
                this.Visit("/tickets/1");
                this.HasText("Test comment");
                this.HasText("banner2.jpg");
            });
        }

        private void CreateEmail(string user, string title, string message, string image = null)
        {
            var service = App.Get<EmailService>(); ;
            var builder = Mail
               .Html(message)
               .To("sprintotesto@gmail.com")
               .From(user)
               .Subject(title);

            if (image != null)
            {
                builder.AddAttachment(image);
            }               
            
            var email = builder.Create();
            service.Send(email);
        }
    }
}