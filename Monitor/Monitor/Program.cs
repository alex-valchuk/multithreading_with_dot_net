using System.Threading;
using System.Threading.Tasks;

namespace MonitorExample
{
    class Program
    {
        static void Main()
        {
            Task.Run(() =>
            {
                var singleton = Singleton.GetSingletonUsingMonitorExplicitly();
                singleton.ShowId("Thread1");
            });

            Task.Run(() =>
            {
                var singleton = Singleton.GetSingletonUsingMonitorImplicitly();
                singleton.ShowId("Thread2");
            });
            
            Thread.Sleep(100);
        }
    }
}