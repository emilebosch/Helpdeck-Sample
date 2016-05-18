using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Catalyst
{
    public class SimpleHash : Dictionary<string, object>, IEnumerable<SimpleHash>
    {
        private object Value { get; set; }

        public static SimpleHash Parse(Dictionary<string, string> parameters)
        {
            var dic = new SimpleHash();
            foreach (var value in parameters)
            {
                var current = dic;
                var key = value.Key;

                var keys = Regex.Matches(key, @"\[(?<key>.*?)\]").Cast<Match>().Select(a => a.Groups["key"].Value).ToList();
                var start = Regex.Match(key, @"(.*?)\[");

                if (start.Success)
                {
                    keys.Insert(0, start.Groups[1].Value);
                }

                for (int i = 0; i < keys.Count - 1; i++)
                {
                    var m = keys[i];
                    if (!current.ContainsKey(m))
                    {
                        current.Add(m, new SimpleHash());
                    }
                    current = (SimpleHash)current[m];
                }
                current.Add(keys.Count == 0 ? key : keys[keys.Count - 1], value.Value);

            }
            return dic;
        }

        public new SimpleHash this[string name]
        {
            get
            {
                if (!base.ContainsKey(name))
                {
                    return null;
                }
                var hash = base[name] as SimpleHash;
                if (hash != null)
                    return hash;
                return new SimpleHash { Value = base[name] };
            }
        }

        public SimpleHash this[int name]
        {
            get
            {
                return base[name.ToString()] as SimpleHash;
            }
        }

        public static implicit operator string(SimpleHash hash)
        {
            if (hash == null) return default(string);
            return hash.Value as string;
        }

        public static implicit operator bool(SimpleHash hash)
        {
            if (hash == null) return default(bool);
            return !String.IsNullOrEmpty(hash.Value as string);
        }

        public static implicit operator int(SimpleHash hash)
        {
            if (hash == null) return default(int);
            return int.Parse(hash.Value as string);
        }

        public new IEnumerator<SimpleHash> GetEnumerator()
        {
            foreach (var val in this.Values)
                yield return val as SimpleHash;
        }
    }

    public static class Extensions
    {
        public static void Update(this SimpleHash hash, object obj)
        {
            //FIXME 
            //1. Need to get the metadata definition from the object.
            //2. Populate Value From Hash. Ie.e Location f = CreateFieldValueFromHash(hash h)

            foreach (var prop in obj.GetType().GetProperties())
            {
                var name = prop.Name.ToLower();
                if (prop.PropertyType == typeof(string))
                {
                    prop.SetValue(obj, (string)hash[name], null);
                } else  
                if (prop.PropertyType == typeof(int))
                {
                    prop.SetValue(obj, (int)hash[name], null);
                }
                else
                if (prop.PropertyType == typeof(Location))
                {
                    prop.SetValue(obj, new Location { Lattitude = (string)hash[name]["x"], Longitude = (string)hash[name]["y"] }, null);
                }
                else
                if (prop.PropertyType == typeof(bool))
                {
                    prop.SetValue(obj, (bool)hash[name], null);
                } else
                if (prop.PropertyType == typeof(DateTime))
                {
                    prop.SetValue(obj, DateTime.Parse((string)hash[name]), null);
                } else
                if (prop.PropertyType.IsEnum)
                {
                    var en = Enum.Parse(prop.PropertyType, (string)hash[name], true);
                    prop.SetValue(obj, en, null);
                } else
                if (prop.PropertyType.IsArray && prop.PropertyType.GetElementType().IsEnum)
                {
                    var elementType = prop.PropertyType.GetElementType();
                    var items = (string)hash[name];
                    if (items != null)
                    {
                        var found = items.ToLower().Split(',').Select(elm => Enum.Parse(elementType, elm, true)).ToArray();
                        var array = Array.CreateInstance(elementType, found.Length);
                        for (var j = 0; j < found.Length; j++)
                        {
                            array.SetValue(found[j], j);
                        }
                        prop.SetValue(obj, array, null);
                    }
             
                }
            }
        }
    }
}
