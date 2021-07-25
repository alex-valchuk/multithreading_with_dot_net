using System;
using System.Threading;

namespace WaitHandleExample
{
    static class Program
    {
        private static readonly WaitHandle[] _waitHandles = {
            new AutoResetEvent(false),
            new AutoResetEvent(false)
        };

        private static readonly Random random = new Random();
        
        static void Main()
        {
            // Queue up two tasks on two different threads;
            // wait until all tasks are completed.
            
            DateTime dateTime = DateTime.Now;
            Console.WriteLine("Main thread is waiting for BOTH tasks to complete.");

            ThreadPool.QueueUserWorkItem(DoTask, _waitHandles[0]);
            ThreadPool.QueueUserWorkItem(DoTask, _waitHandles[1]);
            WaitHandle.WaitAll(_waitHandles);
            
            Console.WriteLine($"Both tasks are completed (time waited={(DateTime.Now - dateTime).TotalMilliseconds})");
            Console.WriteLine();

            // Queue up two tasks on two different threads;
            // wait until any tasks are completed.
            dateTime = DateTime.Now;
            Console.WriteLine("The main thread is waiting for either task to complete.");
            
            ThreadPool.QueueUserWorkItem(DoTask, _waitHandles[0]);
            ThreadPool.QueueUserWorkItem(DoTask, _waitHandles[1]);
            int index = WaitHandle.WaitAny(_waitHandles);
            
            // The time shown below should match the shortest task.
            Console.WriteLine($"Task {index + 1} finished first (time waited={(DateTime.Now - dateTime).TotalMilliseconds}).");
        }

        private static void DoTask(object state)
        {
            var autoResetEvent = (AutoResetEvent) state;

            int time = 1000 * random.Next(2, 10);
            Console.WriteLine($"Performing a task for {time} milliseconds.");
            
            Thread.Sleep(time);
            autoResetEvent.Set();
        }
    }
}