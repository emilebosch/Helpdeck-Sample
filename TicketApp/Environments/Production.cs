using System;
using System.Data.Entity;
using Catalyst;

namespace TicketApp
{
    public class Production : AppEnvironment
    {
        public void Setup()
        {
            Database.SetInitializer<Db>(new TicketInitializer());

            App.Add<EmailService, ConcreteEmailService>();
            App.Add<UserService, ConcreteUserLoginService>();
            App.Add<IssueService, ConcreteIssueService>();
            
            App.Get<UserService>().Tap(service =>
            {
                service.Register("admin", "admin", "admin@admin.com", UserRole.Agent, requireConfirm: false);
            });

            new Scheduler().Tap(s =>
            {
                s.Schedule<EmailProcessJob>(TimeSpan.FromSeconds(10));
            })
            .Start();
        }
    }
}
