using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DataSharingAndSynchronization
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //1. CRITICAL SECTIONS
            // Critical section = only one thread can enter this area!
            // Using a padlock means that only one thread will execute that piece of code at a time!
            CriticalSectionsExample();

            //2. INTERLOCKED OPERATIONS
            InterlockedExample();

            //3. Spin Lock and Lock Recursion
            SpinLockExample();

            //4. Mutex - very powerfull synchronization primitive
            //Mutex is smth like a wait handle
            MutexExample();

            //5. Reader Writer lock - smarter kind of lock
            ReaderWriterLockExample();

            Console.WriteLine("Main program done!");
        }
        private static void CriticalSectionsExample()
        {
            Console.WriteLine("Critical Sections:");

            var ba = new CriticalSectionBankAccount();
            var tasks = new List<Task>();

            for (int i = 0; i < 4; i++)
            {
                tasks.Add(Task.Factory.StartNew(() =>
                {
                    for (int i = 0; i < 10; i++)
                    {
                        ba.Deposit(1);
                    }
                }));
                tasks.Add(Task.Factory.StartNew(() =>
                {
                    for (int i = 0; i < 10; i++)
                    {
                        ba.WithDraw(1);
                    }
                }));
            }

            Task.WaitAll(tasks.ToArray());
            Console.WriteLine($"Final ballance is: {ba.Balance}");
            Console.WriteLine($"");
        }

        private static void InterlockedExample()
        {
            Console.WriteLine("Interlocked Operations:");

            var ba = new InterlockedBankAccount();
            var tasks = new List<Task>();

            for (int i = 0; i < 4; i++)
            {
                tasks.Add(Task.Factory.StartNew(() =>
                {
                    for (int i = 0; i < 10; i++)
                    {
                        ba.Deposit(1);
                    }
                }));
                tasks.Add(Task.Factory.StartNew(() =>
                {
                    for (int i = 0; i < 10; i++)
                    {
                        ba.WithDraw(1);
                    }
                }));
            }

            Task.WaitAll(tasks.ToArray());
            Console.WriteLine($"Final ballance is: {ba.Balance}");
            Console.WriteLine($"");
        }

        private static void SpinLockExample()
        {
            Console.WriteLine("SpinLock Operations:");

            var ba = new SpinLockBankAccount();
            var tasks = new List<Task>();

            //Control acces to variables
            SpinLock sl = new SpinLock();

            for (int i = 0; i < 4; i++)
            {
                tasks.Add(Task.Factory.StartNew(() =>
                {
                    for (int i = 0; i < 10; i++)
                    {
                        var lockTaken = false;
                        try
                        {
                            sl.Enter(ref lockTaken);
                            ba.Deposit(1);
                        }
                        finally
                        {
                            if (lockTaken) sl.Exit();
                        }
                    }
                }));
                tasks.Add(Task.Factory.StartNew(() =>
                {
                    for (int i = 0; i < 10; i++)
                    {
                        var lockTaken = false;
                        try
                        {
                            sl.Enter(ref lockTaken);
                            ba.WithDraw(1);
                        }
                        finally
                        {
                            if (lockTaken) sl.Exit();
                        }
                    }
                }));
            }

            Task.WaitAll(tasks.ToArray());
            Console.WriteLine($"Final ballance is: {ba.Balance}");
            Console.WriteLine($"");
        }

        private static void MutexExample()
        {
            Console.WriteLine("Mutex Operations:");

            var ba1 = new MutexBankAccount();
            var ba2 = new MutexBankAccount();
            var tasks = new List<Task>();

            //SIMILAR WITH A LOCK
            Mutex mutex1 = new Mutex();
            Mutex mutex2 = new Mutex();

            //mutex1 controls the acces of the thread to ba1, the same for mutex2 and ba2
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(Task.Factory.StartNew(() =>
                {
                    for (int i = 0; i < 100; i++)
                    {
                        bool haveLocked = mutex1.WaitOne();
                        try
                        {
                            ba1.Deposit(1);
                        }
                        finally
                        {
                            if (haveLocked) mutex1.ReleaseMutex();
                        }
                    }
                }));
                tasks.Add(Task.Factory.StartNew(() =>
                {
                    for (int i = 0; i < 100; i++)
                    {
                        bool haveLocked = mutex2.WaitOne();
                        try
                        {
                            ba2.Deposit(1);
                        }
                        finally
                        {
                            if (haveLocked) mutex2.ReleaseMutex();
                        }
                    }
                }));
                tasks.Add(Task.Factory.StartNew(() =>
                {
                    for (int i = 0; i < 100; i++)
                    {
                        //we get this lock only if both mutexes are available
                        bool haveLocked = WaitHandle.WaitAll(new[] { mutex1, mutex2 });
                        try
                        {
                            ba1.Transfer(ba2, 1);
                            ba2.Transfer(ba1, 1);
                        }
                        finally
                        {
                            if (haveLocked)
                            {
                                mutex1.ReleaseMutex();
                                mutex2.ReleaseMutex();
                            }
                        }
                    }
                }));
            }

            Task.WaitAll(tasks.ToArray());
            Console.WriteLine($"Final ballance is: {ba1.Balance}");
            Console.WriteLine($"Final ballance is: {ba2.Balance}");
            Console.WriteLine($"");
        }

        private static void ReaderWriterLockExample()
        {
            int x = 0;
            var padLock = new ReaderWriterLockSlim();
            var random = new Random();

            var tasks = new List<Task>();
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(Task.Factory.StartNew(() =>
                {
                    padLock.EnterReadLock();

                    Console.WriteLine($"Entered ReadLock x = {x} {Thread.CurrentThread.ManagedThreadId}");

                    Thread.Sleep(1000);

                    padLock.ExitReadLock();

                    Console.WriteLine($"Exited ReadLock x = {x}");
                }));
            }

            try
            {
                Task.WaitAll(tasks.ToArray());
            }
            catch (AggregateException ae)
            {
                ae.Handle(e =>
                {
                    Console.WriteLine(e);
                    return true;
                });
            }

            while(true)
            {
                Console.ReadKey();

                padLock.EnterWriteLock();

                Console.WriteLine($"Write lock acquired! {Thread.CurrentThread.ManagedThreadId}");

                int newValue = random.Next(10);
                x = newValue;
                Console.WriteLine($"Set x = {x}");

                padLock.ExitWriteLock();
                Console.WriteLine($"Write lock released! {Thread.CurrentThread.ManagedThreadId}");
            }
        }
    }
}
