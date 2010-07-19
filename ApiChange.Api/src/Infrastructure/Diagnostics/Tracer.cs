
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;

namespace ApiChange.Infrastructure
{
    /// <summary>
    /// High performance tracer class which enables method enter/leave tracing with duration and
    /// other features.
    /// </summary>
    public struct Tracer : IDisposable
    {
        string myMethod;
        TypeHashes myType;
        string myTypeMethodName;
        string TypeMethodName
        {
            get
            {
                if (myTypeMethodName == null)
                {
                    myTypeMethodName = GenerateTypeMethodName(myType, myMethod);
                }

                return myTypeMethodName;
            }
        }

        DateTime myEnterTime;
        Level myLevel;

        [ThreadStatic]
        static Exception myLastPrintedException;

        static string GenerateTypeMethodName(TypeHashes type, string method)
        {
            return type.FullQualifiedTypeName + "." + method;
        }

        const string MsgTypeInfo       = "<Information>";
        const string MsgTypeInstrument = "<Instrument >";
        const string MsgTypeWarning    = "<Warning    >";
        const string MsgTypeError      = "<Error      >";
        const string MsgTypeException  = "<Exception  >";
        const string MsgTypeIn         = "<{{         >";
        const string MsgTypeOut        = "<         }}<";

        public enum MsgType
        {
            None = 0,
            Information,
            Instrument,
            Warning,
            Error,
            Exception,
            In,
            Out
        }

        static Dictionary<string, MsgType> MsgStr2Type = new Dictionary<string, MsgType>
        {
            {MsgTypeInfo, MsgType.Information},
            {MsgTypeInstrument, MsgType.Instrument},
            {MsgTypeWarning, MsgType.Warning},
            {MsgTypeError, MsgType.Error},
            {MsgTypeException, MsgType.Exception},
            {MsgTypeIn, MsgType.In},
            {MsgTypeOut, MsgType.Out}
        };

        /// <summary>
        /// Create a new Tracer which traces method enter and leave (on Dispose)
        /// </summary>
        /// <param name="type">TypeHandle instance which identifies your class type. This instance should be a static instance of your type.</param>
        /// <param name="method">The method name of your current method.</param>
        public Tracer(TypeHashes type, string method):this(Level.L1, type, method)
        {
        }

        /// <summary>
        /// Create a new Tracer which traces method enter and leave (on Dispose)
        /// </summary>
        /// <param name="level">Trace Level. 1 is the high level overview, 5 is for high volume detailed traces.</param>
        /// <param name="type">TypeHandle instance which identifies your class type. This instance should be a static instance of your type.</param>
        /// <param name="method">The method name of your current method.</param>
        public Tracer(Level level, TypeHashes type, string method)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (method == null)
            {
                throw new ArgumentNullException("method");
            }

            myEnterTime = DateTime.MinValue;
            myMethod = method;
            myType = type;
            myLevel = level;
            myTypeMethodName = null;

            if (TracerConfig.Instance.IsEnabled(myType, MessageTypes.InOut, myLevel))
            {
                myEnterTime = DateTime.Now;
                TraceMsg(MsgTypeIn, this.TypeMethodName, myEnterTime, null, null);
            }
        }

