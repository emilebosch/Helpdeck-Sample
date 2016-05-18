using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//1. Define field value - DONE
//2. Set default renderer for fiedl value - DONE
//3. Make Hash value to field extractor.

namespace Catalyst
{
    public static class FieldRenderer
    {
        static Dictionary<string, Func<Field, object, string, string>> renders = new Dictionary<string, Func<Field, object, string, string>>();
        
        static FieldRenderer()
        {
            renders.Add("text", (field, value, name) =>
            {
                return "<input class='{2}' name='{0}' value='{1}'/>".Inject(name, value, field.Errors.Any()?"error":"");
            });

            renders.Add("date", (field, value, name) =>
            {
                return "<input class='{2}' name='{0}' value='{1}'/> <button>Select</button>".Inject(name, value, field.Errors.Any() ? "error" : "");
            });

            renders.Add("numeric", (field, value, name) =>
            {
                return "<input class='{2}' name='{0}' value='{1}'/>".Inject(name, value, field.Errors.Any() ? "error" : "");
            });

            renders.Add("check", (field, value, name) =>
            {
                return "<input class='{2}' type='checkbox' name='{0}' {1}/>".Inject(name, value != null && ((bool)value) ? "checked='checked'" : "", field.Errors.Any() ? "error" : "");
            });

            renders.Add("memo", (field, value, name) =>
            {
                return "<textarea class='{2}' name='{0}'>{1}</textarea>".Inject(name, value, field.Errors.Any() ? "error" : "");
            });

            renders.Add("select", (field, value, name) =>
            {
                var selected = ConvertAndGetValues(ref field, value, name);
                var choice = (CollectionField)field;
                var html = "<select class='{2}' name='{0}' {1}>".Inject(name, choice.Multiple ? "multiple='multiple'" : "", field.Errors.Any() ? "error" : "");
                foreach (var option in choice.Collection)
                    html += ("<option {2} value='{0}'>{1}</option>".Inject(option.Key, option.Value, value != null && selected.Contains(option.Key) ? "selected" : ""));
                return html + "</select>";
            });

            renders.Add("location", (field, value, name) => 
            {
                var loc = value as Location;
                return "Lat: <input type='text' value='{1}' class='{3}' name='{0}[x]'/> Lon: <input class='{3}' type='text' value='{2}' name='{0}[y]'/>".Inject(name, loc.Lattitude, loc.Longitude, field.Errors.Any() ? "error" : "");

            });

            renders.Add("radio", (field, value, name) =>
            {
                var selected = ConvertAndGetValues(ref field, value, name);
                var choice = (CollectionField)field;
                string html = null;
                foreach (var option in choice.Collection)
                    html += ("<input class='{4}' type='radio' name='{0}' value='{1}' {3}>{2}".Inject(name, option.Key, 
                        option.Value, value != null && selected.Contains(option.Key) ? "checked" : "", field.Errors.Any() ? "error" : ""));
                return html;
            });
        }

        private static List<string> ConvertAndGetValues(ref Field field, object value, string name)
        {
            var selected = new List<string>();
            if (field is BooleanField)
            {
                field = new CollectionField { Name = name, Collection = { { "1", "Yes" }, { "0", "No" } } };
                if (value != null && (bool)value)
                    selected.Add("1");
                else
                    selected.Add("0");
            }
            else
            {
                if (value != null)
                {
                    if (value is Array)
                    {
                        foreach (var en in (Array)value)
                            selected.Add(((int)en).ToString());
                    }
                    else
                    {
                        selected.Add(((int)value).ToString());
                    }
                }
            }
            return selected;
        }

        private static string TryFindDefault(Field field)
        {
            var name = field.Name.ToLower();
            if (field is BooleanField)
                return "check";

            if (field is CollectionField)
                return ((new[] { "sex", "gender" }).Contains(name) ? "radio" : "select"); ;

            if (field is TextField)
                return ((new[] { "description", "comments", "comment", "text" }).Contains(name) ? "memo" : "text");

            if (field is DateField)
                return "date";

            if (field is NumericField)
                return "numeric";

            if (field is LocationField)
            {
                return "location";
            }
            return null;
        }

        public static string RenderInput(Field field, object value, string prefix, string with = null)
        {
            var name = GetFieldName(field, prefix);
            Func<Field, object, string, string> render = null;

            if (with != null)
            {
                return renders[with](field, value, name);
            }
            else
            {
                render = renders[TryFindDefault(field)];
            }

            if (render != null)
            {
                return render(field, value, name);
            };
            return string.Empty;
        }

        private static string GetFieldName(Field field, string prefix)
        {
            if (prefix != null)
                return "{0}[{1}]".Inject(prefix, field.Name).ToLower();
            return "{0}".Inject(field.Name).ToLower();
        }

        public static string RenderLabel(Field field, string prefix)
        {
            return "<div><b><label for='{0}'>{2}{1}</label></b></div>".Inject(GetFieldName(field, prefix),
                !String.IsNullOrEmpty(field.Label) ? field.Label : field.Name, field.Required ? "*" : string.Empty,
                field.Errors.Any() ? "error" : "");
        }

        public static string RenderHint(Field input)
        {
            return "<i>{0}</i>".Inject(input.Hint);
        }

        public static string RenderError(Field input) 
        {
            if (!input.Errors.Any())
                return string.Empty;
            
            var builder = new StringBuilder();
            builder.Append("<ul>");
            foreach (var a in input.Errors)
            {
                builder.AppendFormat("<li>{0}</li>", a);
            }
            builder.Append("</ul>");
            return builder.ToString();
        }
    }
}
