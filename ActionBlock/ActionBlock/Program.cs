using System;
using System.Threading.Tasks.Dataflow;

namespace ActionBlockExample
{
    static class Program
    {
        static void Main()
        {
            var actionBlock = new ActionBlock<int>(Console.WriteLine);

            for (int i = 0; i < 3; i++)
            {
                actionBlock.Post(i);
            }

            actionBlock.Complete();
            actionBlock.Completion.Wait();
        }
    }
}