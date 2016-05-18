using System;
using System.Linq;
using System.Data.Entity;
using Nancy;
using Nancy.ModelBinding;
using Limilabs.Mail.Fluent;
using System.IO;
using Catalyst;

namespace TicketApp
{
    public class TicketModule : NancyModule
    {
        public TicketModule()
        {
            Get["/attachments/{id}"] = p =>
            {
                Attachment attachment = null;
                using (var context = new Db())
                {
                    attachment = context.Attachments.Find((int)p.id);
                }
                return Response.FromStream(new MemoryStream(attachment.Contents), "image/jpg");
            };


            Get["/tickets"] = p =>
            {
                Ticket[] tickets;
                using (var context = new Db())
                {
                    tickets = context.Tickets.Include(t => t.AssignedTo).ToArray();
                }
                return View["App/Views/Tickets/Index.cshtml", tickets];
            };

            Get["/tickets/{id}/claim"] = p =>
            {
                int id = p.id;
                Ticket ticket;
                var userloginService = App.Get<UserService>();

                using (var context = new Db())
                {
                    ticket = context.Tickets
                           .Include(t => t.Comments.Select(c => c.CommentedBy))
                           .Include(t => t.AssignedTo)
                           .FirstOrDefault(a => a.Id == id);
                    ticket.AssignedTo = userloginService.GetLoggedInUser();
                    context.SaveChanges();
                }
                Flash.SetFlash("You've claimed this!");
                return Response.AsRedirect("/tickets/");
            };

            Get["/tickets/{id}"] = p =>
            {
                int id = p.id;
                Ticket ticket;
                using (var context = new Db())
                {
                    ticket = context.Tickets
                        .Include(t => t.Attachments)
                        .Include(t => t.RequestedBy)
                        .Include(t => t.Comments.Select(a => a.CommentedBy))
                        .Include(t => t.Comments.Select(a => a.Attachments))
                        .Include(t => t.AssignedTo)
                        .FirstOrDefault(t => t.Id == id);
                }
                return View["app/views/tickets/view.cshtml", ticket];
            };

            Get["/tickets/{id}/delete"] = p =>
            {
                int id = p.id;
                using (var context = new Db())
                {
                    var ticket = context.Tickets.FirstOrDefault(a => a.Id == id);
                    context.Tickets.Remove(ticket);
                    context.SaveChanges();
                }
                Flash.SetFlash("Ticket deleted!");
                return Response.AsRedirect("/tickets/");
            };

            Post["/tickets/{id}/update"] = p =>
            {
                int id = p.id;
                Ticket ticket;
                Comment comment = null;

                using (var context = new Db())
                {
                    var updatedTicket = this.Bind<Ticket>();
                    var userservice = App.Get<UserService>();

                    ticket = context.Tickets
                        .Include(t => t.Comments)
                        .Include(t => t.AssignedTo)
                        .Include(t => t.RequestedBy)
                        .FirstOrDefault(a => a.Id == id);

                    ticket.Priority = updatedTicket.Priority;
                    ticket.State = updatedTicket.State;
                    ticket.Type = updatedTicket.Type;
                    ticket.Subject = updatedTicket.Subject;
                    ticket.Updated = DateTime.Now;

                    if (ticket.AssignedTo == null)
                    {
                        ticket.AssignedTo = userservice.GetLoggedInUser();
                    }

                    var commentText = (string)this.Request.Form.comment;
                    if (!String.IsNullOrEmpty(commentText))
                    {
                        bool @private = (bool)this.Request.Form.@private;
                        comment = ticket.Comments.Add<Comment>(new Comment { Private = @private, Created = DateTime.Now, Description = commentText, CommentedBy = userservice.GetLoggedInUser() });
                        if (!comment.Private)
                        {
                            var mailservice = App.Get<EmailService>();
                            if (ticket.RequestedBy != null && ticket.RequestedBy.Email != null)
                            {
                                var email = Mail
                                 .Html(String.Format("--- Please type above this line --- <br>Regarding [Ticket #{0}]:<br><br>{1}", ticket.Id, comment.Description))
                                 .To(ticket.RequestedBy.Email)
                                 .Subject(ticket.Subject)
                                 .Create();
                                mailservice.Send(email);
                            }

                        }
                    }
                    context.SaveChanges();
                }
                return Response.AsRedirect("/tickets");
            };

            Get["/tickets/new"] = p =>
            {
                return View["app/views/tickets/new.cshtml"];
            };

            Post["/tickets/new"] = p =>
            {
                var ticket = this.Bind<Ticket>();
                using (var context = new Db())
                {
                    var user = App.Get<UserService>().GetLoggedInUser();
                    ticket.RequestedBy = user;
                    ticket.Created = DateTime.Now;
                    ticket.Updated = DateTime.Now;
                    context.Tickets.Add(ticket);
                    ticket.Comments.Add(new Comment { Description = this.Request.Form.Summary, CommentedBy = user });
                    context.SaveChanges();
                }
                Flash.SetFlash("Successfully created ticket: {0}".Inject(ticket.Subject));
                return Response.AsRedirect("/tickets");              
            };
        }
    }
}