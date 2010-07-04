
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Reflection;
using System.Diagnostics;

namespace ApiChange.Infrastructure
{
    /// <summary>
    /// Distribute units of work to n threads which read its data from
    /// a blocking queue and process it is parallel.
    /// </summary>
    /// <typeparam name="T">Workitem type</typeparam>
    public class WorkItemDispatcher<T> : IDisposable where T : class
    {
        static TypeHashes myType = new TypeHashes(typeof(WorkItemDispatcher<T>));

        WaitHandle[] myExitEvents;
        List<Exception> myExceptions = new List<Exception>();
        WorkItemDispatcherData<T> myData;
        int myRunningCount = 0;
        bool myCancel = false;
        const string DefaultName = "Workitem Dispatcher";
        bool myIsDisposed = false;

        /// <summary>
        /// Instance name of the dispatcher
        /// </summary>
        public string Name
        {
            get { return myData.Name; }
        }

        WorkItemOptions ConvertFromDefault(WorkItemOptions option)
        {
            return option == WorkItemOptions.Default ? WorkItemOptions.ExitOnFirstEror : option;
        }

        void SetName(string name)
        {
            myData.Name = String.IsNullOrEmpty(name) ? DefaultName : name;
        }

        /// <summary>
        /// Create n-threads from the IO thread pool and process the data in the processor delegate which is fetched
        /// from the work queue.
        /// </summary>
        /// <param name="width">Number of IO threads to start in parallel.</param>
        /// <param name="processor">Delegate which is called in width threads to process work.</param>
        /// <param name="work">Input queue which contains the work items for all threads.</param>
        public WorkItemDispatcher(int width, Action<T> processor, BlockingQueue<T> work) :
            this(new WorkItemDispatcherData<T>() { Width = width, Processor = processor, InputData = work})
        {
        }

        /// <summary>
        /// Create n-threads from the IO thread pool and process the data in the processor delegate which is fetched
        /// from the work queue.
        /// </summary>
        /// <param name="width">Number of IO threads to start in parallel.</param>
        /// <param name="processor">Delegate which is called in width threads to process work.</param>
        /// <param name="work">Input queue which contains the work items for all threads.</param>
        /// <param name="options">Exception handling options.</param>
        public WorkItemDispatcher(int width, Action<T> processor, BlockingQueue<T> work, WorkItemOptions options):
            this(new WorkItemDispatcherData<T>() { Width = width, Processor = processor, InputData = work, Options = options })
        {
        }

        /// <summary>
        /// Create n-threads from the IO thread pool and process the data in the processor delegate which is fetched
        /// from the work queue.
        /// </summary>
        /// <param name="width">Number of IO threads to start in parallel.</param>
        /// <param name="processor">Delegate which is called in width threads to process work.</param>
        /// <param name="workList">List of blocking queues. The input queues are processed in the order they were added.</param>
        /// <param name="options">Exception handling options.</param>
        public WorkItemDispatcher(int width, Action<T> processor, BlockingQueueAggregator<T> workList, WorkItemOptions options) :
            this(width, processor, null, workList, options)
        {
        }

        /// <summary>
        /// Create n-threads from the IO thread pool and process the data in the processor delegate which is fetched
        /// from the work queue.
        /// </summary>
        /// <param name="width">Number of IO threads to start in parallel.</param>
        /// <param name="processor">Delegate which is called in width threads to process work.</param>
        /// <param name="name">Instance Name of the Dispatcher.</param>
        /// <param name="workList">List of blocking queues. The input queues are processed in the order they were added.</param>
        /// <param name="options">Exception handling options.</param>
        public WorkItemDispatcher(int width, Action<T> processor, string name, BlockingQueueAggregator<T> workList, WorkItemOptions options) :
            this(new WorkItemDispatcherData<T>() {Width = width, Processor =processor, Name = name, InputDataList = workList, Options = options})
        {
        }

                /// <summary>
        /// Create n-threads from the IO thread pool and process the data in the processor delegate which is fetched
        /// from the work queue.
        /// </summary>
        /// <param name="width">Number of IO threads to start in parallel.</param>
        /// <param name="processor">Delegate which is called in width threads to process work.</param>
        /// <param name="name">Instance Name of the Dispatcher.</param>
        /// <param name="work">Input blocking queue from which work is fetched for the worker threads.</param>
        /// <param name="options">Exception handling options.</param>
        public WorkItemDispatcher(int width, Action<T> processor, string name, BlockingQueue<T> work, WorkItemOptions options) :
            this(new WorkItemDispatcherData<T>() { Width = width, Processor = processor, Name = name, InputData = work, Options = options })
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkItemDispatcher&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="width">Number of IO threads to start in parallel.</param>
        /// <param name="processor">Delegate which is called in width threads to process work.</param>
        /// <param name="name">Instance Name of the Dispatcher.</param>
        /// <param name="workList">List of blocking queues. The input queues are processed in the order they were added.</param>
        public WorkItemDispatcher(int width, Action<T> processor, string name, BlockingQueueAggregator<T> workList) :
            this(width, processor, name, workList, WorkItemOptions.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkItemDispatcher&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="width">Number of IO threads to start in parallel.</param>
        /// <param name="processor">Delegate which is called in width threads to process work.</param>
        /// <param name="name">Instance Name of the Dispatcher.</param>
        /// <param name="work">Input blocking queue from which work is fetched for the worker threads.</param>
        public WorkItemDispatcher(int width, Action<T> processor, string name, BlockingQueue<T> work) :
            this(width, processor, name, work, WorkItemOptions.Default)
        {
        }

        /// <summary>
        /// Create n-threads from the IO thread pool and process the data in the processor delegate which is fetched
        /// from the work queue.
        /// </summary>
        /// <param name="data">Class which configures the dispatcher.</param>
        public WorkItemDispatcher(WorkItemDispatcherData<T> data)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            if( data.Processor == null )
                throw new ArgumentNullException("data.Processor");

            if (data.InputDataList == null)
            {
                throw new ArgumentNullException("data.InputDataList was null. No work given to dispatcher");
            }

            if( data.Width < 1 )
            {
                throw new ArgumentOutOfRangeException("The Width (number of concurrent threads) must be > 0");
            }

            myData = data;
            SetName(data.Name);
            myData.Options = ConvertFromDefault(data.Options);
            myExitEvents = new WaitHandle[myData.Width];
            myRunningCount = myData.Width;

            Action processWork = ProcessWork;
            for (int i = 0; i < myData.Width; i++)
            {
                IAsyncResult res = processWork.BeginInvoke(Completed, null);
                myExitEvents[i] = res.AsyncWaitHandle;
            }
        }

