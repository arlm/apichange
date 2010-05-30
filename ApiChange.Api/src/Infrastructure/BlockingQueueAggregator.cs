
using System.Collections.Generic;
using System;

namespace ApiChange.Infrastructure
{
    /// <summary>
    /// Aggregate one or a list of queues for processing with WorkItemDispatcher. The queues
    /// are processed in the order of addition. Only when the first queue is empty the next 
    /// queue is used until no more queues are left.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BlockingQueueAggregator<T> where T : class
    {
        Queue<BlockingQueue<T>> myQueues = new Queue<BlockingQueue<T>>();
        BlockingQueue<T> myCurrent;

        public BlockingQueueAggregator(BlockingQueue<T> queue)
        {
            if (queue == null)
            {
                throw new ArgumentNullException("queue");
            }

            myQueues.Enqueue(queue);
        }

        public BlockingQueueAggregator(IEnumerable<BlockingQueue<T>> queues)
        {
            if (queues == null)
            {
                throw new ArgumentNullException("queues");
            }

            foreach (BlockingQueue<T> queue in queues)
            {
                myQueues.Enqueue(queue);
            }
        }

        internal T Dequeue()
        {
            T lret = null;

            lock (myQueues)
            {
            TryNextQueue:
                if (myCurrent == null && myQueues.Count > 0)
                    myCurrent = myQueues.Dequeue();

                // No more queues available
                if (myCurrent == null)
                    return lret;

                // get next element or null if no more elements are in the queue
                lret = myCurrent.Dequeue();
                if (lret == null)
                {
                    myCurrent = null;
                    goto TryNextQueue;
                }
            }

            return lret;
        }
    }
}
