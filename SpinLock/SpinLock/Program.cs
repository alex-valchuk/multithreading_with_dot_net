using System;

namespace SpinLockExample
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Press any key to run a first sample...");
            Console.ReadLine();
            SpinLockSample1.Execute();
            Console.WriteLine("First sample is finished...");
            
            Console.WriteLine("Press any key to run a second sample...");
            Console.ReadLine();
            SpinLockSample2.Execute();
            Console.WriteLine("Second sample is finished...");
            
            Console.WriteLine("Press any key to run a third sample...");
            Console.ReadLine();
            SpinLockSample3.Execute();
            Console.WriteLine("Third sample is finished...");
        }
    }
}