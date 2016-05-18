using System;
using Catalyst;

namespace TicketApp
{
    class Agent_Manage_Forums : Feature
    {
        pubflic Agent_Manage_Forums()
        {
            Background(() =>
            {
                using (var db = new Db())
                {
                    db.Rebuild();

                    //Register a user
                    var user = App.Get<UserService>().Tap((s) =>
                    {
                        s.Register("Emile Bosch", "Test", "emilebosch@hotmail.com", UserRole.Agent, requireConfirm: false);
                        s.LogIn("emilebosch@hotmail.com", "Test");
                    }).GetLoggedInUser();

                    //Create some sample posts
                    var desk = db.Forums.Add(new Forum { Name = "desk" });
                    var desk2 = db.Forums.Add(new Forum { Name = "My desk" });

                    //Add some post
                    db.Topics.Add(new Topic { CreatedBy = user, Created = DateTime.Now, Forum = desk, Text = "Sample KB description", Title = "Sample KB title" });
                    db.Tickets.Add(new Ticket { Created = DateTime.Now, RequestedBy = user, Subject = "Ticket Subject" });
                    db.SaveChanges();
                }
            });

            Scenario("Create a knowledge base article", () =>
            {
                //Visit the new page
                this.Visit("/kb/new");

                //Fill the form
                this.Fill("Title", with: "My KB Article");
                this.Fill("Text", with: "My article content");
                this.SelectOption("Forum", "1");

                //Press create
                this.Submit("Create");
                this.HasText("KB created!");

                //Check if we have an knowledge base
                this.Visit("/kb/2");

                //Check if we have more text
                this.HasText("My KB Article");
                this.HasText("My article content");
            });

            Scenario("Create a knowledge base article from a ticket", () =>
            {
                //
                this.Visit("/kb/new/fromticket/1");

                this.SelectOption("Forum", "1"); //TODO: Fix selection (be able to select just from title/label)
                this.Submit("Create");
                this.HasText("KB created!");
                this.Visit("/kb/3");

                this.HasText("Ticket Subject");
            });

            Scenario("View knowledge base portal", () =>
            {
                //Check the knowledge base homepage
                this.Visit("/kb");

                //Check if article is there
                this.HasText("Sample KB title");

                //Click the KB article
                this.Click("Sample KB title");

                //Check if the description is there
                this.HasText("Sample KB description");
            });

            Scenario("Search the knowledge base via homepage", () =>
            {
                //Check the knowledge base homepage
                this.Visit("/kb");

                this.Fill("Search for", with: "KB");
                this.Submit("Search");

                //Check if article is there
                this.HasText("Sample KB title");
            });

            Scenario("Search the knowledge base via search", () =>
            {
                //Check the knowledge base homepage
                this.Visit("/kb/search");

                this.Fill("Search for", with: "KB");
                this.Submit("Search");

                //Check if article is there
                this.HasText("Sample KB title");
            });

            Scenario("Mark knowledge base item as helpful", () =>
            {
                //Visit an kb
                this.Visit("/kb/1");

                //Click the i found this helpful link
                this.Click("I found this helpful!");

                //Check if it registerd
                this.HasText("You've marked this as helpful!");

                this.Visit("/kb/1");
                this.HasText("You find this helpful");
            });

            Scenario("Comment on an knowledge base item ", () =>
            {
                //Create a comment
                this.Visit("/kb/1");
                this.Fill("Your comment", with: "This is my KB comment!");
                this.Submit("Create!");

                //Check that you've got your comment
                this.HasText("Thanks for your comment");

                this.Visit("/kb/1");
                this.HasText("This is my KB comment!");
            });
        }
    }
}