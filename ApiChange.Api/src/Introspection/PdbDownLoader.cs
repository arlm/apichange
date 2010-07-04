
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Threading;
using ApiChange.Infrastructure;

namespace ApiChange.Api.Introspection
{
    class PdbDownLoader
    {
        static TypeHashes myType = new TypeHashes(typeof(PdbDownLoader));

        int myDownLoadThreadCount;

        public int SucceededPdbCount
        {
            get;
            private set;
        }

        public List<string> FailedPdbs
        {
            get;
            set; 
        }

        ISymChkExecutor myExecutor;

        internal ISymChkExecutor Executor
        {
            get 
            {
                lock (this)
                {
                    if (myExecutor == null)
                    {
                        myExecutor = new SymChkExecutor();
                    }
                    return myExecutor;
                }
            }

            set
            {
                myExecutor = value;
            }
        }


        public PdbDownLoader():this(1)
        {
        }

        /// <summary>
        /// Patch after the pdb was downloaded the drive letter to match the pdb files with the source files
        /// </summary>
        /// <param name="downLoadThreadCount">Down load thread count.</param>
        public PdbDownLoader(int downLoadThreadCount)
        {
            using (Tracer t = new Tracer(myType, "PdbDownLoader"))
            {
                FailedPdbs = new List<string>();
                if (downLoadThreadCount <= 0)
                {
                    throw new ArgumentException("The download thread count cannot be <= 0");
                }

                myDownLoadThreadCount = downLoadThreadCount;
                t.Info("Download thread count is {0}", myDownLoadThreadCount);
            }
        }


        void DeleteOldPdb(string binaryName)
        {
            using (Tracer t = new Tracer(Level.L5,myType, "DeleteOldPdb"))
            {
                string pdbFile = GetPdbNameFromBinaryName(binaryName);
                t.Info("Try to delete pdb {0} for binary {1}", pdbFile, binaryName);
                try
                {
                    File.Delete(pdbFile);
                }
                catch (FileNotFoundException ex)
                {
                    t.Error(ex, "No old pdb file did exist");
                }
                catch (Exception ex)
                {
                    t.Error(ex, "Could not delete old pdb file");
                }
            }
        }

        public bool DownloadPdbs(FileQuery query, string symbolServer)
        {
            return DownloadPdbs(query, symbolServer, null);
        }

        /// <summary>
        /// Downloads the pdbs from the symbol server.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="symbolServer">The symbol server name.</param>
        /// <param name="downloadDir">The download directory. Can be null.</param>
        /// <returns>
        /// true if all symbols could be downloaded. False otherwise.
        /// </returns>
        public bool DownloadPdbs(FileQuery query, string symbolServer, string downloadDir)
        {
            using (Tracer t = new Tracer(myType, "DownloadPdbs"))
            {
                bool lret = SymChkExecutor.bCanStartSymChk;
                int currentFailCount = FailedPdbs.Count;

                BlockingQueue<string> fileQueue = query.EnumerateFiles;
                BlockingQueueAggregator<string> aggregator = new BlockingQueueAggregator<string>(fileQueue);

                Action<string> downLoadPdbThread = (string fileName) =>
                    {
                        string pdbFileName = GetPdbNameFromBinaryName(fileName);

                        // delete old pdb to ensure that the new matching pdb is really downloaded. Symchk does not replace existing but not matching pdbs.
                        try { File.Delete(pdbFileName); }
                        catch { }

                        if (!this.Executor.DownLoadPdb(fileName, symbolServer, downloadDir))
                        {
                            lock (FailedPdbs)
                            {
                                FailedPdbs.Add(Path.GetFileName(fileName));
                            }
                        }
                        else
                        {
                            lock (FailedPdbs)
                            {
                                SucceededPdbCount++;
                            }
                        }
                    };

                var dispatcher = new WorkItemDispatcher<string>(myDownLoadThreadCount, 
                    downLoadPdbThread,
                    "Pdb Downloader", 
                    aggregator, 
                    WorkItemOptions.AggregateExceptions);

                try
                {
                    dispatcher.Dispose();
                }
                catch (AggregateException ex)
                {
                    t.Error(ex, "Got error during pdb download");
                    lret = false;
                }

                if (FailedPdbs.Count > currentFailCount)
                {
                    t.Warning("The failed pdb count has increased by {0}", FailedPdbs.Count - currentFailCount);
                    lret = false;
                }

                return lret;
            }
        }

        internal static string GetPdbNameFromBinaryName(string binaryFileName)
        {
            return Path.Combine(Path.GetDirectoryName(binaryFileName), Path.GetFileNameWithoutExtension(binaryFileName) + ".pdb");
        }

        
    }
}
