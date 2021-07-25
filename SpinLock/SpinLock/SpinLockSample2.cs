using System;
using System.Threading;
using System.Threading.Tasks;

namespace SpinLockExample
{
    public static class SpinLockSample2
    {
        public static void Execute()
        {
            // Instantiate a SpinLock
            SpinLock sl = new SpinLock();

            // These MRESs help to sequence the two jobs below
            ManualResetEventSlim mre1 = new ManualResetEventSlim(false);
            ManualResetEventSlim mre2 = new ManualResetEventSlim(false);
            bool lockTaken = false;

            Task taskA = Task.Factory.StartNew(() =>
            {
                try
                {
                    sl.Enter(ref lockTaken);
                    Console.WriteLine("Task A: entered SpinLock");
                    mre1.Set(); // Signal Task B to commence with its logic

                    // Wait for Task B to complete its logic
                    // (Normally, you would not want to perform such a potentially
                    // heavyweight operation while holding a SpinLock, but we do it
                    // here to more effectively show off SpinLock properties in
                    // taskB.)
                    mre2.Wait();
                }
                finally
                {
                    if (lockTaken) sl.Exit();
                }
            });

            Task taskB = Task.Factory.StartNew(() =>
            {
                mre1.Wait(); // wait for Task A to signal me
                Console.WriteLine("Task B: sl.IsHeld = {0} (should be true)", sl.IsHeld);
                Console.WriteLine("Task B: sl.IsHeldByCurrentThread = {0} (should be false)", sl.IsHeldByCurrentThread);
                Console.WriteLine("Task B: sl.IsThreadOwnerTrackingEnabled = {0} (should be true)",
                    sl.IsThreadOwnerTrackingEnabled);

                try
                {
                    sl.Exit();
                    Console.WriteLine("Task B: Released sl, should not have been able to!");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Task B: sl.Exit resulted in exception, as expected: {0}", e.Message);
                }

                mre2.Set(); // Signal Task A to exit the SpinLock
            });

            // Wait for task completion and clean up
            Task.WaitAll(taskA, taskB);
            mre1.Dispose();
            mre2.Dispose();
        }
    }
}