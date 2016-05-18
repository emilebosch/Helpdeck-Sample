using System;
using System.Collections.Generic;
using System.Threading;

namespace Catalyst
{
    public interface IJob
    {
        void Execute();
    }

    public class Scheduler
    {
        List<Tuple<IJob, TimeSpan>> jobs = new List<Tuple<IJob, TimeSpan>>();
        bool running;

        public void Schedule<T>(TimeSpan span) where T : IJob, new()
        {
            jobs.Add(new Tuple<IJob, TimeSpan>(new T(), span));
        }

        public void Start()
        {
            running = true;
            jobs.ForEach(j =>
            {
                new Thread(() =>
                {
                    while (running)
                    {
                        Console.WriteLine("Job: {0}".Inject(j.Item1.ToString()));
                        j.Item1.Execute();
                        Thread.Sleep(j.Item2);
                    }
                }).Start();
            });
        }

        public void Stop()
        {
            running = false;
        }
    }
}
