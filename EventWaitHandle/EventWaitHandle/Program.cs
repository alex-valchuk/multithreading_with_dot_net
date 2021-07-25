using System;
using System.Threading;

namespace EventWaitHandleExample
{
    static class Program
    {
        // An AutoReset event that allows the main thread to block
        // until an exiting thread has decremented the count.
        private static readonly EventWaitHandle _clearCount = new EventWaitHandle(false, EventResetMode.AutoReset);

        // The EventWaitHandle used to demonstrate the difference
        // between AutoReset and ManualReset synchronization events.
        private static EventWaitHandle _eventWaitHandle;

        // A counter to make sure all threads are started and
        // blocked before any are released. A Long is used to show
        // the use of the 64-bit Interlocked methods.
        private static long _threadCount;
        
        [MTAThread]
        static void Main()
        {
            _eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);

            // Create and start five numbered threads. Use the ParameterizedThreadStart delegate,
            // so the thread number can be passed as an argument to the Start method.
            for (int i = 0; i < 5; i++)
            {
                var thread = new Thread(ThreadProc);
                thread.Start(i);
            }

            // Wait until all the threads have started and blocked.
            // When multiple threads use a 64-bit value on a 32-bit
            // system, you must access the value through the
            // Interlocked class to guarantee thread safety.
            while (Interlocked.Read(ref _threadCount) < 5)
            {
                Thread.Sleep(500);
            }

            // Release one thread each time the user presses ENTER,
            // until all threads have been released.
            while (Interlocked.Read(ref _threadCount) > 0)
            {
                Console.WriteLine("Press any key to release a waiting thread.");
                Console.ReadLine();
                
                // SignalAndWait signals the EventWaitHandle, which
                // releases exactly one thread before resetting, 
                // because it was created with AutoReset mode. 
                // SignalAndWait then blocks on clearCount, to 
                // allow the signaled thread to decrement the count
                // before looping again.
                WaitHandle.SignalAndWait(_eventWaitHandle, _clearCount);
            }
            Console.WriteLine();

            _eventWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
            
            // Create and start five more numbered threads.
            for (int i = 0; i < 5; i++)
            {
                var thread = new Thread(ThreadProc);
                thread.Start(i);
            }
            
            // Wait until all the threads have started and blocked.
            while (Interlocked.Read(ref _threadCount) < 5)
            {
                Thread.Sleep(500);
            }

            // Because the EventWaitHandle was created with ManualReset mode,
            // signaling it releases all the waiting threads.
            Console.WriteLine("Press any key to release the waiting threads.");
            Console.ReadLine();

            _eventWaitHandle.Set();
        }

        private static void ThreadProc(object data)
        {
            int index = (int) data;
            
            Console.WriteLine($"Thread {index} blocks.");
            Interlocked.Increment(ref _threadCount);

            _eventWaitHandle.WaitOne();
            
            Console.WriteLine($"Thread {index} exits.");
            Interlocked.Decrement(ref _threadCount);
            
            // After signaling _eventWaitHandle, the main thread blocks on _clearCount
            // until the signaled thread has decremented the count. Signal it now.
            _clearCount.Set();
        }
    }
}