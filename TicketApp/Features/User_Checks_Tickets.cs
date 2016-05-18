using Catalyst;
using System;

namespace TicketApp
{
    class User_Checks_Tickets : Feature
    {
        public User_Checks_Tickets()
        {
            Background(() =>
            {
                //using (var db = new Db())
                //{
                //    db.Tickets.Add(new Ticket { Subject = "Lol" });
                //    db.Rebuild();
                //}
            });

            Scenario("User logs in to checking existing tickets", () =>
            {
                //User visits homepage
                this.Visit("/");

                //Click check my tickets
                //this.HasText("Check my2 tickets");

                //this.Fill("lol", "ok");

                //Login
                //this.HasText("Check my? tickets");
            });

            //Scenario("User comments on existing ticket", () =>
            //{
            //    this.Visit("/requests");
            //    this.Click("desk broken");

            //    this.Fill("Comment", with: "Mijn commentaar");
            //    this.Submit("Send");

            //    this.HasText("Mijn commentaar");
            //});

            Scenario("User", () =>
            {
                throw new Exception("BROKEN!");
            });
        }
    }
}
