using Catalyst;

namespace TicketApp
{
    public class Agent_Handles_Incoming_Calls : Feature
    {
        public Agent_Handles_Incoming_Calls()
        {
            Background(() =>
            {
                using (var db = new Db())
                {
                    db.Rebuild();
                    db.Users.Add(new User
                    {
                        Phone = "31534567899",
                        Name = "Jan Jaap",
                        Tickets = 
                        { 
                            new Ticket { Subject = "Jan open ticket1", State = TicketState.Open },
                            new Ticket { Subject = "Jan open ticket2", State = TicketState.Open } 
                        }
                    });
                    db.Users.Add(
                    new User
                    {
                        Phone = "31534567890",
                        Name = "Emile Bosch",
                        Tickets = 
                        { 
                            new Ticket { Subject = "Emile open ticket", State = TicketState.Open } 
                        }
                    });
                    db.SaveChanges();
                }
                App.Add<UserService, ConcreteUserLoginService>();
            });

            Scenario("Unknown customer calling", () =>
            {
                Given("Customer calls 3161122334455",
                    () => PlaceIncomingCall("3161122334455"));

                When("I visit the calls page",
                    () => this.Visit("/calls"));

                Then("I should see unkown customer calling",
                    () => this.HasText("Unknown customer 3161122334455 calling"));

                When("I press accept",
                    () => this.Click("Accept 3161122334455"));

                Then("I should see the create ticket screen",
                    () => this.HasText("Create ticket"));
            });

            Scenario("Known customer calling with an one unresolved ticket", () =>
            {
                Given("Customer calls 31534567890",
                    () => PlaceIncomingCall("31534567890"));

                Given("I visit the calls page",
                    () => this.Visit("/calls"));

                Then("I should see Emile Bosch calling",
                    () => this.HasText("Emile Bosch calling"));

                When("I accept this call",
                    () => this.Click("Accept Emile Bosch"));

                Then("I should see Emile open ticket",
                    () => this.HasText("Emile open ticket"));
            });

            Scenario("Known customer calls with more than one ticket", () =>
            {
                Given("Customer calls 31534567899",
                    () => PlaceIncomingCall("31534567899"));

                When("I visit the calls page",
                    () => this.Visit("/calls"));

                Then("I should see Jan Jaap calling",
                    () => this.HasText("Jan Jaap calling"));

                When("I accept this call",
                    () => this.Click("Accept Jan Jaap"));

                Then("I should see Jan Jaap's user info",
                     () => this.HasText("Jan Jaap user"));
            });

            Scenario("Known customer hanging up with a voicemail", () =>
            {
                //Given there are no agents available (?)

                //Given a customer calls
                //PlaceIncomingCall("+123456");

                ////When i press option one for voicemail
                //PressKey("1");

                ////And i record my messsge (do callback)
                //RecordMessage();

                ////A ticket should be made with the voicemail embedded
                //this.Visit("/calls");

                ////When i click the link
                //this.Click("Voicemail Ticket from Emile Bosch");

                ////I should see the voicemail message attached
                //this.HasText(".mp3");

            });

            Scenario("Customer hanging up with a call me back", () =>
            {
                //PlaceIncomingCall("+123456");
            });

            Scenario("Customer hanging up with nothing", () =>
            {
                //PlaceIncomingCall("+123456");
            });
        }

        private void RecordMessage()
        {
        }

        private void PressKey(string p)
        {
        }

        private void PlaceIncomingCall(string p)
        {
            this.Post("/calls/twilio/request", w =>
            {
                w.FormValue("CallSid", "23");
                w.FormValue("AccountSid", "123");
                w.FormValue("CallStatus", "ringing");
                w.FormValue("From", p);
            });
        }
    }
}
