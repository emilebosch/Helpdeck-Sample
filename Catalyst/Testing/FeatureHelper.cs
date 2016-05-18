using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy.Testing;
using NUnit.Framework;
using System.Diagnostics;

namespace Catalyst
{
    public class ContentNotExpectedException : Exception
    {
        BrowserResponse res;

        public ContentNotExpectedException(BrowserResponse response, string msg)
            : base(msg)
        {
            res = response;
        }

        internal void Dump()
        {
            var fg = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("------------------------------------");
            Console.WriteLine(res.Body.AsString());
            Console.WriteLine("------------------------------------");
            Console.ForegroundColor = fg;
        }
    }

    public static class FeatureHelper
    {
        public static void HasText(this Feature f, string data, bool invert = false)
        {
            bool contains = f.Page.Body.AsString().Contains(data);
            if (invert)
            {
                if (contains)
                    throw new ContentNotExpectedException(f.Page, "Content does contain contain '" + data + "'");
            }
            else
            {
                if (!contains)
                    throw new ContentNotExpectedException(f.Page, "Content does not contain '" + data + "'");
            }
        }

        [DebuggerStepThrough]
        public static void Click(this Feature f, string name)
        {
            var e = f.Page.Body["a"].FirstOrDefault(a => a.InnerText == name);
            if (e == null) throw new ContentNotExpectedException(f.Page, "Can't find an A to click with name '" + name + "'!");
            f.Visit(e.Attribute["href"]);
        }

        public static void HasSelectedOption(this Feature f, string name)
        {
            var option = f.Page.Body["option:selected"].FirstOrDefault(a => a.Attribute["value"] == name);
            if (option == null) throw new ContentNotExpectedException(f.Page, "Can't find selected element with value: " + name);
        }

        public static void Fill(this Feature f, string labelname, string with = "")
        {
            var input = FindElementByLabel(f, labelname);
            f.Form.Add(input.Attribute["name"], with);
        }

        public static void SelectOption(this Feature f, string labelname, string with = "")
        {
            var input = FindElementByLabel(f, labelname);
            f.Form.Add(input.Attribute["name"], with);
        }

        private static NodeWrapper FindElementByLabel(Feature f, string name)
        {
            var label = f.Page.Body["label"].FirstOrDefault(a => a.InnerText == name);
            if (label == null) throw new ContentNotExpectedException(f.Page, "Can't find element with label: " + name);

            var id = label.Attribute["for"];
            var input = f.Page.Body["#" + id].FirstOrDefault();
            if (input == null) throw new ContentNotExpectedException(f.Page, "Can't find refering input/element with id '" + id + "' as refered by label '" + name + "'");

            return input;
        }

        public static void Select(this Feature f, string name)
        {
            var radio = FindElementByLabel(f, name);
            var id = radio.Attribute["id"];

            var attrName = radio.TryGetAttribute("name");
            var attrValue = radio.TryGetAttribute("value");

            if (attrName == null)
                throw new ContentNotExpectedException(f.Page, "The element with id '" + id + "' has no name attribute");

            if (attrValue == null)
                throw new ContentNotExpectedException(f.Page, "The element with id '" + id + "' has no value attribute");

            f.Form.Add(radio.Attribute["name"], radio.Attribute["value"]);
        }

        [DebuggerStepThrough]
        public static void Visit(this Feature f, string url)
        {
            f.Page = f.Browser.Get(url, with =>
            {
                with.HttpRequest();
            });

            if (f.Page.StatusCode != Nancy.HttpStatusCode.OK && f.Page.StatusCode != Nancy.HttpStatusCode.SeeOther)
                throw new Exception("Visiting '" + url + "' returned a '" + f.Page.StatusCode + "' statuscode!");

            f.Form.Clear();
            FollowRedirects(f);
        }

        [DebuggerStepThrough]
        private static void FollowRedirects(Feature f)
        {
            if (f.Page.Headers.ContainsKey("Location"))
            {
                f.Visit(f.Page.Headers["Location"]);
            }
        }

        [DebuggerStepThrough]
        public static void Post(this Feature f, string url, Action<BrowserContext> w)
        {
            f.Page = f.Browser.Post(url, w);
            if (f.Page.StatusCode != Nancy.HttpStatusCode.OK && f.Page.StatusCode != Nancy.HttpStatusCode.SeeOther)
                throw new Exception("Visiting '" + url + "' returned a '" + f.Page.StatusCode + "' statuscode!");
            f.Form.Clear();
            FollowRedirects(f);
        }

        public static void Submit(this Feature f, string name)
        {
            //Todo: Use label for lookup?
            var submitbutton = f.Page.Body["input[type='submit']"].FirstOrDefault(a => a.Attribute["value"] == name);
            if (submitbutton == null) throw new ContentNotExpectedException(f.Page, "Cannot find submit with name '" + name + "'");

            //Fix: why cant we walk up to our parent form?
            var form = f.Page.Body["form"].FirstOrDefault();
            string action = form.TryGetAttribute("action");
            if (action == null)
                throw new ContentNotExpectedException(f.Page, "Action attribute not found on form or misspelled");

            string method = form.TryGetAttribute("method");
            if (method == null)
                throw new ContentNotExpectedException(f.Page, "Method attribute not found on form or misspelled");

            var nm = submitbutton.TryGetAttribute("name");
            if (nm != null)
                f.Form.Add(nm, submitbutton.Attribute["value"]);

            //Add 
            f.Page.Body["input[type='text'][value]"].ToList().ForEach(e =>
            {
                nm = e.TryGetAttribute("name");
                if (nm != null && !f.Form.ContainsKey(nm))
                {
                    f.Form.Add(nm, e.Attribute["value"]);
                }
            });

            f.Post(action, a =>
            {
                a.HttpRequest();
                foreach (var formvalue in f.Form)
                {
                    a.FormValue(formvalue.Key, formvalue.Value);
                }
            });

            FollowRedirects(f);
        }

        public static string TryGetAttribute(this NodeWrapper w, string name)
        {
            try
            {
                return w.Attribute[name];
            }
            catch (NullReferenceException)
            {
            }
            return null;
        }
    }
}
