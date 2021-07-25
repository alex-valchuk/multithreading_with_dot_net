using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ReaderWriterLockSlimExample
{
   class Program
   {
      public static void Main()
      {
         var synchronizedCache = new SynchronizedCache();
         var tasks = new List<Task>();
         int itemsWritten = 0;

         // Execute a writer.
         tasks.Add(Task.Run(() =>
         {
            String[] vegetables =
            {
               "broccoli", "cauliflower",
               "carrot", "sorrel", "baby turnip",
               "beet", "brussel sprout",
               "cabbage", "plantain",
               "spinach", "grape leaves",
               "lime leaves", "corn",
               "radish", "cucumber",
               "raddichio", "lima beans"
            };
            
            for (int i = 0; i < vegetables.Length; i++)
            {
               synchronizedCache.Add(i, vegetables[i]);
            }

            itemsWritten = vegetables.Length;
            Console.WriteLine("Task {0} wrote {1} items\n", Task.CurrentId, itemsWritten);
         }));
         
         // Execute two readers, one to read from first to last and the second from last to first.
         for (int ctr = 0; ctr < 2; ctr++)
         {
            bool desc = ctr == 1;
            tasks.Add(Task.Run(() =>
            {
               int start, last, step;
               int items;
               do
               {
                  String output = String.Empty;
                  items = synchronizedCache.Count;
                  if (!desc)
                  {
                     start = 0;
                     step = 1;
                     last = items;
                  }
                  else
                  {
                     start = items - 1;
                     step = -1;
                     last = 0;
                  }

                  for (int index = start; desc ? index >= last : index < last; index += step)
                  {
                     output += string.Format("[{0}] ", synchronizedCache.Read(index));
                  }

                  Console.WriteLine("Task {0} read {1} items: {2}\n", Task.CurrentId, items, output);
               } while (items < itemsWritten | itemsWritten == 0);
            }));
         }

         // Execute a red/update task.
         tasks.Add(Task.Run(() =>
         {
            Thread.Sleep(100);
            for (int i = 0; i < synchronizedCache.Count; i++)
            {
               String value = synchronizedCache.Read(i);
               if (value == "cucumber")
                  if (synchronizedCache.AddOrUpdate(i, "green bean") != SynchronizedCache.AddOrUpdateStatus.Unchanged)
                     Console.WriteLine("Changed 'cucumber' to 'green bean'");
            }
         }));

         // Wait for all three tasks to complete.
         Task.WaitAll(tasks.ToArray());

         // Display the final contents of the cache.
         Console.WriteLine();
         Console.WriteLine("Values in synchronized cache: ");
         for (int i = 0; i < synchronizedCache.Count; i++)
         {
            Console.WriteLine("   {0}: {1}", i, synchronizedCache.Read(i));
         }
      }
   }
}