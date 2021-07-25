using System;
using System.Threading;

namespace ManualResetEventExample
{
    static class Program
    {
        // mre is used to block and release threads manually. It is
        // created in the unsignaled state.
        private static readonly ManualResetEvent mre = new ManualResetEvent(false);

        static void Main()
        {
            Console.WriteLine("Start 3 named threads that block on a ManualResetEvent:");

            for (int i = 0; i <= 2; i++)
            {
                Thread t = new Thread(ThreadProc)
                {
                    Name = "Thread_" + i
                };
                t.Start();
            }

            Thread.Sleep(500);
            Console.WriteLine("When all three threads have started, press Enter to call Set() to release all the threads.");
            Console.ReadLine();

            mre.Set();

            Thread.Sleep(500);
            Console.WriteLine("When a ManualResetEvent is signaled, threads that call WaitOne() do not block. " +
                              "Press Enter to show this.");
            Console.ReadLine();

            for (int i = 3; i <= 4; i++)
            {
                Thread t = new Thread(ThreadProc)
                {
                    Name = "Thread_" + i
                };
                t.Start();
            }

            Thread.Sleep(500);
            Console.WriteLine("Press Enter to call Reset(), so that threads once again block when they call WaitOne().");
            Console.ReadLine();

            mre.Reset();

            // Start a thread that waits on the ManualResetEvent.
            Thread t5 = new Thread(ThreadProc)
            {
                Name = "Thread_5"
            };
            t5.Start();

            Thread.Sleep(500);
            Console.WriteLine("\nPress Enter to call Set() and conclude the demo.");
            Console.ReadLine();

            mre.Set();
        }

        private static void ThreadProc()
        {
            string name = Thread.CurrentThread.Name;

            Console.WriteLine(name + " starts and calls mre.WaitOne()");

            mre.WaitOne();

            Console.WriteLine(name + " ends.");
        }
    }
}