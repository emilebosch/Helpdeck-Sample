using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Diagnostics;

namespace Catalyst.Guard
{
    class Program
    {
        public static bool changed = true;
        public static bool isRunning = false;
        public static DateTime lastUpdated = DateTime.Now;

        static void Main(string[] args)
        {
            var path = @"E:\Code\IssueApp\IssueApp\bin\Release\TicketApp.exe";
            var pargs = new string[] { "/spec", "/b" };

            var dir = Path.GetDirectoryName(path);
            var filename = Path.GetFileName(path);

            Console.WriteLine(filename);

            var watcher = new FileSystemWatcher(dir);
            watcher.Changed += new FileSystemEventHandler(watcher_Changed);
            watcher.EnableRaisingEvents = true;

            new Thread(new ThreadStart(() =>
            {
                while (true)
                {
                    Thread.Sleep(500);
                    if (changed && !isRunning && DateTime.Now > lastUpdated.AddSeconds(1))
                    {
                        isRunning = true;
                        Reload(path, pargs);
                        isRunning = changed = false;
                    }
                }

            })).Start();
            Console.ReadKey();
        }
        private static void Reload(string path, string[] args)
        {
            Console.WriteLine("Reloading!");

            var setup = new AppDomainSetup();
            setup.ConfigurationFile = path + ".config";
            setup.ApplicationBase = Path.GetDirectoryName(path);

            Directory.SetCurrentDirectory(Path.GetDirectoryName(path));

            var domain = AppDomain.CreateDomain("TEMP", null, setup);
            domain.ExecuteAssembly(path, args);
            AppDomain.Unload(domain);
        }

        static void watcher_Changed(object sender, FileSystemEventArgs e)
        {
            lastUpdated = DateTime.Now;
            changed = true;
        }
    }
}
