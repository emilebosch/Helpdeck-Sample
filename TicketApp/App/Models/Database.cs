using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;

namespace TicketApp
{
    public enum TicketState
    {
        Open,
        Pending,
        Resolved
    }

    public enum TicketType
    {
        Incident,
        Question,
        Problem,
        Task
    }

    public enum TicketPriority
    {
        Low,
        Normal,
        High,
        Urgent
    }

    public enum UserRole
    {
        Normal,
        Agent,
        Admin
    }

    public class Call
    {
        [Key]
        public int Id { get; set; }
        public string From { get; set; }
        public DateTime? EnteredQueue { get; set; }
        public User KnownCaller { get; set; }
        public int? CustomerId { get; set; }

        public DateTime? PickupTime { get; set; }
        public User HandledBy { get; set; }
        public int? HandledById;
    }


    public class Attachment
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        [Column(TypeName = "image")]
        public byte[] Contents { get; set; }
        public int TicketId { get; set; }
        public int CommentId { get; set; }
    }

    public class Group
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<User> Users { get; set; }
    }

    public class Organization
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class User
    {
        public User()
        {
            Tickets = new HashSet<Ticket>();
        }

        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Notes { get; set; }
        public string Password { get; set; }
        public string Phone { get; set; }

        public string PasswordResetToken { get; set; }
        public int OrganizationId { get; set; }
        
        public ICollection<Ticket> Tickets { get; set; }
        public UserRole Role { get; set; }
        public int RoleId
        {
            get { return (int)Role; }
            set { Role = (UserRole)value; }
        }
        public string ConfirmationToken { get; set; }
        public bool Locked { get; set; }
    }

    public class Topic
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public Forum Forum { get; set; }
        public int ForumId { get; set; }

        public User CreatedBy { get; set; }
        public DateTime? Created { get; set; }

        public bool PinOnHomepage { get; set; }
        public bool MarkAsFeatured { get; set; }
        public bool AllowComments { get; set; }

        public ICollection<TopicComment> Comments { get; set; }
        public ICollection<TopicVote> Votes { get; set; }
    }

    public class Forum
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<Topic> Posts { get; set; }
    }

    public class TopicVote
    {
        [Key]
        public int Id { get; set; }
        public int TopicId { get; set; }
        public User VotedBy { get; set; }
    }

    public class TopicComment
    {
        [Key]
        public int Id { get; set; }
        public int TopicId { get; set; }
        public string Description { get; set; }
        public User CommentedBy { get; set; }
        public DateTime? Created { get; set; }
    }

    public class Comment
    {
        public Comment()
        {
            Attachments = new HashSet<Attachment>();
        }

        [Key]
        public int Id { get; set; }
        public int TicketId { get; set; }
        public string Description { get; set; }
        public User CommentedBy { get; set; }
        public DateTime? Created { get; set; }
        public bool Private { get; set; }
        public ICollection<Attachment> Attachments { get; set; }

    }

    public class Ticket
    {
        public Ticket()
        {
            Comments = new HashSet<Comment>();
            Attachments = new HashSet<Attachment>();
        }

        [Key]
        public int Id { get; set; }
        public string Subject { get; set; }

        public int Rating { get; set; }
        public User RequestedBy { get; set; }
        public User AssignedTo { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? Updated { get; set; }

        public ICollection<Comment> Comments { get; set; }
        public ICollection<Attachment> Attachments { get; set; }

        public string IssueTicketUrl { get; set; }
        public string IssueTicketId { get; set; }

        public TicketState State { get; set; }
        public int StateId
        {
            get { return (int)State; }
            set { State = (TicketState)value; }
        }

        public TicketPriority Priority { get; set; }
        public int PriorityId
        {
            get { return (int)Priority; }
            set { Priority = (TicketPriority)value; }
        }

        public TicketType Type { get; set; }
        public int TicketTypeId
        {
            get { return (int)Type; }
            set { Type = (TicketType)value; }
        }

        public bool HasIssueTicket()
        {
            return !String.IsNullOrEmpty(IssueTicketUrl);
        }
    }

    public class Db : DbContext
    {
        public Db()
        {
            Configuration.ProxyCreationEnabled = false;
        }

        protected override bool ShouldValidateEntity(System.Data.Entity.Infrastructure.DbEntityEntry entityEntry)
        {
            if (entityEntry.Entity is Attachment)
            {
                return false;
            }
            return base.ShouldValidateEntity(entityEntry);
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Comment>().HasMany<Attachment>(a=>a.Attachments).WithMany();
            modelBuilder.Entity<Ticket>().HasMany<Attachment>(a => a.Attachments).WithMany();
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Topic> Topics { get; set; }
        public DbSet<TopicComment> TopicComments { get; set; }
        public DbSet<TopicVote> TopicVotes { get; set; }
        public DbSet<Forum> Forums { get; set; }
        public DbSet<Call> Calls { get; set; }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<Attachment> Attachments { get; set; }
    }

    public class TicketInitializer : DropCreateDatabaseAlways<Db>
    {    
    }
}
