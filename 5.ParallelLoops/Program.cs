using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace _5.ParallelLoops
{
    public class Program
    {
        [Benchmark]
        public void SquareEachValue()
        {
            const int count = 1000;
            var values = Enumerable.Range(0, count);
            var results = new int[count];

            //it creates a lot of delegates, very inefficient
            Parallel.ForEach(values, x => { results[x] = (int)Math.Pow(x, 2); });
        }

        [Benchmark]
        public void SquareEachValueChunked()
        {
            const int count = 1000;
            var values = Enumerable.Range(0, count);
            var results = new int[count];

            var part = Partitioner.Create(0, count, 100);
            Parallel.ForEach(part, range =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    results[i] = (int)Math.Pow(i, 2);
                }
            });
        }

        public static void Main(string[] args)
        {
            //1. Parallel Invoke/For/Foreach
            //Parallel_Invoke_For_ForEachExample();

            //2. Breaking, Cancellations and Exceptions
            //try
            //{
            //    DemoCancellaltions();
            //}
            //catch (AggregateException ex)
            //{
            //    ex.Handle(e =>
            //    {
            //        Console.WriteLine(e.Message);
            //        return true;
            //    });
            //}
            //catch (OperationCanceledException ex)
            //{
            //    Console.WriteLine(ex.Message);
            //}

            //3. Thread Local Storage
            //ThreadLocalStorageExample();
            //Console.WriteLine();

            //4. Partitioning
            var summary = BenchmarkRunner.Run<Program>();
            Console.WriteLine(summary);

            //5. Summary



            Console.WriteLine("Main program done!");
        }



        private static void ThreadLocalStorageExample()
        {
            int sum = 0;
            //Parallel.For(1, 10001, x =>
            //{
            //    Interlocked.Add(ref sum, x);
            //});
            Parallel.For(1, 11,
                () => 0,
                (x, state, tls) =>
                {
                    Console.WriteLine($"Task {Task.CurrentId}[{Thread.CurrentThread.ManagedThreadId}] has X {x}");
                    tls += x;
                    Console.WriteLine($"Task {Task.CurrentId}[{Thread.CurrentThread.ManagedThreadId}] has tls {tls}");
                    return tls;
                }, partialSum =>
                {
                    Console.WriteLine($"Task {Task.CurrentId}[{Thread.CurrentThread.ManagedThreadId}] has partialsum {partialSum}");
                    Interlocked.Add(ref sum, partialSum);
                });

            Console.WriteLine($"The sum of 1 to 1000 is {sum}");
        }

        private static ParallelLoopResult result;
        public static void DemoCancellaltions()
        {
            var cts = new CancellationTokenSource();
            var po = new ParallelOptions();
            po.CancellationToken = cts.Token;

            result = Parallel.For(0, 20, po, (x, state) => 
            {
                Console.WriteLine($"{x} has task {Task.CurrentId}");
                if(x == 10)
                {
                    //throw new Exception();
                    //state.Stop();
                    //state.Break();
                    cts.Cancel();
                }
            });

            Console.WriteLine();
            Console.WriteLine($"Was the loop completed: {result.IsCompleted}");
            if(result.LowestBreakIteration.HasValue) 
            {
                Console.WriteLine($"Lowest break iteration is: {result.LowestBreakIteration}");
            }
            Console.WriteLine();
        }
        private static void Parallel_Invoke_For_ForEachExample()
        {
            Console.WriteLine("Invoke Example");
            var a = new Action(() => { Console.WriteLine($"First task {Task.CurrentId} on thread {Thread.CurrentThread.ManagedThreadId}"); });
            var b = new Action(() => { Console.WriteLine($"Second task {Task.CurrentId} on thread {Thread.CurrentThread.ManagedThreadId}"); });
            var c = new Action(() => { Console.WriteLine($"Third task {Task.CurrentId} on thread {Thread.CurrentThread.ManagedThreadId}"); });
            var d = new Action(() => { Console.WriteLine($"Forth task {Task.CurrentId} on thread {Thread.CurrentThread.ManagedThreadId}"); });
            var e = new Action(() => { Console.WriteLine($"Fifth task {Task.CurrentId} on thread {Thread.CurrentThread.ManagedThreadId}"); });
            var f = new Action(() => { Console.WriteLine($"Sixth task {Task.CurrentId} on thread {Thread.CurrentThread.ManagedThreadId}"); });
            var g = new Action(() => { Console.WriteLine($"Seventh task {Task.CurrentId} on thread {Thread.CurrentThread.ManagedThreadId}"); });
            var h = new Action(() => { Console.WriteLine($"Eighth task {Task.CurrentId} on thread {Thread.CurrentThread.ManagedThreadId}"); });
            var i = new Action(() => { Console.WriteLine($"Nineth task {Task.CurrentId} on thread {Thread.CurrentThread.ManagedThreadId}"); });
            var o = new Action(() => { Console.WriteLine($"The ten task {Task.CurrentId} on thread {Thread.CurrentThread.ManagedThreadId}"); });

            Parallel.Invoke(a, b, c, d, e, f, g, h, i, o);
            Console.WriteLine("");

            Console.WriteLine("For Example");
            Parallel.For(1, 11, i => { Console.WriteLine($"{i * i} -- task {Task.CurrentId} on thread {Thread.CurrentThread.ManagedThreadId} \t"); });
            Console.WriteLine("");

            Console.WriteLine("ForEach Example");
            string[] words = { "dasdsadasdasdas", "321", "rewrew", "ds321321adas" };
            Parallel.ForEach(words, word =>
            {
                Console.WriteLine($"{word} has length {word.Length} -- task {Task.CurrentId} on thread {Thread.CurrentThread.ManagedThreadId}");
            });
            Console.WriteLine("");
        }
    }
}
