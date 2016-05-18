using System;
using System.Collections.Generic;
using System.Linq;
using Atlassian.Jira;

namespace TicketApp
{
    public class Issue
    {
        public string Id { get; set; }
        public string Url { get; set; }
    }

    public interface IssueService
    {
        Issue CreateNewIssue(Ticket ticket);
        Issue AppendToExistingIssue(string issueId, Ticket ticket);
        IEnumerable<Issue> SearchIssues(string query);
        void AppendCommentToIssue(Ticket ticket, Comment comment);
        void UnlinkIssue(Ticket ticket);
    }

    public class ConcreteIssueService : IssueService
    {
        public ConcreteIssueService()
        {
            JiraServer = "https://sprintrepubliq.atlassian.net";
            TicketServer = "http://localhost:12345/tickets/";
            User = "emilebosch";
            Password = "Test123!";
        }

        private Jira Connect()
        {
            return new Jira(JiraServer, User, Password);
        }

        public Issue CreateNewIssue(Ticket ticket)
        {
            var jira = Connect();
            var issue = jira.CreateIssue("OD");
            issue.Type = "Bug";
            issue.Summary = ticket.Subject;
            issue.CustomFields.Add("TicketAmount",new string[]{ "1" });
            issue.CustomFields.Add("TicketLinks", new String[] { TicketServer + ticket.Id +"\n" });
            issue.SaveChanges(); 
            return new Issue { Id = issue.Key.Value, Url = jira.Url + "/browse/" + issue.Key };
        }

        public Issue AppendToExistingIssue(string issueId, Ticket ticket)
        {
            var jira = Connect();
            var issue = jira.GetIssue(issueId);
            int cnt = 0;
            var customFieldAmount = issue.CustomFields["TicketAmount"];
            if (customFieldAmount == null)
            {
                issue.CustomFields.Add("TicketAmount", new string[] { "1" });
                issue.CustomFields.Add("TicketLinks", new String[] { TicketServer + ticket.Id + "\n" });
            }
            else
            {
                var value = customFieldAmount.Values.FirstOrDefault();
                cnt = (int.Parse(value) + 1);
                customFieldAmount.Values[0] = cnt.ToString();
                issue.CustomFields["TicketLinks"].Values[0] += TicketServer + ticket.Id + "\n";

            }
            issue.SaveChanges();
            return new Issue { Id = issue.Key.Value, Url = jira.Url + "/browse/" + issue.Key };
        }

        public IEnumerable<Issue> SearchIssues(string query)
        {
            var jira = Connect();
            var mapped = (from i in jira.Issues orderby i.Created select i).ToArray();
            return mapped.Select((a) => new Issue() { Id = a.Key.Value, Url = a.Key.Value }).ToArray();
        }

        public void AppendCommentToIssue(Ticket ticket, Comment comment)
        {
            var jira = Connect();
            var issue = jira.GetIssue(ticket.IssueTicketId);
            var comments = issue.GetComments();
            issue.AddComment(comment.Description);
        }

        public void UnlinkIssue(Ticket ticket)
        {
            var jira = Connect();
            var issue = jira.GetIssue(ticket.IssueTicketId);
            var noIncidents = int.Parse(issue.CustomFields["TicketAmount"].Values[0])-1;
            issue.CustomFields["TicketAmount"].Values[0] = noIncidents!=0 ? noIncidents.ToString() : "";
            issue.CustomFields["TicketLinks"].Values[0] = issue.CustomFields["TicketLinks"].Values[0]
                .Replace(TicketServer + ticket.Id + "\n", "");
            issue.SaveChanges();
        }    

        public string TicketServer { get; set; }
        public string JiraServer { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
    }
}