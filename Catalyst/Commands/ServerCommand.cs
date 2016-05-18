using System;
using System.Collections.Generic;
using System.Linq;
using Nancy.Hosting.Self;
using Nancy;
using System.Diagnostics;
using System.IO;

namespace Catalyst
{
    public class AppCommands
    {
        public void RunServer(ServerCommand command)
        {
            LoadEnvionment(command.Environment);
            Console.WriteLine("Starting.."+command.Host);
            new NancyHost(new Uri(command.Host)).Tap(n =>
            {
                n.Start();
                Console.ReadKey();
                n.Stop();
            });
        }

        public void RunSpec(SpecCommand command)
        {
            LoadEnvionment(command.Environment);

            var bootstrapper = new DefaultNancyBootstrapper();
            var foundFeatures = DiscoverTypes(typeof(Feature)).Where(a => !a.IsAbstract);

            if (!command.Feature.IsEmpty())
            {
                foundFeatures = foundFeatures.Where(a => a.Name == command.Feature);
            }

            if (command.Broken)
            {
                Console.WriteLine("Running broken only");
                if (File.Exists(BROKEN_FILE))
                {
                    var lines = File.ReadAllLines(BROKEN_FILE);
                    if(lines.Count()!=0)
                        foundFeatures = foundFeatures.Where(f => lines.Contains(f.Name));
                }
            }

            if (command.Debug)
            {
                Debugger.Launch();
            }

            var results = new Dictionary<Feature, RunResult>();

            foreach (var featureType in foundFeatures)
            {
                var feature = Activator.CreateInstance(featureType) as Feature;
                feature.Initialize(bootstrapper);

                var result = feature.Run();
                results.Add(feature, result);

                if (result.Exception != null && command.Debug)
                {
                    feature.Before(() =>
                    {
                        Debugger.Break();
                    });
                    feature.Run();
                }
            }

            if (command.Broken)
            {
                var brokenFeatures = results.Where(a => a.Value.Exception != null).Select(x => x.Key.GetType().Name).ToArray();
                File.WriteAllLines(BROKEN_FILE, brokenFeatures);
            }
        }

        public class SpecCommand
        {
            public SpecCommand()
            {
                Environment = "Test";
            }

            public string Feature { get; set; }
            public string Scenario { get; set; }
            public bool Debug { get; set; }
            public string Environment { get; set; }
            public bool Broken { get; set; }
        }

        string BROKEN_FILE = "FEATURES.BROKEN";
        private static IEnumerable<Type> DiscoverTypes(Type type)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var foundTypes = assembly.GetTypes().Where(t => type.IsAssignableFrom(t));
                foreach (var t in foundTypes)
                    yield return t;
            }
        }

        public class ServerCommand
        {
            public ServerCommand()
            {
                Environment = "Production";
                Host = "http://127.0.0.1:1080";
            }
            public string Environment { get; set; }
            public string Host { get; set; }
        }

        private static void LoadEnvionment(string name)
        {
            Console.WriteLine("Loading " + name);
            var env = DiscoverTypes(typeof(AppEnvironment)).First(a => a.Name == name);
            var p = Activator.CreateInstance(env) as AppEnvironment;
            p.Setup();
        }
    }
}
