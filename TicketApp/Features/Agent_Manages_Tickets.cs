using System;
using System.Linq;
using NUnit.Framework;
using Catalyst;

namespace TicketApp
{
    class Agent_Manages_Tickets : Feature
    {
        public Agent_Manages_Tickets()
        {
            Background(() =>
            {
                using (var db = new Db())
                {
                    db.Rebuild();
                    var userservice = App.Get<UserService>().Tap(s =>
                    {
                        s.Register("Emile Bosch", "Test", "emilebosch@hotmail.com", UserRole.Agent, requireConfirm: false);
                        s.LogIn("emilebosch@hotmail.com", "Test");
                    });
                }
            });

            Scenario("Create an ticket", () =>
            {
                //Create an issue
                this.Visit("/tickets/new");

                this.Fill("Subject", with: "Sample Ticket!");
                this.Fill("Summary", with: "Sample Description!");

                this.SelectOption("Priority", "Urgent");
                this.SelectOption("State", "Resolved");
                this.SelectOption("Type", "Incident");

                //Returns to the create page
                this.Submit("Bla!");

                //Validate
                this.HasText("Successfully created ticket: Sample Ticket!");
                this.Visit("/tickets");

                this.HasText("Sample Ticket!");
            });

            Scenario("View all tickets", () =>
            {
                this.Visit("/tickets");
                this.HasText("View all tickets");
            });

            Scenario("View a single tickets", () =>
            {
                //Visit the issue
                this.Visit("/tickets");
                this.Click("Sample Ticket!");

                this.HasText("Sample Ticket!");
                this.HasText("Sample Description!");
                this.HasText("Unassigned", invert: true);

                this.HasSelectedOption("Urgent");
                this.HasSelectedOption("Resolved");
                this.HasSelectedOption("Incident");
            });

            Scenario("Take an ticket", () =>
            {
                //Visit the issue
                this.Visit("/tickets/1");

                //Make sure its unassigned
                this.HasText("This ticket is not owned, claim it.");
                this.Click("This ticket is not owned, claim it.");

                this.HasText("You've claimed this!");
                this.Visit("/tickets/1");

                //Check if we really are assigned
                this.HasText("Assigned to");
            });

            Scenario("Commenting an ticket (public)", () =>
            {
                App.Get<EmailService>().Clear();

                //Visit the issue
                this.Visit("/tickets/1");
                this.Fill("Comment", with: "This is a public comment");
                this.Submit("Update Ticket!");

                this.Visit("/tickets/1");

                this.HasText("This is a public comment");
                this.HasText("Commented by");
                this.HasText("Emile Bosch");
                this.HasText("Agent");

                //Check if the user has been mailed 
                Assert.IsTrue(App.Get<EmailService>().Read().Count() == 1, "A private comment should not mail the user!");

            });

            Scenario("Commenting an ticket (private)", () =>
            {
                App.Get<EmailService>().Clear();

                this.Visit("/tickets/1");
                this.Fill("Comment", with: "This is a private comment");
                this.Select("Make this comment private");
                this.Submit("Update Ticket!");

                this.Visit("/tickets/1");
                this.HasText("This is a private comment");

                this.HasText("Commented privately");

                //Check the mailer to see if the comment has not been e-mailed!
                Assert.IsTrue(App.Get<EmailService>().Read().Count() == 0, "A private comment should not mail the user!");
            });

            Scenario("Updating an ticket with new state, type and priority", () =>
            {
                //Visit the issue
                this.Visit("/tickets/1");
                this.SelectOption("Priority", with: "Low");
                this.SelectOption("State", with: "Pending");
                this.SelectOption("Type", with: "Question");
                this.Submit("Update Ticket!");

                this.Visit("/tickets/1");

                this.HasSelectedOption("Low");
                this.HasSelectedOption("Pending");
                this.HasSelectedOption("Question");
            });

            Scenario("Delete an single ticket", () =>
            {
                //Visit the issue
                this.Visit("/tickets/1");

                //Delete the issue
                this.Click("Delete");
                this.HasText("Ticket deleted!");

                //Should redirect us to the view issues page
                this.HasText("Sample Ticket!", invert: true);
            });
        }
    }
}
