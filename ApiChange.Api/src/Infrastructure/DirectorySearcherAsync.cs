
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.ConstrainedExecution;
using System.Security.Permissions;
using Microsoft.Win32.SafeHandles;
using System.Threading;
using System.Diagnostics;

namespace ApiChange.Infrastructure
{
    public class DirectorySearcherAsync : IEnumerable<string>
    {
        static TypeHashes myType = new TypeHashes(typeof(DirectorySearcherAsync));

        string mySearchPath;
        string mySearchPattern;
        SearchOption mySearchOption;
        internal List<string> myFiles = new List<string>();
        internal List<BlockingQueue<string>> myListeningQueues = new List<BlockingQueue<string>>();
        ManualResetEvent myHasFileEvent = new ManualResetEvent(false);
        ManualResetEvent myHasFinishedEvent = new ManualResetEvent(false);


        enum SearchState
        {
            NotStartedYet,
            Running,
            Finished
        }

        volatile SearchState mySearchState;


        public DirectorySearcherAsync(string searchPath)
            : this(searchPath, "*", SearchOption.TopDirectoryOnly)
        {
        }

        public DirectorySearcherAsync(string searchPath, string searchPattern)
            : this(searchPath, searchPattern, SearchOption.TopDirectoryOnly)
        {
        }

        public DirectorySearcherAsync(string searchPath, string searchPattern, SearchOption searchOption)
        {
            if (String.IsNullOrEmpty(searchPath))
            {
                throw new ArgumentNullException("searchPath");
            }

            if(!Directory.Exists(searchPath))
            {
                throw new DirectoryNotFoundException(
                    String.Format("The search path does not exist: {0}", searchPath));
            }

            if(string.IsNullOrEmpty(searchPattern))
            {
                throw new ArgumentNullException("searchPattern");
            }

            mySearchPath = Path.GetFullPath(searchPath);
            Tracer.Info(Level.L1, myType, "DirectorySearcherAsync", "Search in path {0} for {1}", mySearchPath, searchPattern);

            mySearchPattern = searchPattern;
            mySearchOption = searchOption;
        }

