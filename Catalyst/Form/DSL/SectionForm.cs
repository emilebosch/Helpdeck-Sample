using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Catalyst
{
    public class SectionForm :IEnumerable<Section>
    {
        public SectionForm()
        {
            Sections = new List<Section>();
        }
        public void Add(Section section) {
            Sections.Add(section);
        }

        public List<Section> Sections { get; set; }

        public IEnumerator<Section> GetEnumerator()
        {
            return Sections.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return Sections.GetEnumerator();
        }

        public string Render(Form fb)
        {
            return FormRenderer.Render(this, fb);
        }
    }

    public class Section : IEnumerable<SectionField>
    {
        
        public string Title { get; set; }
        public string Description { get; set; }
        public List<SectionField> Fields { get; set; }

        public Section(string title="",string description="")
        {
            Title = title;
            Description = description;
            Fields = new List<SectionField>();
        }

        public void Add(SectionField field) 
        {
            Fields.Add(field);
        }

        public IEnumerator<SectionField> GetEnumerator()
        {
            return Fields.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return Fields.GetEnumerator();
        }
    }

    public class SectionField
    {
        public SectionField(string field=null, string with=null)
        {
            Name = field;
            With = with;
        }
        public string Name { get; set; }
        public string With { get; set; }
    }

}
