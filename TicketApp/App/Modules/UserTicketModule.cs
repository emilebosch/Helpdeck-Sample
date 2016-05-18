using System.Data.Entity;
using System.Linq;
using Nancy;

namespace TicketApp
{
    public class UserTicketModule : NancyModule
    {
        public UserTicketModule()
        {
            Get["/requests"]  = p =>
            {
                Ticket[] tickets;
                using (var context = new Db())
                {
                    tickets = context.Tickets.Include(t => t.AssignedTo).ToArray();
                }
                return View["App/Views/Requests/Index.cshtml", tickets];
            };

            Get["/requests/{id}"] = p =>
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

            Post["/request/{id}/comment"] = t =>
            {
                return null;
            };
        }
    }
}
