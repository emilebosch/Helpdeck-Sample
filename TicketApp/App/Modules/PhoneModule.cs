using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Text;
using Nancy;
using Nancy.ModelBinding;
using Twilio.TwiML;
using Catalyst;

namespace TicketApp
{
    public class TwilioModule : NancyModule
    {
        public class TwilioRequest
        {
            public string CallSid { get; set; }
            public string AccountSid { get; set; }
            public string CallStatus { get; set; }
            public string From { get; set; }
            public string Direction { get; set; }
        }

        public TwilioModule()
        {
            Post["/calls/twilio/request"] = t =>
            {
                var request = this.Bind<TwilioRequest>();
                User user = null;
                using (var db = new Db())
                {
                    user = db.Users.FirstOrDefault(u => u.Phone == request.From);
                    db.Calls.Add(new Call { From = request.From, EnteredQueue = DateTime.Now, KnownCaller = user });
                    db.SaveChanges();
                }
                return new TwilioResponse().Tap(r =>
                {
                    r.Say("Welcome at helpdesk");
                }).ToString();
            };

            Get["/calls/"] = t =>
            {
                Call[] calls;
                using (var db = new Db())
                {
                    calls = db.Calls.Include(a => a.KnownCaller).ToArray();
                }
                return View["app/views/calls/index.cshtml", calls];
            };

            Get["/calls/{id}/accept"] = t =>
            {
                int callid = t.id;
                Call call;
                using (var db = new Db())
                {
                    call = db.Calls.Include(a => a.KnownCaller).FirstOrDefault(a => a.Id == callid);
                    call.HandledBy = App.Get<UserService>().GetLoggedInUser();
                    call.PickupTime = DateTime.Now;
                    db.SaveChanges();
                }

                //If we don't know this person we just go to the new ticket screen
                if (call.KnownCaller == null)
                {
                    return Response.AsRedirect("/tickets/new");
                }

                //If we know this person
                Ticket[] open;
                using (var db = new Db())
                {
                    var c = db.Users.Find(call.KnownCaller.Id);
                    open = db.Entry(c)
                        .Collection(a => a.Tickets)
                        .Query()
                        .Where(tt => tt.StateId != (int)TicketState.Resolved).ToArray();
                }

                //If they got one ticket unresolved we go there
                if (open.Length == 1)
                {
                    return Response.AsRedirect("/tickets/{0}/".Inject(open[0].Id));
                }

                //If they have any other number we go the screen of the customer
                return Response.AsRedirect("/users/{0}".Inject(call.KnownCaller.Id));
            };

            Get["/calls/{id}/reject"] = t =>
            {
                //Get the call
                //Set the status to reject
                //Redirect the user to the voicemail

                return null;
            };

        }
    }
}
