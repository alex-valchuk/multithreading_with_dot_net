using System;
using System.Threading;

namespace MonitorExample
{
    public class Singleton
    {
        private static readonly Object s_lock = new object();

        private static Singleton singleton;
        private readonly int _id;

        private Singleton()
        {
            _id = new Random().Next(0, 100);
        }

        public static Singleton GetSingletonUsingMonitorExplicitly()
        {
            if (singleton != null)
            {
                return singleton;
            }

            Monitor.Enter(s_lock);

            try
            {
                singleton ??= new Singleton();
            }
            finally
            {
                Monitor.Exit(s_lock);
            }
            
            return singleton;
        }

        public static Singleton GetSingletonUsingMonitorImplicitly()
        {
            if (singleton != null)
            {
                return singleton;
            }

            lock (s_lock)
            {
                singleton ??= new Singleton();
            }
            
            return singleton;
        }

        public void ShowId(string threadName)
        {
            Console.WriteLine($"The Id of a singleton for thread {threadName} is {_id}.");
        }
    }
}