using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace BroadcastBlockExample
{
    static class Program
    {
        static int Delay = 500;
        static async Task Main()
        {
            var broadcastBlock = new BroadcastBlock<string>(null);

            var sendTask = Task.Run(() =>
            {
                broadcastBlock.Post("Enemy spotted!");
                Thread.Sleep(Delay);
                broadcastBlock.Post("Fire in the hole!");
                Thread.Sleep(Delay);
                broadcastBlock.Post("Affirmative!");
                Thread.Sleep(Delay);
            
                broadcastBlock.Complete();
            });

            var receiveTask = Task.Run(() =>
            {
                while (!broadcastBlock.Completion.IsCompleted)
                {
                    Console.WriteLine("Received:" + broadcastBlock.Receive());
                    Thread.Sleep(Delay);
                }
            });

            await Task.WhenAll(sendTask, receiveTask);
        }
    }
}