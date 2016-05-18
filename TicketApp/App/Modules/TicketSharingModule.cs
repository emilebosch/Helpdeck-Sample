using System;
using Nancy;
using System.Data.Entity;
using System.Linq;
using System.Collections.Generic;
using Catalyst;

namespace TicketApp
{
    public class TicketSharingModule : NancyModule
    {
        public TicketSharingModule()
        {
            Get["/tickets/{id}/share"] = p =>
            {
                int id = p.id;
                object model;

                var service = App.Get<IssueService>();
                using (var context = new Db())
                {
                    model = new 
                    {
                        Ticket =  context.Tickets.Include(t=>t.Comments).FirstOrDefault(a => a.Id == id),
                        Issues = service.SearchIssues(string.Empty).ToArray()
                    };

                }
                return View["app/views/tickets/share.cshtml", model];
            };

            Post["/tickets/{id}/share"] = p =>
            {
                int id = p.id;
                Ticket ticket;

                using (var context = new Db())
                {
                    ticket = context.Tickets.Include("Comments").FirstOrDefault(a => a.Id == id);
                    var issueService = App.Get<IssueService>();

                    Issue issue = null;
                    if (this.Request.Form.action == "Create Jira Issue")
                    {
                        issue = issueService.CreateNewIssue(ticket);
                    }
                    else
                    {
                        string issueId = this.Request.Form.issueno;
                        issue = issueService.AppendToExistingIssue(issueId, ticket);
                    }

                    ticket.IssueTicketUrl = issue.Url;
                    ticket.IssueTicketId = issue.Id;
                    context.SaveChanges();
                }
                return this.Request.Form.action == "Create Jira Issue" ? "JIRA issue created!" : "JIRA issue linked!";
            };

            Get["/tickets/{id}/unshare"] = p =>
            {
                int id = p.id;
                Ticket ticket;
                using (var context = new Db())
                {
                    ticket = context.Tickets.Find(id);
                    var issueService = App.Get<IssueService>();
                    issueService.UnlinkIssue(ticket);
                    ticket.IssueTicketUrl = null;
                    context.SaveChanges();
                }
                return "Unshared!";
            };
        }
    }
}
