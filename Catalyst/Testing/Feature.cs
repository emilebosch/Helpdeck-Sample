using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy.Testing;
using Nancy;
using Nancy.Bootstrapper;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.ComponentModel;

namespace Catalyst
{
    public class RunResult
    {
        public Exception Exception { get; set; }
    }

    public abstract class Feature
    {
        String[] story = new string[] { };
        Dictionary<string, Action> scenarios = new Dictionary<string, Action>();
        Action background;
        Action _beforeAction = null;

        public Browser Browser;
        public BrowserResponse Page;
        public Dictionary<string, string> Form { get; set; }

        public void Initialize(INancyBootstrapper b)
        {
            Browser = new Browser(b);
            Form = new Dictionary<string, string>();
        }

        public void Story(params string[] lines)
        {
            story = lines;
        }

        public void Scenario(string name, Action t)
        {
            scenarios.Add(name, t);
        }

        public void Given(string str, Action t)
        {
            ExecuteStep(str, t, "Given");
        }

        private static void ExecuteStep(string str, Action step, string name)
        {
            Console.Write(" " + name + ": " + str);
            step();
            Console.WriteLine();
        }

        public void When(string str, Action step)
        {
            ExecuteStep(str, step, "When");
        }

        public void Then(string str, Action step)
        {
            ExecuteStep(str, step, "Then");
        }

        public void And(string str, Action step)
        {
            ExecuteStep(str, step, "When");
        }

        public void Given(string str)
        {
            foreach (var stepdef in defs)
            {
                var match = Regex.Match(str, stepdef.Key);
                if (match.Success)
                {
                    var cnt = match.Groups.Count - 1;
                    var t = stepdef.Value.GetType();
                    var args = t.GetGenericArguments();

                    var parameters = new List<object>();
                    foreach (var arg in args)
                    {
                        parameters.Add(new TypeConverter().ConvertTo(match.Groups[parameters.Count + 1].Value, arg));
                    }
                    Console.WriteLine("Invoke"+parameters[0]);
                    ((Delegate)stepdef.Value).DynamicInvoke(parameters.ToArray());              
                }
            }
        }

        Dictionary<string, object> defs = new Dictionary<string, object>();
        public void Given<T1>(string str, Action<T1> t)
        {
            defs.Add(str, t);   
        }

        public void Given<T1,T2>(string str, Action<T1,T2> t)
        {
            defs.Add(str, t);
        }

        public void Background(Action action)
        {
            background = action;
        }

        public void Before(Action beforeAction)
        {
            _beforeAction = beforeAction;
        }

        public RunResult Run()
        {
            var result = new RunResult();

            var fg = Console.ForegroundColor;

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(GetType().Name);
            Console.ForegroundColor = fg;

            foreach (var line in story)
            {
                Console.WriteLine(line);
            }
            Console.WriteLine("-----------");

            if (background != null)
            {
                Console.WriteLine("Running background..");
                background();
            }

            var failed = 0;
            foreach (var scenario in scenarios)
            {
                Console.WriteLine("Scenario: " + scenario.Key);
                try
                {
                    if (_beforeAction != null)
                        _beforeAction();

                    scenario.Value();

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(" [OK]");
                    Console.ForegroundColor = fg;
                }
                catch (Exception ex)
                {
                    result.Exception = ex;
                    failed++;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(" [FAIL]");

                    var currentEx = ex;
                    while (currentEx != null)
                    {
                        Console.WriteLine(currentEx.Message);
                        currentEx = currentEx.InnerException;
                    }

                    Console.ForegroundColor = fg;

                    var contentNotExpected = ex as ContentNotExpectedException;
                    if (contentNotExpected != null)
                    {
                        contentNotExpected.Dump();
                    }
                }
            }
            Console.WriteLine();
            Console.ForegroundColor = failed == 0 ? ConsoleColor.Green : ConsoleColor.Red;
            Console.WriteLine("{0} scenarios, {1} failed", scenarios.Count(), failed);
            Console.ForegroundColor = fg;
            return result;
        }
    }
}
