using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using Nancy;
using Nancy.ModelBinding;
using Catalyst;

namespace TicketApp
{
    public class KnowledgeBaseModule : NancyModule
    {
        public KnowledgeBaseModule()
        {
            Get["/kb"] = p =>
            {
                Forum[] fora;
                using (var db = new Db())
                {
                    fora = db.Forums.Include(t => t.Posts).ToArray();
                }
                return View["app/views/kb/index.cshtml", fora];
            };

            Get["/kb/{id}"] = p =>
            {
                int id = p.id;
                Topic post;
                using (var db = new Db())
                {
                    var currentuser = App.Get<UserService>().GetLoggedInUser();
                    post = db.Topics
                        .Include(a => a.Comments.Select(c => c.CommentedBy))
                        .Include(a => a.CreatedBy)
                        .Include(a => a.Votes)
                        .FirstOrDefault(a => a.Id == id);
                }
                return View["app/views/kb/view.cshtml", post];
            };

            Get["/kb/new"] = p =>
            {
                object model;
                using (var db = new Db())
                {
                    model = new
                    {
                        Article = new Topic(),
                        Forums = db.Forums.ToArray()
                    };
                }
                return View["app/views/kb/new.cshtml", model];
            };

            Get["/kb/new/fromticket/{id}"] = p =>
            {
                object model;
                int id = (int)p.id;
                using (var db = new Db())
                {
                    var ticket = db.Tickets.Include(c=>c.Comments).FirstOrDefault(a=>a.Id==id);
                    model = new 
                    {
                        Article = new Topic
                        {
                            Title = ticket.Subject,
                            Text = String.Concat(ticket.Comments.Select(a=>a.Description+"<br/>"))
                        },
                        Forums = db.Forums.ToArray()
                    };
                }
                return View["app/views/kb/new.cshtml", model];
            };

            Post["/kb/new"] = p =>
            {
                using (var db = new Db())
                {
                    var service = App.Get<UserService>();
                    var article = this.Bind<Topic>();

                    var forum = db.Forums.Find((int)this.Request.Form.forumId);
                    article.Created = DateTime.Now;
                    article.CreatedBy = service.GetLoggedInUser();
                    article.Forum = forum;
                    db.Topics.Add(article);
                    db.SaveChanges();
                }
                return "KB created!";
            };

            Get["/kb/search"] = p =>
            {
                return View["app/views/kb/search.cshtml", new Topic[] { }];
            };

            Post["/kb/search"] = p =>
            {
                Topic[] posts;
                using (var db = new Db())
                {
                    string term = Request.Form.term;
                    posts = db.Topics.Where(a => a.Text.Contains(term) || a.Title.Contains(term))
                        .Take(10)
                        .ToArray();
                }
                return View["app/views/kb/search.cshtml", posts];
            };

            Get["/kb/{id}/recommend"] = p =>
            {
                int id = p.id;
                using (var db = new Db())
                {
                    var userservice = App.Get<UserService>();
                    var post = db.Topics.Include(p2 => p2.Votes).FirstOrDefault(p3 => p3.Id == id);
                    post.Votes.Add(new TopicVote { VotedBy = userservice.GetLoggedInUser() });
                    db.SaveChanges();
                }
                Flash.SetFlash("You've marked this as helpful!");
                return Response.AsRedirect("/kb/{0}".Inject(id));
            };

            Post["/kb/{id}/comment"] = p =>
            {
                int id = p.id;
                using (var db = new Db())
                {
                    var service = App.Get<UserService>();
                    var article = db.Topics.Include(a => a.Comments).FirstOrDefault(a => a.Id == id);

                    var comment = this.Bind<TopicComment>();
                    comment.CommentedBy = service.GetLoggedInUser();
                    comment.Created = DateTime.Now;
                    article.Comments.Add(comment);
                    db.SaveChanges();
                }
                Flash.SetFlash("Thanks for your comment");
                return Response.AsRedirect("/kb/{0}".Inject(id));
            };
        }
    }
}
