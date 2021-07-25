using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SpinLockExample
{
    public static class SpinLockSample1
    {
        public static void Execute()
        {
            SpinLock spinLock = new SpinLock();

            StringBuilder sb = new StringBuilder();

            // Action taken by each parallel job.
            // Append to the StringBuilder 10000 times, protecting
            // access to sb with a SpinLock.
            void Action()
            {
                bool gotLock;
                for (int i = 0; i < 10_000; i++)
                {
                    gotLock = false;
                    try
                    {
                        spinLock.Enter(ref gotLock);
                        sb.Append((i % 10).ToString());
                    }
                    finally
                    {
                        // Only give up the lock if you actually acquired it
                        if (gotLock)
                        {
                            spinLock.Exit();
                        }
                    }
                }
            }

            // Invoke 3 concurrent instances of the action above
            Parallel.Invoke((Action) Action, Action, Action);

            // Check/Show the results
            Console.WriteLine("sb.Length = {0} (should be 30000)", sb.Length);
            Console.WriteLine("number of occurrences of '5' in sb: {0} (should be 3000)",
                sb.ToString().Count(c => c == '5'));
        }
    }
}