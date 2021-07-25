using System;
using System.Threading;

namespace SemaphoreExample
{
    class Program
    {
        private static Semaphore _pool;
        
        // a padding interval to make the output more orderly
        private static int _padding;
        
        static void Main(string[] args)
        {
            // create a semaphore that can satisfy up to three concurrent requests.
            // Use an initial count of zero, so that the entire semaphore count is initially
            // owned by the main program thread.
            _pool = new Semaphore(0, 3);
            
            // create and start 5 numbered threads.
            for (int i = 0; i < 5; i++)
            {
                var thread = new Thread(Worker);
                thread.Start(i);
            }
            
            // wait for half a second, to allow all the threads to start and to block on the semaphore.
            Thread.Sleep(500);
            
            // The main thread starts out holding the entire semaphore count.
            // Calling Release(3) brings the semaphore count back to its maximum value,
            // and allows the waiting threads to enter the semaphore, up to three at a time.
            Console.WriteLine("Main thread calls Release(3).");
            _pool.Release(3);
            
            Console.WriteLine("Main thread exits.");
        }

        private static void Worker(object num)
        {
            // Each working thread begins by requesting the semaphore.
            Console.WriteLine($"Thread {num} begins and waits for the semaphore.");
            _pool.WaitOne();
            
            // A padding interval to make the output more orderly.
            int padding = Interlocked.Add(ref _padding, 100);
            
            Console.WriteLine($"Thread {num} enters the semaphore.");
            
            // The thread's work consists of sleeping for about a second.
            // Each thread works a little longer, just to make the output more orderly.
            Thread.Sleep(1000 + padding);
            
            Console.WriteLine($"Thread {num} releases the semaphore.");
            Console.WriteLine($"Thread {num} previous semaphore count: {_pool.Release()}");
        }
    }
}