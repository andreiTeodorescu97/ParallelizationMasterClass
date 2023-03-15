using System;
using System.Threading;
using System.Threading.Tasks;

namespace TaskCoordination
{
    internal class Program
    {
        //Barrier
        static Barrier barrier = new Barrier(2, b =>
        {
            Console.WriteLine($"Phase {b.CurrentPhaseNumber} is finished");
        });

        //CountDownEvent
        private static int taskCount = 5;
        static readonly CountdownEvent cte = new CountdownEvent(taskCount);
        static Random random = new Random();

        //ManualResetEventSlim
        private static ManualResetEventSlim mre = new ManualResetEventSlim();
        private static AutoResetEvent are = new AutoResetEvent(false);

        //SemaphoreSlim
        private static SemaphoreSlim sme = new SemaphoreSlim(2, 10);

        static void Main(string[] args)
        {
            //1.Continuations
            ContinuationsExample();
            Console.WriteLine("");

            //2. Child tasks
            ChildTasksExample();
            Console.WriteLine("");

            //3. Barrier
            BarrierExample();
            Console.WriteLine("");

            //4. Countdown Event
            CountDownEventExample();
            Console.WriteLine("");

            //5. ManualResetEventSlim and AutoResetEvent
            ManualResetEventSlimAndAutoResetEvent();
            Console.WriteLine("");

            //6. SemaphoreSlim
            SemaphoreSlimExample();
            Console.WriteLine("");

            Console.WriteLine("Main program done!");
        }

        private static void SemaphoreSlimExample()
        {
            for (int i = 0; i < 20; i++)
            {
                Task.Factory.StartNew(() =>
                {
                    Console.WriteLine($"Entering task {Task.CurrentId} from Thread: {Thread.CurrentThread.ManagedThreadId}.");
                    sme.Wait(); //ReleaseCount--
                    Console.WriteLine($"Processing task {Task.CurrentId} from Thread: {Thread.CurrentThread.ManagedThreadId}.");
                });
            }

            while (sme.CurrentCount <= 2)
            {
                Console.WriteLine($"Semaphore count: {sme.CurrentCount}.");
                Console.ReadKey();
                sme.Release(2); //ReseCount = ReleaseCount + 2
            }
        }

        private static void ManualResetEventSlimAndAutoResetEvent()
        {
            Task.Factory.StartNew(() =>
            {
                Console.WriteLine($"Boiling Water. Task: {Task.CurrentId} from Thread: {Thread.CurrentThread.ManagedThreadId}.");
                mre.Set();
            });

            var makeTea = Task.Factory.StartNew(() =>
            {
                Console.WriteLine($"Waiting for water. Task: {Task.CurrentId} from Thread: {Thread.CurrentThread.ManagedThreadId}.");
                mre.Wait();
                Console.WriteLine($"Tea is done. Task: {Task.CurrentId} from Thread: {Thread.CurrentThread.ManagedThreadId}.");
            });
            makeTea.Wait();

            Console.WriteLine("--------");

            Task.Factory.StartNew(() =>
            {
                Console.WriteLine($"Boiling Water. Task: {Task.CurrentId} from Thread: {Thread.CurrentThread.ManagedThreadId}.");
                are.Set(); // event becomes true
            });

            var makeTea2 = Task.Factory.StartNew(() =>
            {
                Console.WriteLine($"Waiting for water. Task: {Task.CurrentId} from Thread: {Thread.CurrentThread.ManagedThreadId}.");
                are.WaitOne(); // waits for the event to be true, execute the code and the event will be false again
                Console.WriteLine($"Tea is done. Task: {Task.CurrentId} from Thread: {Thread.CurrentThread.ManagedThreadId}.");
                var ok = are.WaitOne(1000);
                if (ok)
                {
                    Console.WriteLine("Enjoy your tea");
                }
                else
                {
                    //This will always get executed because there is no signal that could make the event true again
                    Console.WriteLine("No tea for you!");

                }
            });
            makeTea2.Wait();
        }

