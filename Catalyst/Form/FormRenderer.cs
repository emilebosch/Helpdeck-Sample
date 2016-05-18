using System;
using System.Text;

namespace Catalyst
{
    public class FormRenderer
    {
        public static string Render(SectionForm form, Form model)
        {
            var sb = new StringBuilder();
            foreach (var section in form.Sections)
            {
                sb.AppendLine("<div class='section'>");
                sb.AppendLine("<h2>{0}</h2>".Inject(section.Title));
                sb.AppendLine("<div class='notes'>{0}</div>".Inject(section.Description));
                foreach (var field in section.Fields)
                {
                    sb.AppendLine(model.Label(field.Name));
                    if (String.IsNullOrEmpty(field.With))
                    {
                        sb.AppendLine(model.Input(field.Name));
                    }
                    else
                    {
                        sb.AppendLine(model.Input(field.Name, with: field.With));
                    }
                    sb.AppendLine(model.Hint(field.Name));
                    sb.AppendLine(model.Error(field.Name));
                }
                sb.AppendLine("</div>");
            }

            return sb.ToString();
        }

        public static string Render(Form model)
        {
            var sb = new StringBuilder();
            foreach (var field in model.Fields)
            {
                sb.AppendLine(model.Label(field.Name));
                sb.AppendLine(model.Input(field.Name));
                sb.AppendLine(model.Error(field.Name));
                sb.AppendLine(model.Hint(field.Name));
            }
            return sb.ToString();
        }
    }
}
