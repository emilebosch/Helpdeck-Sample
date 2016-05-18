using System;
using System.Collections.Generic;
using Nancy;
using Catalyst;

namespace TicketApp
{
    public enum ProjectType
    {
        Site,
        App,
        Something
    }

    public enum Gender
    {
        Male,
        Female
    }

    public class Project : IModel, IValidatable
    {
        public Project()
        {
            Tasks = new HashSet<Task>();
            Location = new Catalyst.Location();
            validator = new Validator(v =>
            {
                v.ErrorIf(Name.IsEmpty(), "name", "Name should not be empty");
                v.ErrorIf(Location.Lattitude != Location.Longitude, "location", "Lats hihger than long");
            });
        }

        public int Aantal { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public ProjectType Type { get; set; } 
        public Gender Gender { get; set; }
        public ProjectType[] Types { get; set; }
        public ICollection<Task> Tasks { get; set; }
        public Location Location { get; set; }
        public string Description { get; set; }

        Validator validator;
        public Dictionary<string, List<string>> Errors { get { return validator.Errors; } }
        public bool IsValid() { return validator.IsValid(); }

        public void UpdateRelated(Relation relation, SimpleHash data)
        {
            var tasks = new List<Task>(Tasks);
            for (int i = 0; i < Tasks.Count; i++)
            {
                tasks[i].UpdateAttributes(data[i]);
            }
        }
    }

    public class Task : IModel, IValidatable
    {
        Validator validator;
        public bool Active { get; set; }
        public string Name { get; set; }

        public Location Location { get; set; }

        public Task()
        {
            Location = new Location();
            validator = new Validator(v =>
            {
                v.ErrorIf(Name.IsEmpty(), "name", "Name should not be empty");
            });
        }

        public bool IsValid() { return validator.IsValid(); }
        public Dictionary<string, List<string>> Errors { get { return validator.Errors; } }

        public void UpdateRelated(Relation relation, SimpleHash hash)
        {
            
        }
    }

    public class HomeModule : NancyModule
    {
        public HomeModule()
        {
            Get["/"] = p =>
            {
                return View["app/views/home/index.cshtml"];
            };

            Post["/home"] = p =>
            {
                var parameters = this.Request.Parameters();
                var project = GetProject();

                project.UpdateAttributes(parameters["project"]);
                if (!project.IsValid())
                {
                    return View["app/views/home/view.cshtml", project];
                }

                return "OK Saved!";
            };

            Get["/home"] = p =>
            {
                var person = GetProject();
                return View["app/views/home/view.cshtml", person];
            };
        }

        private static Project GetProject()
        {
            var person = new Project()
            {
                Tasks = {  new Task {}, new Task {} }
            };
            return person;
        }
    }
}