        private static void CountDownEventExample()
        {
            for (int i = 0; i < taskCount; i++)
            {
                Task.Factory.StartNew(() =>
                {
                    Console.WriteLine($"Entering Task: {Task.CurrentId} from Thread: {Thread.CurrentThread.ManagedThreadId}. CurrentCount: {cte.CurrentCount}");
                    Thread.Sleep(random.Next(3000));
                    cte.Signal(); //countdown--
                    Console.WriteLine($"Exiting Task: {Task.CurrentId} from Thread: {Thread.CurrentThread.ManagedThreadId}. CurrentCount: {cte.CurrentCount}");
                });
            }

            var finalTask = Task.Factory.StartNew(() =>
            {
                Console.WriteLine($"Waiting for other tasks to complete in Task: {Task.CurrentId} from Thread: {Thread.CurrentThread.ManagedThreadId}");
                cte.Wait(); //when countdown = 0 than next code executes
                Console.WriteLine($"All tasks completed succesfully! CurrentCount: {cte.CurrentCount}");
            });
            finalTask.Wait();
            Console.WriteLine($"Final CurrentCount: {cte.CurrentCount}");
        }

        private static void BarrierExample()
        {
            var water = Task.Factory.StartNew(Water);
            var cup = Task.Factory.StartNew(Cup);
            var tea = Task.Factory.ContinueWhenAll(new[] { water, cup }, tasks =>
            {
                Console.WriteLine("Enjoy your cup of tea");
            });
            tea.Wait();
        }

        private static void Cup()
        {
            Console.WriteLine("Finding the nicest cup of tea!");
            barrier.SignalAndWait();
            Console.WriteLine("Adding tea to the cup!");
            barrier.SignalAndWait();
            Console.WriteLine("Adding sugar!");
        }

        private static void Water()
        {
            Console.WriteLine("Putting the kettle on (take a bit longer)");
            Thread.Sleep(2000);
            barrier.SignalAndWait();
            Console.WriteLine("Pouring water into the cup!");
            barrier.SignalAndWait();
            Console.WriteLine("Putting the kettle away!");
        }

        private static void ChildTasksExample()
        {
            Console.WriteLine("Child tasks example!");
            var parent = new Task(() =>
            {
                Console.WriteLine($"Parent task starting on Thread: {Thread.CurrentThread.ManagedThreadId}!");

                //detached
                var child = new Task(() =>
                {
                    Console.WriteLine($"Child task starting on Thread: {Thread.CurrentThread.ManagedThreadId}!");
                    Thread.Sleep(3000);
                    //Uncomment to make the task fail
                    //throw new Exception();
                    Console.WriteLine("Child task finishing!");

                }, TaskCreationOptions.AttachedToParent);

                var completionHandler = child.ContinueWith(t =>
                {
                    Console.WriteLine($"Hooray, task {t.Id}'s state is {t.Status}");
                }, TaskContinuationOptions.AttachedToParent | TaskContinuationOptions.OnlyOnRanToCompletion);

                var failHandler = child.ContinueWith(t =>
                {
                    Console.WriteLine($"Hooray, task {t.Id}'s state is {t.Status}");
                }, TaskContinuationOptions.AttachedToParent | TaskContinuationOptions.OnlyOnFaulted);

                child.Start();
            });
            parent.Start();

            try
            {
                parent.Wait();
            }
            catch (AggregateException ex)
            {
                ex.Handle(e => true);
            }
        }

        private static void ContinuationsExample()
        {
            Console.WriteLine("Continuations example!");

            var task = Task.Factory.StartNew(() =>
            {
                Console.WriteLine("Boiling Water!");
            });
            //t is the original task that has been completed
            var task2 =
                task.ContinueWith(t => { Console.WriteLine($"Completed task {t.Id}, pour water into cup."); });

            task2.Wait();

            var task3 = Task.Factory.StartNew(() => "Task 3");
            var task4 = Task.Factory.StartNew(() => "Task 4");

            var task5 = Task.Factory.ContinueWhenAll(new[] { task3, task4 }, tasks =>
            {
                Console.WriteLine("Tasks 3 and 4 completed!");
                foreach (var t in tasks)
                {
                    Console.WriteLine("-" + t.Result);
                }
                Console.WriteLine("All tasks done!");

            });
            task5.Wait();
        }
    }
}