        void Completed(IAsyncResult res)
        {
            using (Tracer t = new Tracer(myType, "Completed"))
            {
                Interlocked.Decrement(ref myRunningCount);
                t.Info("Current running count: {0}", myRunningCount);

                if (myRunningCount == 0 && myData.OnCompleted != null)
                {
                    // Fire completion event when all threads are done
                    this.myData.OnCompleted(CreateException());
                }
            }
        }



        AggregateException CreateException()
        {
            if( myExceptions.Count == 0 )
                return null;

            return new AggregateException("One or more worker thread exceptions did occur.", myExceptions);
        }

        void ThrowWorkerException(bool bThrowBackToEnqueuer)
        {
            lock (myExceptions)
            {
                if (myExceptions.Count > 0)
                {
                    if (IsEnabled(WorkItemOptions.ExitOnFirstEror) || !bThrowBackToEnqueuer)
                    {
                        throw CreateException();
                    }
                }
            }
        }

        T GetNextWorkItem()
        {
            return myData.InputDataList.Dequeue();
        }

        bool IsEnabled(WorkItemOptions option)
        {
            return ((myData.Options & option) == option);
        }


        /// <summary>
        /// Worker thread which fetches work from the queue and processes it.
        /// </summary>
        void ProcessWork()
        {
            using (var tr = new Tracer(myType, "ProcessWork"))
            {
                try
                {
                    while (true)
                    {
                        if (myCancel == true)
                            break;

                        T work = GetNextWorkItem();
                        if (work == null || (myExceptions.Count > 0 && IsEnabled(WorkItemOptions.ExitOnFirstEror)))
                        {
                            // No more work present or an error has happened
                            break;
                        }

                        try
                        {
                            // Do some work with work item.
                            myData.Processor(work);
                        }
                        catch (Exception ex)
                        {
                            tr.Error(Level.L2, ex, "Got exception in worker thread {0} while processing work item {1}", this.Name, work);
                            if (IsEnabled(WorkItemOptions.AggregateExceptions))
                            {
                                tr.Info("Aggregating Exception");
                                lock (myExceptions)
                                {
                                    myExceptions.Add(ex);
                                }
                            }
                            else
                            {
                                throw;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    tr.Error(Level.L1, ex, "Worker thread {0} was interrupted by exception of worker thread", this.Name);
                    lock (myExceptions)
                    {
                        myExceptions.Add(ex);
                    }
                }
            }
        }

        /// <summary>
        /// Cancel all pending threads. When all threads have finished processing their current work item 
        /// the dispose call will unblock. The OnCompleted delegate from the WorkitemDispatcherData class will
        /// be called as usual.
        /// </summary>
        public void Cancel()
        {
            using (Tracer t = new Tracer(myType, "Cancel"))
            {
                myCancel = true;
            }
        }

        /// <summary>
        /// Cancel all pending threads and wait when they have finished processing the current work item.
        /// </summary>
        public void CancelAndWait()
        {
            using (Tracer t = new Tracer(myType, "Cancel"))
            {
                Cancel();
                WaitUntilFinished();
            }
        }

        /// <summary>
        /// Wait until all worker threads have processed all pending data and no more input data is available
        /// in the input queues. An alternative is to use the OnCompleted callback in the <see cref="T:WorkItemDispatcherData"/> class to get a notification
        /// 
        /// </summary>
        public void Dispose()
        {
            using (Tracer t = new Tracer(myType, "Dispose"))
            {
                if (!myIsDisposed)
                {
                    myIsDisposed = true;

                    t.Info("Wait until all workers have finished");
                    WaitUntilFinished();
                    t.Info("All worker have finished");

                    foreach (WaitHandle wait in myExitEvents)
                    {
                        wait.Close();
                    }

                    // Throw excepton only if we are not already in exception processing
                    // to prevent masking the orginal excepton when the dispose is triggered
                    // via a using statement which is exited via an exception.
                    if (myExceptions.Count > 0 && !ExceptionHelper.InException)
                    {
                        ThrowWorkerException(false);
                    }
                }
            }
        }


        /// <summary>
        /// Wait until all work has been processed by all worker threads.
        /// </summary>
        void WaitUntilFinished()
        {
            // Windows does support wait only for a single WaitHandle. To work around this
            // we wait on another thread for our handles.
            if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
            {
                Action acc = () => WaitHandle.WaitAll(myExitEvents);
                acc.EndInvoke(acc.BeginInvoke(null, null));
            }
            else
            {
                WaitHandle.WaitAll(myExitEvents);
            }
        }
    }
}
