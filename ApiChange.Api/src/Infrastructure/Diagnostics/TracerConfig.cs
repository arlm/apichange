
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace ApiChange.Infrastructure
{

    /// <summary>
    /// Main Class to configure trace output devices. The default instance is basically a null device.
    /// </summary>
    public class TracerConfig : IDisposable
    {
        internal static TracerConfig Instance = new TracerConfig(Environment.GetEnvironmentVariable(TraceEnvVarName));
        TraceListenerCollection myListeners;
        static object myLock = new object();

        TraceFilter myFilters = new TraceFilterMatchNone();
        TraceFilter myNotFilters = null;

        /// <summary>
        /// Environment variable which configures tracing.
        /// </summary>
        public const string TraceEnvVarName = "_Trace";

        static string myPid = Process.GetCurrentProcess().Id.ToString("D5");

        [ThreadStatic]
        static string ProcessAndThreadId;

        internal string PidAndTid
        {
            get
            {
                if (ProcessAndThreadId == null)
                {
                    ProcessAndThreadId = myPid + "/" + GetCurrentThreadId().ToString("D5");
                }

                return ProcessAndThreadId;
            }
        }

        internal static TraceListenerCollection Listeners
        {
            get
            {
                return Instance.myListeners;
            }
        }

        /// <summary>
        /// Re/Set trace configuration in a thread safe way by shutting down the already existing listeners and then 
        /// put the new config into place.
        /// </summary>
        /// <param name="cfg">The trace string format is of the form OutputDevice;TypeFilter MessageFilter; TypeFilter MessageFilter; ...</param>
        /// <param name="bClearEvents">if true all registered trace callbacks are removed.</param>
        /// <returns>The old trace configuration string.</returns>
        public static string Reset(string cfg, bool bClearEvents)
        {
            lock (myLock)
            {
                Instance.Dispose();
                string old = Environment.GetEnvironmentVariable(TraceEnvVarName);
                Environment.SetEnvironmentVariable(TraceEnvVarName, cfg);
                Instance = new TracerConfig(Environment.GetEnvironmentVariable(TraceEnvVarName));
                if (bClearEvents)
                {
                    Tracer.ClearEvents();
                }
                return old;
            }
        }

        /// <summary>
        /// Re/Set trace configuration in a thread safe way by shutting down the already existing listeners and then 
        /// put the new config into place.
        /// </summary>
        /// <param name="cfg">The trace string format is of the form OutputDevice;TypeFilter MessageFilter; TypeFilter MessageFilter; ...</param>
        /// <returns>The old trace configuration string.</returns>
        public static string Reset(string cfg)
        {
            return Reset(cfg, true);
        }


        internal bool IsEnabled(TypeHashes type, MessageTypes msgType, Level level)
        {
            if( myListeners == null || type == null)
            {
                return false;
            }

            bool lret = myFilters.IsMatch(type, msgType, level);
            if (myNotFilters != null && lret == true)
            {
                lret = myNotFilters.IsMatch(type, msgType, level);
            }

            return lret;
        }

        internal void WriteTraceMessage(string traceMsg)
        {
            foreach (TraceListener listener in TracerConfig.Listeners)
            {
                listener.Write(traceMsg);
                listener.Flush();
            }
        }

        internal TracerConfig(string cfg)
        {
            if (String.IsNullOrEmpty(cfg))
            {
                return;
            }

            TraceSource source = new TraceSource(TraceEnvVarName, SourceLevels.All);
            myListeners = source.Listeners;

            TraceCfgParser parser = new TraceCfgParser(cfg);
            TraceListener newListener = parser.OutDevice;
            this.myFilters = parser.Filters;
            this.myNotFilters = parser.NotFilters;

            if (newListener != null)
            {
                // when the App.config _Trace source should be used we do not replace
                // anything
                if (!parser.UseAppConfigListeners)
                {
                    myListeners.Clear();
                    myListeners.Add(newListener);
                }
            }
            else
            {
                myListeners = null;
            }
        }

        

        #region IDisposable Members

        /// <summary>
        /// Close the current active trace listeners in a thread safe way.
        /// </summary>
        public void Dispose()
        {
            lock (myLock)
            {
                // The shutdown protocol works like this
                // 1. Get all listeners into an array
                // 2. Clear the thread safe listeners collection
                // 3. Call flush and dispose on each listener to ensure that any pending messages are written.
                // This way we ensure that while we are shutting the listerns down no additional trace messages
                // arrive which could be used accidentally by tracing. Using a disposed listener is almost always a bad idea.
                if (myListeners != null && myListeners.Count > 0)
                {
                    TraceListener [] listeners = new TraceListener[myListeners.Count];
                    myListeners.CopyTo(listeners, 0);
                    myListeners.Clear();
                    foreach (var listener in listeners)
                    {
                        listener.Flush();
                        listener.Dispose();
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Get the current unmanaged Thread ID.
        /// </summary>
        /// <returns>Integer that identifies the current thread.</returns>
        [DllImport("kernel32.dll")]
        public static extern int GetCurrentThreadId();
    }
}
