using System;
using System.Collections.Generic;
using System.Linq;

namespace Catalyst
{
    public static class App
    {
        static List<object> components = new List<object>();

        public static T Get<T>(bool allowNull = false) where T : class
        {
            var found = components.FirstOrDefault(a => a is T) as T;
            if (!allowNull && found == default(T))
                throw new Exception("Tried to get component " + typeof(T).Name + " but it was not in the ioc container");
            return found;
        }

        public static void Add<T>(T component)
        {
            components.Add(component);
        }

        public static I Add<I, C>() where C : I, new()
        {
            var concrete = new C();
            components.Add(concrete);
            return concrete;
        }

        public static void Start(string[] args)
        {
            if (args.Length > 0)
            {
                var commands = new AppCommands();
                if (args[0] == "/server")
                {
                    commands.RunServer(Args.Configuration.Configure<AppCommands.ServerCommand>().CreateAndBind(args));
                } else if (args[0] == "/spec")
                {              
                    commands.RunSpec(Args.Configuration.Configure<AppCommands.SpecCommand>().CreateAndBind(args));
                }
            }
        }

        private static IEnumerable<Type> DiscoverTypes(Type type)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var types = assembly.GetTypes().Where(t => type.IsAssignableFrom(t));
                foreach (var t in types)
                    yield return t;
            }
        }
    }
}
