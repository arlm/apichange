
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace ApiChange.Infrastructure
{
    internal class TraceCfgParser
    {
        public TraceFilter Filters = null;
        public TraceFilter NotFilters = null;
        public TraceListener OutDevice = null;
        public bool UseAppConfigListeners = false;

        bool bHasError = false;

        static Dictionary<string, MessageTypes> myFlagTranslator = new Dictionary<string, MessageTypes>(StringComparer.OrdinalIgnoreCase)
        {
             {"inout", MessageTypes.InOut },
             {"info", MessageTypes.Info},
             {"i", MessageTypes.Info},
             {"information", MessageTypes.Info},
             {"instrument", MessageTypes.Instrument},
             {"warning", MessageTypes.Warning},
             {"warn", MessageTypes.Warning},
             {"w", MessageTypes.Warning},
             {"error", MessageTypes.Error},
             {"e", MessageTypes.Error},
             {"exception", MessageTypes.Exception},
             {"ex", MessageTypes.Exception},
             {"all", MessageTypes.All },
             {"*", MessageTypes.All }
        };

        static Dictionary<string, Level> myLevelTranslator = new Dictionary<string, Level>(StringComparer.OrdinalIgnoreCase)
        {
            {"l1", Level.L1},
            {"l2", Level.L2},
            {"l3", Level.L3},
            {"l4", Level.L4},
            {"l5", Level.L5},
            {"ldispose", Level.Dispose},
            {"l*", Level.All},
            {"level1", Level.L1},
            {"level2", Level.L2},
            {"level3", Level.L3},
            {"level4", Level.L4},
            {"level5", Level.L5},
            {"leveldispose", Level.Dispose},
            {"level*", Level.All}
        };
        


        IEnumerable<KeyValuePair<string,string []>> GetFilters(string [] filters, int nSkip)
        {
            foreach(string current in filters.Skip(nSkip))
            {
                string[] filterParts = current.Split(new char[] { '+', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (filterParts.Length < 2)
                {
                    bHasError = true;
                    InternalError.Print("The configuration string {0} did have an unmatched type severity or level filter part: {0}", current);
                }

                yield return new KeyValuePair<string,string []>(filterParts[0], filterParts.Skip(1).ToArray());
            }
        }

        /// <summary>
        /// Format string is of the form
        /// outDevice; type flag1+flag2+...;type flags; ...
        /// where flags are a combination of trace markers
        /// </summary>
        /// <param name="config"></param>
        public TraceCfgParser(string config)
        {
            if (String.IsNullOrEmpty(config))
            {
                return;
            }

            string [] parts = config.Split(new char [] { ';'}, StringSplitOptions.RemoveEmptyEntries)
                                    .Select( (str) => str.Trim() )
                                    .ToArray();

            foreach (KeyValuePair<string, string []> filter in GetFilters(parts, 1).Reverse())
            {
                string typeName = filter.Key.TrimStart(new char[] { '!' });
                bool bIsNotFilter = filter.Key.IndexOf('!') == 0;

                KeyValuePair<Level, MessageTypes> levelAndMsgFilter =  ParseMsgTypeFilter(filter.Value);

                TraceFilter curFilterInstance = new TraceFilter(typeName,
                    levelAndMsgFilter.Value,
                    levelAndMsgFilter.Key,
                    bIsNotFilter ? NotFilters : Filters);

                if (bIsNotFilter)
                {
                    NotFilters = curFilterInstance;
                }
                else
                {
                    Filters = curFilterInstance;
                }
            }

            if (parts.Length > 0)
            {
                OpenOutputDevice(parts[0].ToLower());
            }

            // when only output device was configured or wrong mask was entere we enable full tracing
            // by default
            if (this.Filters == null)
            {
                this.Filters = new TraceFilterMatchAll();
            }

            if (bHasError == true)
            {
                InternalError.PrintHelp();
            }
        }

        private void OpenOutputDevice(string outDevice)
        {
            string [] parts = outDevice.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            string deviceName = parts[0];
            string deviceConfig = String.Join(" ", parts.Skip(1).ToArray());

            switch (deviceName)
            {
                case "file":
                    if (deviceConfig == "")
                    {
                        deviceConfig = DefaultTraceFileBaseName;
                    }
                    OutDevice = new TextWriterTraceListener(CreateTraceFile(deviceConfig));
                    break;

                case "debugoutput":
                    OutDevice = new DefaultTraceListener();
                    break;

                case "console":
                    OutDevice = new ConsoleTraceListener();
                    break;
                case "null":
                    OutDevice = new NullTraceListener();
                    break;

                case "default":
                    UseAppConfigListeners = true;
                    OutDevice = new NullTraceListener();
                    break;

                default:
                    InternalError.Print("The trace output device {0} is not supported.", outDevice);
                    bHasError = true;
                    break;
            }
        }

        KeyValuePair<Level, MessageTypes> ParseMsgTypeFilter(string [] typeFilters)
        {
            MessageTypes msgTypeFilter = MessageTypes.None;
            Level level = Level.None;

            foreach (string filter in typeFilters)
            {
                MessageTypes curFilter = MessageTypes.None;
                Level curLevel = Level.None;

                if (!myFlagTranslator.TryGetValue(filter.Trim(), out curFilter))
                {
                    if (!myLevelTranslator.TryGetValue(filter.Trim(), out curLevel))
                    {
                        InternalError.Print("The trace message type filter string {0} was not expected.", filter);
                        bHasError = true;
                    }
                    else
                    {
                        level |= curLevel;
                    }
                }
                else
                {
                    msgTypeFilter |= curFilter;
                }
            }

            // if nothing was enabled we do enable full tracing by default
            msgTypeFilter = (msgTypeFilter == MessageTypes.None) ? MessageTypes.All : msgTypeFilter;
            level = (level == Level.None) ? Level.All : level;

            return new KeyValuePair<Level,MessageTypes>(level,msgTypeFilter);
        }

        public static string DefaultTraceFileBaseName
        {
            get
            {
                string mainModule = Process.GetCurrentProcess().MainModule.FileName;

                return Path.Combine(Path.GetDirectoryName(mainModule), 
                    Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule.FileName) + ".txt");
            }
        }

        public static string DefaultExpandedTraceFileName
        {
            get
            {
                return AddPIDAndAppDomainNameToFileName(DefaultTraceFileBaseName);
            }
        }


        public static TextWriter CreateTraceFile(string filebaseName)
        {
            string traceFileName = AddPIDAndAppDomainNameToFileName(Path.GetFullPath(filebaseName));
            string traceDir = Path.GetDirectoryName(traceFileName);

            FileStream fstream = null;
            bool successFullyOpened = false;
            for (int i = 0; i < 2; i++)  // Retry the open operation in case of errors
            {
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(traceFileName)); // if the directory to the trace file does not exist create it
                    fstream = new FileStream(traceFileName, FileMode.Create, FileAccess.Write, FileShare.Read);
                    successFullyOpened = true;
                }
                catch (IOException)  // try to open the file with another name in case of a locking error
                {
                    traceDir = traceFileName + Guid.NewGuid().ToString();
                }

                if (successFullyOpened)
                {
                    break;
                }
            }


            if (fstream != null)
            {
                TextWriter writer = new StreamWriter(fstream);
                // Create a synchronized TextWriter to enforce proper locking in case of concurrent tracing to file
                writer = StreamWriter.Synchronized(writer);
                return writer;
            }

            return null;
        }

        static public string AddPIDAndAppDomainNameToFileName(string file)
        {
            // any supplied PID would be useless since we always append the PID
            string fileName = Path.GetFileName(file).Replace("PID", "");

            int idx = fileName.LastIndexOf('.');
            if (idx == -1)
            {
                idx = fileName.Length;
            }

            string strippedAppDomainName = AppDomain.CurrentDomain.FriendlyName.Replace('.', '_');
            strippedAppDomainName = strippedAppDomainName.Replace(':', '_').Replace('\\', '_').Replace('/', '_');

            string pidAndAppDomainName = "_" + Process.GetCurrentProcess().Id.ToString() + "_" + strippedAppDomainName;


            // insert process id and AppDomain name
            fileName = fileName.Insert(idx, pidAndAppDomainName);

            return Path.Combine(Path.GetDirectoryName(file), fileName);
        }

    }
}
