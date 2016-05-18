using Catalyst;

namespace TicketApp
{
    class Agent_Escalate_Tickets : Feature
    {
        public Agent_Escalate_Tickets()
        {
            Background(() =>
            {
                using (var db = new Db())
                {
                    db.Rebuild();
                    db.Tickets.Add(new Ticket { Subject = "Sample Ticket!" });
                    db.SaveChanges();
                }
            });

            Scenario("Create a new JIRA issue from ticket", () =>
            {
                //When check the sample issue
                this.Visit("/tickets/1");
      
                //And i dont see that we have a connected Jira issue
                this.HasText("JIRA:", invert:true);

                //Then i click the sharing button (in an lightbox)
                this.Click("Share");

                //When in the issue type to create
                this.Fill("Type", with: "Bug");

                //And create
                this.Submit("Create Jira Issue");

                //Then I should see a success message
                this.HasText("JIRA issue created!");

                //And when i visit the ticket page
                this.Visit("/tickets/1");

                //Then i should see the text Jira issue
                this.HasText("JIRA:");
            });

            Scenario("Unlink from JIRA issue", () =>
            {
                //Visit the view issue page
                this.Visit("/tickets/1");

                //Check if we have  a linked ticket
                this.HasText("JIRA");

                //Unlink the ticket
                this.Click("Unshare!");
                
                //Visit the page again
                this.Visit("/tickets/1");

                //Should have jira text
                this.HasText("JIRA:", invert:true);
            });

            Scenario("Add to existing JIRA issue from ticket", () =>
            {
                //Visit the the sample issue
                this.Visit("/tickets/1");
                this.HasText("JIRA:", invert:true);

                //Load the sharing page (in an lightbox)
                this.Click("Share");

                //Select an existing ticket
                this.Select("JIRA Issue (OD-0)");

                //Pres link to link it to an existing issue
                this.Submit("Link Jira Issue");

                //Check if creation message is shown
                this.HasText("JIRA issue linked!");

                //Check if we have a issue linked on the ticket page 
                this.Visit("/tickets/1");

                //Should have the JIRA text
                this.HasText("JIRA:");
            });

        }
    }
}