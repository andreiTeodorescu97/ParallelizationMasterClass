using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace _6.ParallelLinq
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //AsParallelExample();
            //CancellationExample();
            //MergeExample();

            //Secvential
            var sum = Enumerable.Range(1, 1000).Sum();
            Console.WriteLine(sum.ToString());

            //Secvential
            var sumS = Enumerable.Range(1, 1000)
                .Aggregate(0, (i, acc) => i + acc);
            Console.WriteLine(sumS.ToString());

            //Multi-threaded
            var sumP = ParallelEnumerable.Range(1, 1000)
                .Aggregate(
                    0,
                    (partialSum, i) => partialSum += i,
                    (total, subtotal) => total += subtotal,
                    i => i);
            Console.WriteLine(sumP.ToString());
        }

        private static void MergeExample()
        {
            var numbers = Enumerable.Range(1, 20).ToArray();
            var results = numbers
                .AsParallel()
                .WithMergeOptions(ParallelMergeOptions.FullyBuffered)
                .Select(x =>
                {
                    var result = x;
                    Console.Write($"P:{result} \t");
                    return result;
                });
            Console.WriteLine("");

            foreach (var item in results)
            {
                Console.Write($"C:{item} \t");
            }
        }

        private static void CancellationExample()
        {
            var items = ParallelEnumerable.Range(1, 20);
            var cts = new CancellationTokenSource();
            var results = items
                .WithCancellation(cts.Token)
                .Select(x =>
                {
                    double result = Math.Log10(x);
                    //if(result > 1) throw new InvalidOperationException();
                    Console.WriteLine($"{x} taskId = {Task.CurrentId}");
                    return result;
                });

            try
            {
                foreach (var item in results)
                {
                    if (item > 1) cts.Cancel();

                    Console.WriteLine($"{item}");
                }
            }
            catch (AggregateException ae)
            {
                ae.Handle(e =>
                {
                    Console.WriteLine($"{e.GetType().Name}: {e.Message}");
                    return true;
                });
            }
            catch (OperationCanceledException ex)
            {
                Console.WriteLine($"{ex.GetType().Name}: {ex.Message}");
            }
        }

        private static void AsParallelExample()
        {
            const int count = 50;
            var items = Enumerable.Range(1, count).ToArray();

            var results = new int[count];

            items
                .AsParallel()
                .ForAll(x =>
                {
                    int newValue = x * x * x;
                    Console.WriteLine($"{newValue} ({Task.CurrentId})");
                    results[x - 1] = newValue;

                });
            Console.WriteLine();
            Console.WriteLine();

            //foreach ( var item in results) 
            //{
            //    Console.WriteLine(item);
            //}

            var cubes = items
                .AsParallel()
                .AsOrdered()
                .Select(x => x * x * x);

            //After this cubes is a plan of execution
            //The actual calculation it happens on enumeration or materialize (turn into Array or List)
            foreach (var item in cubes)
            {
                Console.WriteLine(item);
            }
        }
    }
}
