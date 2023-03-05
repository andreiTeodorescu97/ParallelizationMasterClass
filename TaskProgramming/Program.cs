using System;
using System.Threading;
using System.Threading.Tasks;

namespace TaskProgramming
{
    internal class Program
    {
        public static void Write(char c)
        {
            int i = 100;
            while (i-- > 0)
            {
                Console.Write(c);
            }
        }

        public static void Write(object o)
        {
            int i = 100;
            while (i-- > 0)
            {
                Console.Write(o.ToString());
            }
        }

        public static int TextLenth(object o)
        {
            Console.WriteLine($"Task with id {Task.CurrentId} processing object {o}..\n");
            return o.ToString().Length;
        }

        static void Main(string[] args)
        {
            //TASK = A UNIT OF WORK
            //1. CREATING AND STARTING TASKS
            StartingTaskExample();

            //2. CANCELLING TASKS
            CancellingTasks();

            //3. WAITING FOR TIME TO PASS
            WaitingForTimeToPass();

            //4. WAITING FOR TASKS
            WaitingForTasks();

            //5. EXCEPTION HANDLING
            ExceptionHandling();
        }

        private static void ExceptionHandling()
        {
            try
            {
                TestExceptionHandling();
            }
            catch (AggregateException ae)
            {
                foreach (var e in ae.InnerExceptions)
                {
                    Console.WriteLine($"Exception {e.GetType()} from {e.Source}");
                }
            }
            Console.WriteLine("Main program done!");
            Console.ReadLine();
        }

        private static void TestExceptionHandling()
        {
            var t = Task.Factory.StartNew(() =>
            {
                throw new InvalidOperationException("CAN NOT DO THIS") { Source = "t" };
            });
            var t2 = Task.Factory.StartNew(() =>
            {
                throw new AccessViolationException("YOU DO NOT HAVE ACCES!") { Source = "t2" };
            });
            try
            {
                Task.WaitAll(t, t2);

            }
            catch (AggregateException ae)
            {
                ae.Handle(e =>
                {
                    if (e is InvalidOperationException)
                    {
                        Console.WriteLine("Invalid op!");
                        return true;
                    }
                    else { return false; }
                });
            }
        }


        private static void WaitingForTasks()
        {
            var cts = new CancellationTokenSource();
            var token = cts.Token;

            var t = new Task(() =>
            {
                Console.WriteLine("I take 5 seconds");
                for (int i = 0; i < 5; i++)
                {
                    token.ThrowIfCancellationRequested();
                    Thread.Sleep(1000);
                }
                Console.WriteLine("I am done!");

            }, token);
            t.Start();
            //t.Wait(token);
            Task t2 = Task.Factory.StartNew(() => Thread.Sleep(3000), token);
            //Task.WaitAll(t, t2);
            //Task.WaitAny(t, t2);
            Task.WaitAll(new[] { t, t2 }, 4000, token);

            Console.WriteLine($"Task 1 status is {t.Status}");
            Console.WriteLine($"Task 2 status is {t2.Status}");
        }

        private static void WaitingForTimeToPass()
        {
            var cts = new CancellationTokenSource();
            var token = cts.Token;

            var t = new Task(() =>
            {
                //Thread.Sleep(99999);
                //more efficient if we need to wait just a little bit, because thread doest not lose execution context
                //SpinWait.SpinUntil();

                Console.WriteLine("Press any key to disarm; you have 5 seconds");
                bool isTaskCanceled = token.WaitHandle.WaitOne(5000);
                if (isTaskCanceled == false)
                {
                    Console.WriteLine("BUMMM!");
                }
                else
                {
                    Console.WriteLine("BOMB DISARMED!");
                }

            }, token);

            t.Start();
            Console.ReadKey();
            cts.Cancel();
        }

        private static void CancellingTasks()
        {
            var cts = new CancellationTokenSource();
            var token = cts.Token;

            //register an event that will trigger when the task is cancelled
            token.Register(() =>
            {
                Console.WriteLine("Cancellation requested");
            });

            var t = new Task(() =>
            {
                int i = 0;
                while (true)
                {
                    token.ThrowIfCancellationRequested();
                    Console.WriteLine($"{i++}\t");
                }
            }, token);

            t.Start();

            Task.Factory.StartNew(() =>
            {
                Console.WriteLine("Ala bala portocala!");
                token.WaitHandle.WaitOne();
                Console.WriteLine("Wait handle has beed released, cancellation requested");
            });

            Console.ReadKey();
            cts.Cancel();

            //COMPOSITE CANCELLATION TOKENS
            var planned = new CancellationTokenSource();
            var preventative = new CancellationTokenSource();
            var emergency = new CancellationTokenSource();

            var paranoid = CancellationTokenSource.CreateLinkedTokenSource(planned.Token, preventative.Token, emergency.Token);

            Task.Factory.StartNew(() =>
            {
                int i = 0;
                while (true)
                {
                    paranoid.Token.ThrowIfCancellationRequested();
                    Console.WriteLine($"{i++}\t");
                    Thread.Sleep(1000);
                }
            }, paranoid.Token);

            Console.ReadKey();
            emergency.Cancel();
            planned.Cancel();
            preventative.Cancel();
        }

        private static void StartingTaskExample()
        {
            Task.Factory.StartNew(() => Write('.'));
            var t = new Task(() => Write('?'));

            var t = new Task(Write, "hello");
            t.Start();
            Task.Factory.StartNew(Write, 123);

            string text1 = "Testing", text2 = "this";
            var task1 = new Task<int>(TextLenth, text1);
            task1.Start();
            Task<int> task2 = Task.Factory.StartNew(TextLenth, text2);
            //When you want the result of a task this is a blocking instructions because it should wait for the task to complete
            Console.WriteLine($"Length {text1} is {task1.Result}");
            Console.WriteLine($"Length {text2} is {task2.Result}");
        }
    }
}