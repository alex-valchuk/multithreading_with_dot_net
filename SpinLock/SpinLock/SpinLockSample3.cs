using System;
using System.Threading;
using System.Threading.Tasks;

namespace SpinLockExample
{
    public static class SpinLockSample3
    {
        public static void Execute()
        {
            // Create SpinLock that does not track ownership/threadIDs
            SpinLock sl = new SpinLock(false);

            // Used to synchronize with the Task below
            ManualResetEventSlim mres = new ManualResetEventSlim(false);

            // We will verify that the Task below runs on a separate thread
            Console.WriteLine("main thread id = {0}", Thread.CurrentThread.ManagedThreadId);

            // Now enter the SpinLock.  Ordinarily, you would not want to spend so
            // much time holding a SpinLock, but we do it here for the purpose of 
            // demonstrating that a non-ownership-tracking SpinLock can be exited 
            // by a different thread than that which was used to enter it.
            bool lockTaken = false;
            sl.Enter(ref lockTaken);

            // Create a separate Task from which to Exit() the SpinLock
            Task worker = Task.Factory.StartNew(() =>
            {
                Console.WriteLine("worker task thread id = {0} (should be different than main thread id)",
                    Thread.CurrentThread.ManagedThreadId);

                // Now exit the SpinLock
                try
                {
                    sl.Exit();
                    Console.WriteLine("worker task: successfully exited SpinLock, as expected");
                }
                catch (Exception e)
                {
                    Console.WriteLine("worker task: unexpected failure in exiting SpinLock: {0}", e.Message);
                }

                // Notify main thread to continue
                mres.Set();
            });

            // Do this instead of worker.Wait(), because worker.Wait() could inline the worker Task,
            // causing it to be run on the same thread.  The purpose of this example is to show that
            // a different thread can exit the SpinLock created (without thread tracking) on your thread.
            mres.Wait();

            // now Wait() on worker and clean up
            worker.Wait();
            mres.Dispose();
        }
    }
}