using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace WriteOnceBlockExample
{
    static class Program
    {
        static void Main()
        {
            var writeOnceBlock = new WriteOnceBlock<string>(null);
            
            Parallel.Invoke(
                () => writeOnceBlock.Post("Fire in the hole!"),
                () => writeOnceBlock.Post("Enemy spotted!"),
                () => writeOnceBlock.Post("Affirmative!"));
            
            Console.WriteLine(writeOnceBlock.Receive());
        }
    }
}