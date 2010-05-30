
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ApiChange.Infrastructure;
using System.Threading;
using System.Reflection;

namespace UnitTests.Infrastructure
{
    [TestFixture]
    public class WorkItemDispatcherTests : Trace_
    {
        [Test]
        public void Enqueue_Work_Without_WaitingForCompletion()
        {
            int count = 0;
            int Runs = 100;

            for (int k = 0; k < Runs; k++)
            {
                BlockingQueue<string> work = new BlockingQueue<string>();

                WorkItemDispatcher<string> dispatcher = new WorkItemDispatcher<string>(
                    2, // work in 2 threads
                    (data) => Interlocked.Increment(ref count), // increment counter for each work item
                    work
                    );

                for (int i = 0; i < Runs; i++)
                {
                    work.Enqueue("Hello");
                }
                work.Enqueue(null);
            }

            for(int i=0;i<10;i++)
            {
                if (count == Runs * Runs)
                    break;

                Thread.Sleep(100);
            }

            Assert.AreEqual(Runs*Runs, count, "All work items should be processed even when we do not wait for completion.");
        }

        const string ExceptionMessage = "Test excepton";

        [Test]
        public void RethrowException_On_Dispose_By_Default()
        {
            BlockingQueue<string> work = new BlockingQueue<string>();
            WorkItemDispatcher<string> dispatcher = new WorkItemDispatcher<string>(
                5, // work in 5 threads
                (data) =>
                    {
                        throw new Exception(ExceptionMessage);
                    },
                work
                );

            work.Enqueue("some work");
            work.ReleaseWaiters();

            Assert.Throws<AggregateException>(() => dispatcher.Dispose());
        }

        [Test]
        public void ThrowException_On_Enqueue_WhenWorker_Has_Faulted()
        {
            bool bCalled = false;
            BlockingQueue<string> work = new BlockingQueue<string>();

            WorkItemDispatcher<string> dispatcher = new WorkItemDispatcher<string>(
                5, // work in 5 threads
                (data) =>
                {
                    try
                    {
                        throw new Exception(ExceptionMessage);
                    }
                    finally
                    {
                        bCalled = true;
                    }
                },
                work
                );

            work.Enqueue("some work");
            work.ReleaseWaiters();

            while (!bCalled) Thread.Sleep(10);
            Thread.Sleep(10);

            Assert.Throws<AggregateException>(() => dispatcher.Dispose());
        }

        [Test]
        public void Do_Not_Mask_Exceptions_When_Disposing()
        {
            
            Assert.Throws<DataMisalignedException>(() =>
                {
                    bool bCalled = false;
                    BlockingQueue<string> work = new BlockingQueue<string>();

                    using (WorkItemDispatcher<string> dispatcher = new WorkItemDispatcher<string>(
                        5, // work in 5 threads
                        (data) =>
                        {
                            bCalled = true;
                            throw new Exception(ExceptionMessage);
                        },
                        work
                        ))
                    {
                        work.Enqueue("some work");
                        while (!bCalled) Thread.Sleep(10);
                        work.ReleaseWaiters();
                        throw new DataMisalignedException("Some other exception");
                    }
                });
        }

        [Test]
        public void Do_Not_Throw_On_Enqueue_When_Aggregating()
        {
            int called = 0;

            BlockingQueue<string> work = new BlockingQueue<string>();
            WorkItemDispatcher<string> dispatcher = new WorkItemDispatcher<string>(
                5, // work in 5 threads
                (data) =>
                {
                    Interlocked.Increment(ref called);
                    throw new Exception(ExceptionMessage);
                },
                work,
                WorkItemOptions.AggregateExceptions
                );

            work.Enqueue("some work");

            while (called == 0) Thread.Sleep(10);
            
            for (int i = 0; i < 100; i++)
            {
                work.Enqueue("other work");
            }

            work.ReleaseWaiters();

            try
            {
                dispatcher.Dispose();
            }
            catch (AggregateException ex)
            {
                Assert.AreEqual(101, ex.InnerExceptions.Count);
            }
        }

        [Test]
        public void Can_Process_List_Of_Queues()
        {
            var q1 = new BlockingQueue<string>();
            var q2 = new BlockingQueue<string>();
            var q3 = new BlockingQueue<string>();
            var queues = new BlockingQueue<string>[] { q1, q2, q3 };

            var agg = new BlockingQueueAggregator<string>(queues);

            int processed = 0;
            var dispatcher = new WorkItemDispatcher<string>(5,
                (workstring) =>
                {
                    Interlocked.Increment(ref processed);
                },
                "Tester", agg);

            const int Runs = 10;
            for(int i=0;i<Runs;i++)
            {
                for (int k = 0; k < queues.Length; k++)
                {
                    queues[k].Enqueue("q" + k.ToString() + "_" + i.ToString());
                }
            }

            // signal end of each queue
            q1.Enqueue(null);
            q2.Enqueue(null);
            q3.Enqueue(null);

            dispatcher.Dispose();

            Assert.AreEqual(Runs * queues.Length, processed);
        }

        [Test]
        public void Can_Use_WorkItemDispatcher_On_STA_Thread()
        {
            Exception cex = null;
            Thread t = new Thread(() =>
                {
                    try
                    {
                        var queue = new BlockingQueue<string>();

                        int processed = 0;
                        var dispatcher = new WorkItemDispatcher<string>(5,
                            (workstring) =>
                            {
                                Interlocked.Increment(ref processed);
                            },
                            "Tester", queue);

                        queue.Enqueue("Work1");
                        queue.Enqueue("Work2");
                        queue.ReleaseWaiters();

                        dispatcher.Dispose();
                    }
                    catch (Exception ex)
                    {
                        cex = ex;
                    }
                });

            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();

            if (cex != null)
            {
                Assert.Fail("Get Exception On STA Thread: " + cex);
            }
        }
    }
}
