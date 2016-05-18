using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using Moq;
using Limilabs.Mail;
using Catalyst;

namespace TicketApp
{
    public class Test : AppEnvironment
    {
        public void Setup()
        {
            Database.SetInitializer<Db>(new TicketInitializer());

            App.Add<UserService, ConcreteUserLoginService>();
            App.Add<EmailService>(new Mock<EmailService>().Tap(mock =>
            {
                var mails = new List<IMail>();
                mock.Setup(s => s.Send(It.IsAny<IMail>())).Callback((IMail mail) => mails.Add(mail));
                mock.Setup(s => s.Read()).Returns(mails);
                mock.Setup(s => s.Clear()).Callback(() =>
                {
                    mails.Clear();
                });
            }).Object);

            App.Add<IssueService>(new Mock<IssueService>().Tap(mock =>
            {
                var issues = new List<Issue>();
                mock.Setup(s => s.SearchIssues(It.IsAny<string>())).Returns(issues);
                mock.Setup(s => s.UnlinkIssue(It.IsAny<Ticket>()));
                mock.Setup(s => s.AppendCommentToIssue(It.IsAny<Ticket>(), It.IsAny<Comment>()));
                mock.Setup(s => s.AppendToExistingIssue(It.IsAny<String>(), It.IsAny<Ticket>()))
                    .Returns((string id, Ticket t) => issues.FirstOrDefault(i => i.Id == id));
                mock.Setup(s => s.CreateNewIssue(It.IsAny<Ticket>()))
                    .Returns((Ticket t) =>
                    {
                        return issues.Add<Issue>(new Issue { Id = "OD-" + issues.Count(), Url = "http://mock/" + issues.Count() });
                    });

            }).Object);
        }
    }

}
