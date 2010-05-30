
using System;
using System.Reflection;

namespace ApiChange.Infrastructure
{
    class AsyncWriter<T> : IDisposable where T : class
    {
        BlockingQueue<T> myQueue = new BlockingQueue<T>();
        IAsyncResult myResult = null;
        Action<T> myWriter = null;
        Exception myLastException = null;

        public AsyncWriter(Action<T> writer)
        {
            myWriter = writer;
            Action acc = ThreadWriter;
            myResult = acc.BeginInvoke(null, null);
        }

        public void Write(T item)
        {
            if (myLastException != null)
            {
                throw new TargetInvocationException("Got at least one exception from AsyncWriter", myLastException);
            }
            myQueue.Enqueue(item);
        }


        public void Close()
        {
            Dispose();
        }

        void ThreadWriter()
        {
            try
            {
                while (true)
                {
                    T item = myQueue.Dequeue();
                    myWriter(item);

                    if (item == null)
                        break;
                }
            }
            catch (Exception ex)
            {
                myLastException = ex;
            }
        }

        public void Dispose()
        {
            if (myResult != null)
            {
                myQueue.Enqueue(null);
                // Wait until worker thread has terminated 
                // normally or via exception
                myResult.AsyncWaitHandle.WaitOne();
                myResult = null;

                // Throw excepton only if we are not already in exception processing
                // to prevent masking the orginal excepton when the dispose is triggered
                // via a using statement which is exited via an exception.
                if (myLastException != null && !ExceptionHelper.InException)
                {
                    throw new TargetInvocationException("Got at least one exception from AsyncWriter", myLastException);
                }
            }
        }
   }
}
