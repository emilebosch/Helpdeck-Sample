using System;
using System.Collections.Generic;
using Nancy;

namespace Catalyst
{
    public static class Helpers
    {
        public static bool IsEmpty(this string str)
        {
            return String.IsNullOrEmpty(str);
        }

        public static string ToHuman(this DateTime time)
        {
            return time.ToString();
        }

        public static T Tap<T>(this T c, Action<T> t)
        {
            t(c);
            return c;
        }

        public static string Inject(this string f, params object[] p)
        {
            return String.Format(f, p);
        }

        public static T Add<T>(this ICollection<T> collection, T item)
        {
            collection.Add(item);
            return item;
        }

        public static Dictionary<string, string> ToDictionary(this Request r)
        {
            var d = new Dictionary<string, string>();
            var dict = (Nancy.DynamicDictionary)r.Form;

            foreach (var a in dict.GetDynamicMemberNames())
            {
                d.Add(a, (string)dict[a]);
            }
            return d;
        }

        public static SimpleHash Parameters(this Request r)
        {
            return SimpleHash.Parse(r.ToDictionary());
        }
    }
}