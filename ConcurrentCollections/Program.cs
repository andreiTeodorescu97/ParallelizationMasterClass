using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ConcurrentCollections
{
    internal class Program
    {
        private static ConcurrentDictionary<string, string> capitals
            = new ConcurrentDictionary<string, string>();

        public static void AddParis()
        {
            bool success = capitals.TryAdd("France", "Paris");
            string who = Task.CurrentId.HasValue ? ("Task " + Task.CurrentId) : "Main Thread";
            Console.WriteLine($"{who} {(success ? " added" : " did not add")} the element");
        }

        private static BlockingCollection<int> messages =
            new BlockingCollection<int>(new ConcurrentBag<int>(), 10);

        private static CancellationTokenSource cts = new CancellationTokenSource();
        private static Random random = new Random();

        static void Main(string[] args)
        {
            //1. Concurrent Dictionary
            //Thread safety collections
            //You can use the same collection on different threads
            ConcurrentDictionaryExample();

            //2. Concurrent Queue (FIFO)
            //Same approach as a ConcurrentDictionary

            //3. Concurrent Stack (LIFO)
            //Same approach as a ConcurrentDictionary

            //4. Concurrent Bag (no ordering)
            //TPL does not have a concurrent list implementation
            //Keeps a list a elements for each thread.
            //Collection optimized for speed!
            ConcurrentBagExample();

            //5.BlockingCollection and Producer-Consumer Pattern
            AddSomeSpacesIntoConsole();
            Task.Factory.StartNew(ProduceAndConsume, cts.Token);
            Console.WriteLine("Press a key to finish execution: ");
            Console.ReadKey();
            cts.Cancel();

            Console.WriteLine("Main program done!");
        }



        private static void ProduceAndConsume()
        {
            var producer = Task.Factory.StartNew(RunProducer);
            var consumner = Task.Factory.StartNew(RunConsumer);

            try
            {
                Task.WaitAll(new[] { producer, consumner }, cts.Token);
            }
            catch (AggregateException ex)
            {
                ex.Handle(e => true);
            }
        }

        private static void RunConsumer()
        {
            foreach (var item in messages.GetConsumingEnumerable())
            {
                cts.Token.ThrowIfCancellationRequested();
                Console.WriteLine($"--{item}! \t");
                //Console.WriteLine($"Thread: {Thread.CurrentThread.ManagedThreadId} Task: {Task.CurrentId} has consumed {item}");
                Thread.Sleep(random.Next(1000));
            }
        }

        private static void RunProducer()
        {
            while (true)
            {
                cts.Token.ThrowIfCancellationRequested();
                int i = random.Next(100);
                messages.Add(i);
                Console.WriteLine($"++{i}! \t");
                //Console.WriteLine($"Thread: {Thread.CurrentThread.ManagedThreadId} Task: {Task.CurrentId} has produced {i}");
                Thread.Sleep(random.Next(1000));
            }
        }

        private static void ConcurrentBagExample()
        {
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("");

            var bag = new ConcurrentBag<int>();

            var tasks = new List<Task>();
            for (int i = 0; i < 10; i++)
            {
                var i1 = i;
                tasks.Add(Task.Factory.StartNew(() =>
                {
                    bag.Add(i1);
                    Console.WriteLine($"Thread: {Thread.CurrentThread.ManagedThreadId} Task: {Task.CurrentId} has added {i1}");
                    int result;
                    if (bag.TryPeek(out result))
                    {
                        Console.WriteLine($"Thread: {Thread.CurrentThread.ManagedThreadId} Task: {Task.CurrentId} has peeked the value: {result}");
                    }
                }));
            }

            Task.WaitAll(tasks.ToArray());

            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("");

            for (int i = 0; i < 10; i++)
            {
                var i1 = i;
                int result;
                if (bag.TryPeek(out result))
                {
                    Console.WriteLine($"Thread: {Thread.CurrentThread.ManagedThreadId} Task: {Task.CurrentId} has peeked the value: {result}");
                }
            }
            int last;
            if (bag.TryTake(out last))
            {
                Console.WriteLine($"Last element of the bag: {last}");
            }
        }

        private static void ConcurrentDictionaryExample()
        {
            Task.Factory.StartNew(AddParis).Wait();
            AddParis();

            //Fails sillently
            //Modify an element
            capitals["Russia"] = "Leningrad";
            capitals["Russia"] = "Moscow";
            Console.WriteLine($"{capitals["Russia"]}");

            //If an element is there you can do smth else
            capitals.AddOrUpdate("Russia", "Ploiesti", (key, old) => old + "--> Ploiesti");
            Console.WriteLine($"{capitals["Russia"]}");

            capitals.AddOrUpdate("Japan", "Tokyo", (key, old) => old + "--> Targoviste");
            Console.WriteLine($"{capitals["Japan"]}");

            capitals["Sweden"] = "Uppsala";
            var capOfSweden = capitals.GetOrAdd("Sweden", "Stockholm");
            Console.WriteLine($"{capitals["Sweden"]}");

            var capOfItaly = capitals.GetOrAdd("Italy", "Rome");
            Console.WriteLine($"{capitals["Italy"]}");

            const string toRemove = "Russia";
            string removed;
            var didRemove = capitals.TryRemove(toRemove, out removed);
            if (didRemove)
            {
                Console.WriteLine("Removed Russia!");
            }
            else
            {
                Console.WriteLine("Failed to remove Russia!");
            }
        }

        private static void AddSomeSpacesIntoConsole()
        {
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("");
        }
    }
}
