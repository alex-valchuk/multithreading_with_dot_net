using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace BufferBlockExample
{
    // Demonstrates a how to write to and read from a dataflow block.
    static class Program
    {
        static async Task Main()
        {
            var bufferBlock = new BufferBlock<int>();

            PostReceive(bufferBlock);
            // Output:
            //   0
            //   1
            //   2

            PostTryReceive(bufferBlock);
            // Output:
            //   0
            //   1
            //   2

            await ConcurrentSendReceiveAsync(bufferBlock);
            // Output:
            //   0
            //   1
            //   2

            await SendReceiveAsync(bufferBlock);
            // Output:
            //   0
            //   1
            //   2
        }

        private static void PostReceive(BufferBlock<int> bufferBlock)
        {
            // Post several messages to the block.
            for (int i = 0; i < 3; i++)
            {
                bufferBlock.Post(i);
            }

            // Receive the messages back from the block.
            for (int i = 0; i < 3; i++)
            {
                Console.WriteLine(bufferBlock.Receive());
            }
        }

        private static void PostTryReceive(BufferBlock<int> bufferBlock)
        {
            Console.WriteLine();
            
            // Post more messages to the block.
            for (int i = 0; i < 3; i++)
            {
                bufferBlock.Post(i);
            }

            // Receive the messages back from the block.
            while (bufferBlock.TryReceive(out int value))
            {
                Console.WriteLine(value);
            }
        }

        // Write to and read from the message block concurrently.
        private static async Task ConcurrentSendReceiveAsync(BufferBlock<int> bufferBlock)
        {
            Console.WriteLine();
            
            var post01 = Task.Run(() =>
            {
                bufferBlock.Post(0);
                bufferBlock.Post(1);
            });
            var receive = Task.Run(() =>
            {
                for (int i = 0; i < 3; i++)
                {
                    Console.WriteLine(bufferBlock.Receive());
                }
            });
            var post2 = Task.Run(() => { bufferBlock.Post(2); });

            await Task.WhenAll(post01, receive, post2);
        }

        // Demonstrates asynchronous dataflow operations.
        static async Task SendReceiveAsync(BufferBlock<int> bufferBlock)
        {
            Console.WriteLine();

            // Post more messages to the block asynchronously.
            for (int i = 0; i < 3; i++)
            {
                await bufferBlock.SendAsync(i);
            }

            // Asynchronously receive the messages back from the block.
            for (int i = 0; i < 3; i++)
            {
                Console.WriteLine(await bufferBlock.ReceiveAsync());
            }
        }
    }
}