        void FindFileThread()
        {
            using (Tracer t = new Tracer(Level.L3, myType, "FindFileThread"))
            {
                try
                {
                    try
                    {
                        bool bHasSetFirstFileEvent = false;
                        FileEnumerator fileenum = new FileEnumerator(mySearchPath, mySearchPattern, mySearchOption);
                        while (fileenum.MoveNext())
                        {
                            lock (this)
                            {
                                myFiles.Add(fileenum.Current);
                                t.Info("Found file {0}, Listening Queue Count: {1}", fileenum.Current, myListeningQueues.Count);

                                foreach (BlockingQueue<string> listeningQueue in myListeningQueues)
                                {
                                    listeningQueue.Enqueue(fileenum.Current);
                                }

                                if (!bHasSetFirstFileEvent)
                                {
                                    myHasFileEvent.Set();
                                    bHasSetFirstFileEvent = true;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        t.Error(ex, "Got error while searching in {0} for pattern {1}.", mySearchPath, mySearchPattern);
                    }
                }
                finally
                {
                    lock (this)
                    {
                        mySearchState = SearchState.Finished;
                        t.Info("Finished searching, Listening Queue count: {0}", myListeningQueues.Count);
                        myHasFinishedEvent.Set();

                        foreach (BlockingQueue<string> listeningQueue in myListeningQueues)
                        {
                            listeningQueue.ReleaseWaiters();
                        }
                        myListeningQueues.Clear();
                    }
                }
            }
        }


        public bool HasMatchingFiles
        {
            get
            {
                BeginSearch();
                int idx = WaitHandle.WaitAny(new WaitHandle[] { myHasFileEvent, myHasFinishedEvent });
                return idx == 0;
            }
        }

        public void BeginSearch()
        {
            lock (this)
            {
                if (mySearchState != SearchState.NotStartedYet)
                {
                    return;
                }

                mySearchState = SearchState.Running;
            }

            Tracer.Info(Level.L3, myType, "BeginSearch", "Called Searcher.BeginInvoke");
            Action searcher = FindFileThread;
            searcher.BeginInvoke(null, null);
        }

        public IEnumerator<string> GetEnumerator()
        {
            BeginSearch();
            return new DirectorySearcherAsyncEnumerator(this);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            BeginSearch();
            return new DirectorySearcherAsyncEnumerator(this);
        }

        public BlockingQueue<string> GetResultQueue()
        {
            using (Tracer t = new Tracer(Level.L3, myType, "GetResultQueue"))
            {
                BeginSearch();
                BlockingQueue<string> queue = new BlockingQueue<string>();
                lock (this)
                {
                    foreach (string foundFile in myFiles)
                    {
                        queue.Enqueue(foundFile);
                    }

                    t.Info("Queue did get {0} files we found so far. Current State: {1}, Path: {2}", myFiles.Count, mySearchState, mySearchPath);

                    // If we still expect files register our queue to get notified
                    // when something new has arrived.
                    if (!myHasFinishedEvent.WaitOne(0))
                    {
                        t.Info("Search is still running. Add it as listener for other found files.");
                        myListeningQueues.Add(queue);
                    }
                    else
                    {
                        // Add final item to queue to signal that no more items will 
                        // be received.
                        queue.ReleaseWaiters();
                        t.Info("Closed queue because no more files are expected");
                    }
                }

                return queue;
            }
        }

        class DirectorySearcherAsyncEnumerator : IEnumerator<string>, IDisposable
        {
            DirectorySearcherAsync mySearcher;
            BlockingQueue<string> myResultQueue;
            string myCurrentFile;

            public DirectorySearcherAsyncEnumerator(DirectorySearcherAsync searcher)
            {
                if (searcher == null)
                    throw new ArgumentNullException("searcher");

                mySearcher = searcher;

                mySearcher.BeginSearch();
                myResultQueue = mySearcher.GetResultQueue();
            }

            #region IEnumerator<string> Members

            public string Current
            {
                get
                {
                    return myCurrentFile;
                }
            }

            object System.Collections.IEnumerator.Current
            {
                get { return myCurrentFile; }
            }

            public bool MoveNext()
            {
                bool lret = true;
                myCurrentFile = myResultQueue.Dequeue();
                if (myCurrentFile == null)
                {
                    lret = false;
                }
                return lret;
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }
            #endregion

             #region IDisposable Members

            public void Dispose()
            {
                mySearcher = null;
                myCurrentFile = null;
                myResultQueue = null;
            }

            #endregion

        }
    }


    /// <summary>
    /// Contains information about the file that is found 
    /// by the FindFirstFile or FindNextFile functions.
    /// </summary>
    [Serializable, StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto), BestFitMapping(false)]
    internal class WIN32_FIND_DATA
    {
        public FileAttributes dwFileAttributes;
        public uint ftCreationTime_dwLowDateTime;
        public uint ftCreationTime_dwHighDateTime;
        public uint ftLastAccessTime_dwLowDateTime;
        public uint ftLastAccessTime_dwHighDateTime;
        public uint ftLastWriteTime_dwLowDateTime;
        public uint ftLastWriteTime_dwHighDateTime;
        public uint nFileSizeHigh;
        public uint nFileSizeLow;
        public int dwReserved0;
        public int dwReserved1;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string cFileName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
        public string cAlternateFileName;

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            return "File name=" + cFileName;
        }
    }

    /// <summary>
    /// Wraps a FindFirstFile handle.
    /// </summary>
    sealed class SafeFindHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [DllImport("kernel32.dll")]
        private static extern bool FindClose(IntPtr handle);

        /// <summary>
        /// Initializes a new instance of the <see cref="SafeFindHandle"/> class.
        /// </summary>
        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        internal SafeFindHandle()
            : base(true)
        {
        }

        /// <summary>
        /// When overridden in a derived class, executes the code required to free the handle.
        /// </summary>
        /// <returns>
        /// true if the handle is released successfully; otherwise, in the 
        /// event of a catastrophic failure, false. In this case, it 
        /// generates a releaseHandleFailed MDA Managed Debugging Assistant.
        /// </returns>
        protected override bool ReleaseHandle()
        {
            return FindClose(base.handle);
        }
    }

          /// <summary>
        /// Provides the implementation of the 
        /// <see cref="T:System.Collections.Generic.IEnumerator`1"/> interface
        /// </summary>
        [System.Security.SuppressUnmanagedCodeSecurity]
        class FileEnumerator : IEnumerator<string>
        {
            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern SafeFindHandle FindFirstFile(string fileName, 
                [In, Out] WIN32_FIND_DATA data);

            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern bool FindNextFile(SafeFindHandle hndFindFile, 
                    [In, Out, MarshalAs(UnmanagedType.LPStruct)] WIN32_FIND_DATA lpFindFileData);

            /// <summary>
            /// Hold context information about where we current are in the directory search.
            /// </summary>
            private class SearchContext
            {
                public readonly string Path;
                public Stack<string> SubdirectoriesToProcess;

                public SearchContext(string path)
                {
                    this.Path = path;
                }
            }

            private string m_path;
            private string m_filter;
            private SearchOption m_searchOption;
            private Stack<SearchContext> m_contextStack;
            private SearchContext m_currentContext;

            private SafeFindHandle m_hndFindFile;
            private WIN32_FIND_DATA m_win_find_data = new WIN32_FIND_DATA();