        /// <summary>
        /// Execute the given callback when the current trace level and error severit filter let it pass.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns>true when the action callback was execute. false otherwise</returns>
        public bool ErrorExecute(Action action)
        {
            if (TracerConfig.Instance.IsEnabled(myType, MessageTypes.Error, myLevel))
            {
                action();
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Execute the given callback when the current trace level and warning severity filter let it pass.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns>true when the action callback was executed. false otherwise</returns>
        public bool WarningExecute(Action action)
        {
            if (TracerConfig.Instance.IsEnabled(myType, MessageTypes.Warning, myLevel))
            {
                action();
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Execute the given callback when the current trace level and info severity filter let it pass.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns>true when the action callback was executed. false otherwise</returns>
        public bool InfoExecute(Action action)
        {
            if (TracerConfig.Instance.IsEnabled(myType, MessageTypes.Info, myLevel))
            {
                action();
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Executes the callback when the given trace level is enabled. That allows when tracing is enabled
        /// complex string formatting only when needed.
        /// </summary>
        /// <param name="msgType">The message type which must match.</param>
        /// <param name="level">The trace level which must match.</param>
        /// <param name="type">TypeHandle of your class.</param>
        /// <param name="action">The delegate to execute when it does match.</param>
        /// <returns>true when the delegate was executed, false otherwise.</returns>
        public static bool Execute(MessageTypes msgType, Level level, TypeHashes type, Action action)
        {
            if (TracerConfig.Instance.IsEnabled(type, msgType, level))
            {
                action();
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Trace an info message.
        /// </summary>
        /// <param name="level">Used Trace Level</param>
        /// <param name="fmt">Trace message format string</param>
        /// <param name="args">Optional message format arguments.</param>
        public void Info(Level level, string fmt, params object[] args)
        {
            if (fmt == null)
            {
                throw new ArgumentNullException(fmt);
            }

            if (TracerConfig.Instance.IsEnabled(myType, MessageTypes.Info, level))
            {
                TraceMsg(MsgTypeInfo, this.TypeMethodName, DateTime.Now, fmt, args);
            }
        }

        /// <summary>
        /// Trace an info message with the trace level used during construction of the Tracer instance. If none default is Level 1.
        /// </summary>
        /// <param name="fmt">Trace message format string</param>
        /// <param name="args">Optional message format arguments.</param>
        public void Info(string fmt, params object[] args)
        {
            Info(myLevel, fmt, args);
        }


        /// <summary>
        ///  Trace an info message with a given level.
        /// </summary>
        /// <param name="level">Trace Level. 1 is the high level overview, 5 is for high volume detailed traces.</param>
        /// <param name="type">TypeHandle instance which identifies your class type. This instance should be a static instance of your type.</param>
        /// <param name="method">The method name of your current method.</param>
        /// <param name="fmt">Trace message format string</param>
        /// <param name="args">Optional message format arguments.</param>
        public static void Info(Level level, TypeHashes type, string method, string fmt, params object [] args)
        {
            if (fmt == null)
            {
                throw new ArgumentNullException(fmt);
            }
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            if (TracerConfig.Instance.IsEnabled(type, MessageTypes.Info, level))
            {
                TraceMsg(MsgTypeInfo, GenerateTypeMethodName(type, method), DateTime.Now, fmt, args);
            }
        }

        /// <summary>
        /// Trace an instrumentation message with a given level.
        /// </summary>
        /// <remarks>The method is only executed if the INSTRUMENT conditional compilation symbol is enabled in your project settings. Otherwise all calls will
        /// not be compiled into your binary. This level is useful for unit testing and custom fault injection.</remarks>
        /// <param name="level">Trace Level. 1 is the high level overview, 5 is for high volume detailed traces.</param>
        /// <param name="fmt">Trace message format string.</param>
        /// <param name="args">Optional message format arguments.</param>
        [Conditional("INSTRUMENT")]
        public void Instrument(Level level, string fmt, params object[] args)
        {
            if (fmt == null)
            {
                throw new ArgumentNullException("fmt");
            }

            if (TracerConfig.Instance.IsEnabled(myType, MessageTypes.Instrument, level))
            {
                TraceMsg(MsgTypeInstrument, this.TypeMethodName, DateTime.Now, fmt, args);
            }
        }

        /// <summary>
        /// Trace an instrumentation message with a given level.
        /// </summary>
        /// <remarks>The method is only executed if the INSTRUMENT conditional compilation symbol is enabled in your project settings. Otherwise all calls will
        /// not be compiled into your binary. This level is useful for unit testing and custom fault injection.</remarks>
        /// <param name="fmt">Trace message format string.</param>
        /// <param name="args">Optional message format arguments.</param>
        [Conditional("INSTRUMENT")]
        public void Instrument(string fmt, params object[] args)
        {
            Instrument(myLevel, fmt, args);
        }

        /// <summary>
        ///  Trace an instrumentation message with a given level.
        /// </summary>
        /// <remarks>The method is only executed if the INSTRUMENT conditional compilation symbol is enabled in your project settings. Otherwise all calls will
        /// not be compiled into your binary. This level is useful for unit testing and custom fault injection.</remarks>
        /// <param name="level">Trace Level. 1 is the high level overview, 5 is for high volume detailed traces.</param>
        /// <param name="type">TypeHandle instance which identifies your class type. This instance should be a static instance of your type.</param>
        /// <param name="method">The method name of your current method.</param>
        /// <param name="fmt">Trace message format string</param>
        /// <param name="args">Optional message format arguments.</param>
        [Conditional("INSTRUMENT")]
        public static void Instrument(Level level, TypeHashes type, string method, string fmt, params object[] args)
        {
            if (fmt == null)
            {
                throw new ArgumentNullException(fmt);
            }
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            if (TracerConfig.Instance.IsEnabled(type, MessageTypes.Instrument, level))
            {
                TraceMsg(MsgTypeInstrument, GenerateTypeMethodName(type, method), DateTime.Now, fmt, args);
            }
        }

        /// <summary>
        /// Write a warning trace to the configured output device.
        /// </summary>
        /// <param name="level">Trace Level. 1 is the high level overview, 5 is for high volume detailed traces.</param>
        /// <param name="fmt">Trace message format string</param>
        /// <param name="args">Optional message format arguments.</param>
        public void Warning(Level level, string fmt, params object[] args)
        {
            if (fmt == null)
            {
                throw new ArgumentNullException(fmt);
            }

            if (TracerConfig.Instance.IsEnabled(myType, MessageTypes.Warning, level))
            {
                TraceMsg(MsgTypeWarning, this.TypeMethodName, DateTime.Now, fmt, args);
            }
        }

        /// <summary>
        /// Write a warning trace to the configured output device.
        /// </summary>
        /// <param name="fmt">Trace message format string</param>
        /// <param name="args">Optional message format arguments.</param>
        public void Warning(string fmt, params object[] args)
        {
            Warning(myLevel, fmt, args);
        }


        /// <summary>
        /// Write a warning trace to the configured output device.
        /// </summary>
        /// <param name="level">Trace Level. 1 is the high level overview, 5 is for high volume detailed traces.</param>
        /// <param name="type">TypeHandle instance which identifies your class type. This instance should be a static instance of your type.</param>
        /// <param name="method">The method name of your current method.</param>
        /// <param name="fmt">Trace message format string</param>
        /// <param name="args">Optional message format arguments.</param>
        public static void Warning(Level level, TypeHashes type, string method, string fmt, params object[] args)
        {
            if (fmt == null)
            {
                throw new ArgumentNullException(fmt);
            }
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            if (TracerConfig.Instance.IsEnabled(type, MessageTypes.Warning, level))
            {
                TraceMsg(MsgTypeWarning, GenerateTypeMethodName(type, method), DateTime.Now, fmt, args);
            }
        }


        /// <summary>
        /// Write an error trace to the configured output device.
        /// </summary>
        /// <param name="level">Trace Level. 1 is the high level overview, 5 is for high volume detailed traces.</param>
        /// <param name="fmt">Trace message format string</param>
        /// <param name="args">Optional message format arguments.</param>
        public void Error(Level level, string fmt, params object[] args)
        {
            if (fmt == null)
                throw new ArgumentNullException(fmt);
            if (TracerConfig.Instance.IsEnabled(myType, MessageTypes.Error, level))
            {
                TraceMsg(MsgTypeError, this.TypeMethodName, DateTime.Now, fmt, args);
            }
        }

        /// <summary>
        /// Write an error trace to the configured output device.
        /// </summary>
        /// <param name="fmt">Trace message format string</param>
        /// <param name="args">Optional message format arguments.</param>
        public void Error(string fmt, params object[] args)
        {
            Error(myLevel, fmt, args);
        }


        /// <summary>
        /// Write an error trace to the configured output device.
        /// </summary>
        /// <param name="level">Trace Level. 1 is the high level overview, 5 is for high volume detailed traces.</param>
        /// <param name="type">TypeHandle instance which identifies your class type. This instance should be a static instance of your type.</param>
        /// <param name="method">The method name of your current method.</param>
        /// <param name="fmt">Trace message format string</param>
        /// <param name="args">Optional message format arguments.</param>
        public static void Error(Level level, TypeHashes type, string method, string fmt, params object[] args)
        {
            if (fmt == null)
            {
                throw new ArgumentNullException(fmt);
            }
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            if (TracerConfig.Instance.IsEnabled(type, MessageTypes.Error, level))
            {
                TraceMsg(MsgTypeError, GenerateTypeMethodName(type, method), DateTime.Now, fmt, args);
            }
        }


        /// <summary>
        /// Write an exception to the configured output device.
        /// </summary>
        /// <param name="ex">The exception to trace.</param>
        public void Error(Exception ex)
        {
            Error(myLevel, ex);
        }


        /// <summary>
        /// Write an exception to the configured output device.
        /// </summary>
        /// <param name="level">Trace Level. 1 is the high level overview, 5 is for high volume detailed traces.</param>
        /// <param name="ex">The exception to trace.</param>
        public void Error(Level level, Exception ex)
        {
            if (ex == null)
                throw new ArgumentNullException("ex");

            if (TracerConfig.Instance.IsEnabled(myType, MessageTypes.Error, level))
            {
                TraceMsg(MsgTypeError, this.TypeMethodName, DateTime.Now, "{0}", ex);
            }
        }

        /// <summary>
        /// Write an exception to the configured output device.
        /// </summary>
        /// <param name="level">Trace Level. 1 is the high level overview, 5 is for high volume detailed traces.</param>
        /// <param name="type">TypeHandle instance which identifies your class type. This instance should be a static instance of your type.</param>
        /// <param name="method">The method name of your current method.</param>
        /// <param name="ex">The excepton to trace.</param>
        public static void Error(Level level, TypeHashes type, string method, Exception ex)
        {
            if (ex == null)
                throw new ArgumentNullException("ex");

            if (TracerConfig.Instance.IsEnabled(type, MessageTypes.Error, level))
            {
                TraceMsg(MsgTypeError, GenerateTypeMethodName(type, method), DateTime.Now, "{0}", ex);
            }
        }

        /// <summary>
        /// Write an exception to the configured output device.
        /// </summary>
        /// <param name="ex">The exception to trace.</param>
        /// <param name="fmt">Message describing what the exception is about.</param>
        /// <param name="args">Optional format arguments for the description message string.</param>
        public void Error(Exception ex, string fmt, params object[] args)
        {
            Error(myLevel, ex, fmt, args);
        }

        /// <summary>
        /// Write an exception to the configured output device.
        /// </summary>
        /// <param name="level">Trace Level. 1 is the high level overview, 5 is for high volume detailed traces.</param>
        /// <param name="ex">The exception to trace.</param>
        /// <param name="fmt">Message describing what the exception is about.</param>
        /// <param name="args">Optional format arguments for the description message string.</param>
        public void Error(Level level, Exception ex, string fmt, params object[] args)
        {
            if( ex == null )
                throw new ArgumentNullException("ex");
            if( fmt == null )
                throw new ArgumentNullException("fmt");

            if (TracerConfig.Instance.IsEnabled(myType, MessageTypes.Error, level))
            {
                string traceMsg = FormatStringSafe(fmt, args);
                TraceMsg(MsgTypeError, this.TypeMethodName, DateTime.Now, "{0}{1}{2}", traceMsg, Environment.NewLine, ex);
            }
        }

        /// <summary>
        /// Write an exception to the configured output device.
        /// </summary>
        /// <param name="level">Trace Level. 1 is the high level overview, 5 is for high volume detailed traces.</param>
        /// <param name="type">TypeHandle instance which identifies your class type. This instance should be a static instance of your type.</param>
        /// <param name="method">The method name of your current method.</param>
        /// <param name="ex">The exception to trace.</param>
        /// <param name="fmt">Message describing what the exception is about.</param>
        /// <param name="args">Optional format arguments for the description message string.</param>
        public static void Error(Level level, TypeHashes type, string method, Exception ex, string fmt, params object[] args)
        {
            if (ex == null)
                throw new ArgumentNullException("ex");
            if (fmt == null)
                throw new ArgumentNullException("fmt");
            if (type == null)
                throw new ArgumentNullException("type");

            if (TracerConfig.Instance.IsEnabled(type, MessageTypes.Error, level))
            {
                string traceMsg = FormatStringSafe(fmt,args);
                TraceMsg(MsgTypeError, GenerateTypeMethodName(type, method), DateTime.Now, "{0}{1}{2}", traceMsg, Environment.NewLine, ex);
            }
        }

        /// <summary>
        /// Generate a leaving method trace. Normally called at the end of an using statement.
        /// </summary>
        /// <remarks>
        /// When the method is left with an exception and Exception tracing is enabled it will trace this
        /// exception.
        /// </remarks>
        public void Dispose()
        {
            if (TracerConfig.Instance.IsEnabled(myType, MessageTypes.Exception, myLevel))
            {
                // only print exception warning when we did not have an exception on the thread stack
                // when we did enter this method. Otherwise we would print a warning while we entered and left a method while
                // executing a catch handler altough in our called methods nothing has happened.
                Exception currentException = ExceptionHelper.CurrentException;
                if (currentException != null && Object.ReferenceEquals(myLastPrintedException, currentException) == false)
                {
                    myLastPrintedException = currentException;
                    TraceMsg(MsgTypeException, this.TypeMethodName, DateTime.Now, "Exception thrown: {0}", currentException);
                }
            }

            if (TracerConfig.Instance.IsEnabled(myType, MessageTypes.InOut, myLevel))
            {
                DateTime now = DateTime.Now;
                TraceMsg(MsgTypeOut, this.TypeMethodName, now, "Duration {0}", FormatDuration(now.Ticks - myEnterTime.Ticks));
            }
        }

        public delegate void TraceCallBack(MsgType msgType, string typeMethodName, DateTime time, string message);

        /// <summary>
        /// This callback is called whenever a trace is encountered which matches the current filter
        /// </summary>
        public static event TraceCallBack TraceEvent;

        static internal void ClearEvents()
        {
             TraceEvent = null;
        }

        private string FormatDuration(long duration)
        {
            // When tracing is reconfigured at runtime we might not 
            // have the enter time at hand which results in huge duration times
            // We mark them as N.a. to indicate that we have the enter time not recorded
            if (duration > 0xfffffffffL)
            {
                return "N.a.ms";
            }
            return String.Format("{0:N0}ms", duration / 10000);
        }

        static void TraceMsg(string msgTypeString, string typeMethodName, DateTime time, string fmt, params object[] args)
        {
            string traceMsg = FormatStringSafe(fmt, args);
            if (TraceEvent != null)
            {
                TraceEvent(MsgStr2Type[msgTypeString], typeMethodName, time, traceMsg);
            }

            traceMsg = String.Join(" ", new string[] { FormatTime(time), TracerConfig.Instance.PidAndTid, msgTypeString, typeMethodName, traceMsg, Environment.NewLine });
            TracerConfig.Instance.WriteTraceMessage(traceMsg);
        }

        static string FormatStringSafe(string fmt, params object[] args)
        {
            string ret = fmt;
            if (args != null && args.Length > 0)
            {
                try
                {
                    ret = String.Format(fmt, args);
                }
                catch (FormatException)
                {
                    ret = String.Format("Error while formatting #{0}#!", fmt);
                    Debug.Assert(false, "Trace string did cause format Exception: #{0}#", fmt);
                }
            }

            return ret;
        }

        private static string FormatTime(DateTime time)
        {
            char[] header = new char[13];
            header[2] = ':';
            header[5] = ':';
            header[8] = '.';
            header[12] = ' ';

            long ticks = time.Ticks;
            int n1 = (int)(ticks >> 32);
            int n2 = (int)ticks;
            if (n2 < 0)
                n1++;

            ticks = ((Math.BigMul(429497, n2) - (int)(Math.BigMul(1161359156, n2) >> 32) - Math.BigMul(1161359156, n1)) >> 32)
                         + Math.BigMul(n1, 429497);
            n1 = (int)(ticks >> 32);
            n2 = (int)ticks;
            if (n2 < 0)
                n1++;
            int q = n1 * 50 + ((50 * (n2 >> 16)) >> 16) - (int)(System.Math.BigMul(1244382467, n1) >> 32) - 1;
            int r = (int)(ticks - System.Math.BigMul(q, 86400000));
            if (r > 86400000)
                r -= 86400000;

            int unit = (int)(Math.BigMul(r >> 7, 9773437) >> 32) >> 6;
            n2 = (unit * 13) >> 7;
            n1 = r - 3600000 * unit;
            header[0] = ((char)(n2 + '0'));
            header[1] = ((char)(unit - 10 * n2 + '0'));

            unit = (int)((Math.BigMul(n1 >> 5, 2290650)) >> 32);
            n1 -= 60000 * unit;
            n2 = (unit * 13) >> 7;
            header[3] = ((char)(n2 + '0'));
            header[4] = ((char)(unit - 10 * n2 + '0'));

            unit = ((n1 >> 3) * 67109) >> 23;
            n1 -= 1000 * unit;
            n2 = (unit * 13) >> 7;
            header[6] = ((char)(n2 + '0'));
            header[7] = ((char)(unit - 10 * n2 + '0'));

            n2 = (n1 * 41) >> 12;
            header[9] = ((char)(n2 + '0'));
            n1 -= 100 * n2;

            n2 = (n1 * 205) >> 11;
            header[10] = ((char)(n2 + '0'));
            header[11] = ((char)(n1 - 10 * n2 + '0'));

            return new string(header);
        }


    }
}
