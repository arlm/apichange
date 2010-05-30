
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ApiChange.Api.Scripting;
using System.Threading;
using ApiChange.Infrastructure;

namespace UnitTests.Infrastructure
{
    [TestFixture]
    public class BlockingQueueTests
    {
        int QueueItems = 20*1000;

        [Test]
        public void Can_Queue_And_Deque_All_Elements_SingleThread()
        {
            BlockingQueue<string> q = new BlockingQueue<string>();
            for(int i=0;i<QueueItems;i++)
            {
                q.Enqueue(i.ToString());
            }

            for(int i=0;i<QueueItems;i++)
            {
                string k = q.Dequeue();
                Assert.AreEqual(i.ToString(), k, "Got wrong order of queue items");
            }

            q.Enqueue(null);

            q.Dequeue();
            q.Dequeue();
            q.Dequeue();
        }

        [Test]
        public void All_Items_Must_Be_Dequeued_When_Dequeue_On_Different_Threads_After_Queue_Has_Been_Filled()
        {
            BlockingQueue<string> q = new BlockingQueue<string>();
            for (int i = 0; i < QueueItems; i++)
            {
                q.Enqueue(i.ToString());
            }

            int dequeueCount = 0;
            Action dequeuer = () =>
                {
                    while (true)
                    {
                        string item = q.Dequeue();

                        if (item == null)
                        {
                            // last element reached
                            break;
                        }

                        Interlocked.Increment(ref dequeueCount);
                    }
                };

            

            for (int i = 0; i < 5; i++)
            {
                dequeuer.BeginInvoke(null, null);
            }

            Thread.Sleep(100);
            q.ReleaseWaiters();
            q.WaitUntilEmpty();

            Assert.AreEqual(QueueItems, dequeueCount, "Imbalanced queue/deque count");
        }

        [Test]
        public void Simulatanous_Queueing_Enqueuing_Must_Not_Skip_Elements()
        {
            BlockingQueue<string> q = new BlockingQueue<string>();

            int dequeueCount = 0;
            int exitCount = 0;
            Action dequeuer = () =>
            {
                while (true)
                {
                    string item = q.Dequeue();

                    if (item == null)
                    {
                        // last element reached
                        Interlocked.Increment(ref exitCount);
                        break;
                    }

                    Interlocked.Increment(ref dequeueCount);
                }
            };

            const int Threads = 10;

            for (int i = 0; i < Threads; i++)
            {
                dequeuer.BeginInvoke(null, null);
            }

            // wait until some threads are up and running
            Thread.Sleep(300);

            for (int i = 0; i < QueueItems; i++)
            {
                q.Enqueue(i.ToString());
            }

            q.ReleaseWaiters();
            q.WaitUntilEmpty();
            Thread.Sleep(30);
            Assert.AreEqual(QueueItems, dequeueCount, "Number of Enqueue and Deque calls must be the same");
            Thread.Sleep(200);
            Assert.AreEqual(Threads, exitCount, "All Threads should have exited by now");
        }
    }
}