            /// <summary>
            /// Initializes a new instance of the <see cref="FileEnumerator"/> class.
            /// </summary>
            /// <param name="path">The path to search.</param>
            /// <param name="filter">The search string to match against files in the path.</param>
            /// <param name="searchOption">
            /// One of the SearchOption values that specifies whether the search 
            /// operation should include all subdirectories or only the current directory.
            /// </param>
            public FileEnumerator(string path, string filter, SearchOption searchOption)
            {
                m_path = path;
                m_filter = filter;
                m_searchOption = searchOption;
                m_currentContext = new SearchContext(path);
                
                if (m_searchOption == SearchOption.AllDirectories)
                {
                    m_contextStack = new Stack<SearchContext>();
                }
            }

            #region IEnumerator<FileData> Members

            /// <summary>
            /// Gets the element in the collection at the current position of the enumerator.
            /// </summary>
            /// <value></value>
            /// <returns>
            /// The element in the collection at the current position of the enumerator.
            /// </returns>
            public string Current
            {
                get { return Path.Combine(m_path, m_win_find_data.cFileName); }
            }

            #endregion

            #region IDisposable Members

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, 
            /// or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                if (m_hndFindFile != null)
                {
                    m_hndFindFile.Dispose();
                }
            }

            #endregion

            #region IEnumerator Members

            /// <summary>
            /// Gets the element in the collection at the current position of the enumerator.
            /// </summary>
            /// <value></value>
            /// <returns>
            /// The element in the collection at the current position of the enumerator.
            /// </returns>
            object System.Collections.IEnumerator.Current
            {
                get { return m_path; }
            }

            /// <summary>
            /// Advances the enumerator to the next element of the collection.
            /// </summary>
            /// <returns>
            /// true if the enumerator was successfully advanced to the next element; 
            /// false if the enumerator has passed the end of the collection.
            /// </returns>
            /// <exception cref="T:System.InvalidOperationException">
            /// The collection was modified after the enumerator was created.
            /// </exception>
            public bool MoveNext()
            {
                bool retval = false;

                //If the handle is null, this is first call to MoveNext in the current 
                // directory.  In that case, start a new search.
                if (m_currentContext.SubdirectoriesToProcess == null)
                {
                    if (m_hndFindFile == null)
                    {
                        new FileIOPermission(FileIOPermissionAccess.PathDiscovery, m_path).Demand();

                        string searchPath = Path.Combine(m_path, m_filter);
                        m_hndFindFile = FindFirstFile(searchPath, m_win_find_data);
                        retval = !m_hndFindFile.IsInvalid;
                    }
                    else
                    {
                        //Otherwise, find the next item.
                        retval = FindNextFile(m_hndFindFile, m_win_find_data);
                    }
                }

                //If the call to FindNextFile or FindFirstFile succeeded...
                if (retval)
                {
                    if (((FileAttributes)m_win_find_data.dwFileAttributes & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        //Ignore folders for now.   We call MoveNext recursively here to 
                        // move to the next item that FindNextFile will return.
                        return MoveNext();
                    }
                }
                else if (m_searchOption == SearchOption.AllDirectories)
                {
                    //SearchContext context = new SearchContext(m_hndFindFile, m_path);
                    //m_contextStack.Push(context);
                    //m_path = Path.Combine(m_path, m_win_find_data.cFileName);
                    //m_hndFindFile = null;

                    if (m_currentContext.SubdirectoriesToProcess == null)
                    {
                        string[] subDirectories = Directory.GetDirectories(m_path);
                        m_currentContext.SubdirectoriesToProcess = new Stack<string>(subDirectories);
                    }

                    if (m_currentContext.SubdirectoriesToProcess.Count > 0)
                    {
                        string subDir = m_currentContext.SubdirectoriesToProcess.Pop();

                        m_contextStack.Push(m_currentContext);
                        m_path = subDir;
                        m_hndFindFile = null;
                        m_currentContext = new SearchContext(m_path);
                        return MoveNext();
                    }

                    //If there are no more files in this directory and we are 
                    // in a sub directory, pop back up to the parent directory and
                    // continue the search from there.
                    if (m_contextStack.Count > 0)
                    {
                        m_currentContext = m_contextStack.Pop();
                        m_path = m_currentContext.Path;
                        if (m_hndFindFile != null)
                        {
                            m_hndFindFile.Close();
                            m_hndFindFile = null;
                        }

                        return MoveNext();
                    }
                }

                return retval;
            }

            /// <summary>
            /// Sets the enumerator to its initial position, which is before the first element in the collection.
            /// </summary>
            /// <exception cref="T:System.InvalidOperationException">
            /// The collection was modified after the enumerator was created.
            /// </exception>
            public void Reset()
            {
                m_hndFindFile = null;
            }

            #endregion
        }
}