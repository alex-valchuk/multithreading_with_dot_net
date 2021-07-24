using System;
using System.Threading;

namespace MutexExample
{
    static class Program
    {
        private static readonly Mutex mutex = new Mutex();
        private const int numOfIterations = 1;
        private const int numOfThreads = 3;
        
        static void Main()
        {
            // Create the threads that will use the protected resource
            for (var i = 0; i < numOfThreads; i++)
            {
                var newThread = new Thread(ThreadProc)
                {
                    Name = string.Format($"Thread{i + 1}")
                };
                newThread.Start();
            }
            
            // the main thread exits, but the application continues to run
            // until all foreground threads have exited.
        }

        private static void ThreadProc()
        {
            for (var i = 0; i < numOfIterations; i++)
            {
                UseResource();
            }
        }

        // this method represents a resource that must be synchronized
        // so that only one thread at a time can enter.
        private static void UseResource()
        {
            // wait until it is safe to enter.
            Console.WriteLine($"{Thread.CurrentThread.Name} is requesting the mutex");
            mutex.WaitOne();
            
            Console.WriteLine($"{Thread.CurrentThread.Name} has entered the protected area");
            
            // place code to access non-reentrant resources here.
            
            // simulate some work.
            Thread.Sleep(500);
            
            Console.WriteLine($"{Thread.CurrentThread.Name} is leaving the protected area");
            
            // release the mutex
            mutex.ReleaseMutex();
            Console.WriteLine($"{Thread.CurrentThread.Name} has released the mutex");
        }
    }
}
