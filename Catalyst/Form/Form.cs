using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Catalyst
{
    public class Form
    {
        public string Prefix { get; set; }
        public List<Field> Fields { get; set; }
        public Dictionary<string, object> Values { get; set; }
        public Dictionary<string, List<string>> Errors { get; set; }

        public Form(object o, string prefix = null, int index = -1)
        {
            Prefix = prefix;

            if (index != -1)
            {
                Prefix += "[" + index + "]";
            }

            Fields = new List<Field>();
            Values = new Dictionary<string, object>();
            Errors = new Dictionary<string, List<string>>();

            Define(o.GetType());
            Populate(o);
        }

        public Form ChildForm(object o, string prefix = null, int index = -1)
        {
            return new Form(o, this.Prefix + "[" + prefix + "]", index);
        }

        public void Define(Type type, Func<PropertyInfo, Field> missing = null)
        {
            Fields = AttributeDescriber.GetFieldsFor(type, missing);
        }

        public void Populate(object target)
        {
            Values = AttributeValueExtractor.GetValuesFor(target);
            var validatable = target as IValidatable;
            if (validatable != null)
            {
                Errors = validatable.Errors;
                foreach (var error in Errors)
                {
                    var field = Field(error.Key);
                    field.Errors = error.Value;
                }
            }
        }

        public Field Field(string name)
        {
            return this.Fields.FirstOrDefault(field => field.Name == name);
        }

        public string Label(string name)
        {
            return FieldRenderer.RenderLabel(Field(name), Prefix);
        }

        public string Hint(string name, string with = null)
        {
            return FieldRenderer.RenderHint(Field(name));
        }

        public string Error(string name)
        {
            return FieldRenderer.RenderError(Field(name));
        }

        public string Input(string name, string with = null)
        {
            object val = null;
            Values.TryGetValue(name, out val);
            return FieldRenderer.RenderInput(Field(name), val, Prefix, with);
        }

        public string Render()
        {
            return FormRenderer.Render(this);
        }
    }

    // Attribute describer

    public static class AttributeDescriber
    {
        public static List<Field> GetFieldsFor(Type type, Func<PropertyInfo, Field> missing = null)
        {
            var fields = new List<Field>();
            foreach (var prop in type.GetProperties())
            {
                var field = GetAttributeFieldFor(prop, missing);
                if (field != null)
                    fields.Add(field);
            }
            return fields;
        }

        public static Field GetAttributeFieldFor(PropertyInfo property, Func<PropertyInfo, Field> missing)
        {
            var name = property.Name.ToLower();
            if (property.PropertyType == typeof(DateTime))
            {
                return new DateField { Name = name, Label = property.Name };
            } else if (property.PropertyType == typeof(string))
            {
                return new TextField { Name = name, Label = property.Name };
            }
            else if (property.PropertyType == typeof(bool))
            {
                return new BooleanField { Name = name, Label = property.Name };
            }
            else if (property.PropertyType == typeof(Location))
            {
                return new LocationField { Name = name, Label = property.Name };
            }
            else if (property.PropertyType == typeof(int))
            {
                return new NumericField { Name = name, Label = property.Name };
            }
            else if (property.PropertyType.IsEnum || (property.PropertyType.IsArray && property.PropertyType.GetElementType().IsEnum))
            {
                var e = !property.PropertyType.IsArray ? property.PropertyType : property.PropertyType.GetElementType();
                var names = Enum.GetNames(e);
                var values = Enum.GetValues(e);

                int i = 0;
                var options = new Collection();

                foreach (var o in values)
                    options.Add(((int)o).ToString(), names[i++].ToString());

                return new CollectionField { Name = name, Collection = options, Label = property.Name, Multiple = property.PropertyType.IsArray };
            }
            else if (missing != null)
            {
                return missing(property);
            }
            return null;
        }
    }

    //Relation describer

    public static class RelationDescriber
    {
        public static Relation[] GetRelationsFor(object target)
        {
            var list = new List<Relation>();
            foreach (var relation in target.GetType().GetProperties())
            {
                if (relation.PropertyType.IsGenericType && 
                    relation.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>))
                {   
                    list.Add(new Relation
                    {
                        Name = relation.Name,
                        Type = relation.PropertyType.GetGenericArguments()[0]
                    });
                }
            }
            return list.ToArray();
        }
    }

    public class Relation
    {
        public string Name { get; set; }
        public Type Type { get; set; }
    }
    
    //Value extractor 
    public static class AttributeValueExtractor
    {
        public static Dictionary<string, object> GetValuesFor(object target)
        {
            var values = new Dictionary<string, object>();
            foreach (var property in target.GetType().GetProperties())
            {
                values.Add(property.Name.ToLower(), property.GetValue(target, null));
            }
            return values;
        }
    }
}
