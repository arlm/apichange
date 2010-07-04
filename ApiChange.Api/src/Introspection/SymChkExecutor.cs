
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.ComponentModel;
using System.IO;
using System.Globalization;
using ApiChange.Infrastructure;

namespace ApiChange.Api.Introspection
{
    class SymChkExecutor : ISymChkExecutor
    {
        static TypeHashes myType = new TypeHashes(typeof(SymChkExecutor));

        internal string SymChkExeName = "symchk.exe";
        internal static bool bCanStartSymChk = true;

        static Regex symPassedFileCountParser = new Regex(@"SYMCHK: PASSED \+ IGNORED files = (?<succeeded>\d+) *", RegexOptions.IgnoreCase);
        static Regex symFailedFileParser = new Regex(@"SYMCHK: (?<filename>.*?) +FAILED", RegexOptions.IgnoreCase);

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

        public SymChkExecutor()
        {
            FailedPdbs = new List<string>();
        }

        internal string BuildCmdLine(string binaryFileName, string symbolServer, string downloadDir)
        {
            string lret =  String.Format("\"{0}\" /su \"{1}\" /oc \"{2}\"",
                    binaryFileName,
                    symbolServer,
                    downloadDir ?? Path.GetDirectoryName(binaryFileName));

            Tracer.Info(Level.L1, myType, "BuildCmdLine", "Symcheck command is {0} {1}", SymChkExeName, lret);

            return lret;
        }

        public bool DownLoadPdb(string fullbinaryName, string symbolServer, string downloadDir)
        {
            using (Tracer t = new Tracer(myType, "DownLoadPdb"))
            {
                bool lret = bCanStartSymChk;

                if (lret)
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo(
                                                            SymChkExeName,
                                                            BuildCmdLine(fullbinaryName,
                                                                          symbolServer,
                                                                          downloadDir));

                    startInfo.RedirectStandardOutput = true;
                    startInfo.RedirectStandardError = true;
                    startInfo.UseShellExecute = false;
                    startInfo.CreateNoWindow = true;

                    Process proc = null;
                    try
                    {
                        proc = Process.Start(startInfo);
                        proc.OutputDataReceived += proc_OutputDataReceived;
                        proc.ErrorDataReceived += proc_OutputDataReceived;
                        proc.BeginErrorReadLine();
                        proc.BeginOutputReadLine();

                        proc.WaitForExit();
                    }
                    catch (Win32Exception ex)
                    {
                        bCanStartSymChk = false;
                        t.Error(ex, "Could not start symchk.exe to download pdb files");
                        lret = false;
                    }
                    finally
                    {
                        if (proc != null)
                        {
                            proc.OutputDataReceived -= proc_OutputDataReceived;
                            proc.ErrorDataReceived -= proc_OutputDataReceived;
                            proc.Dispose();
                        }
                    }
                }

                if (FailedPdbs.Count > 0)
                {
                    lret = false;
                }

                return lret;
            }
        }

        void proc_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                string line = (string)e.Data;

                Match m = symPassedFileCountParser.Match(line);
                if (m.Success)
                {
                    lock (this)
                    {
                        SucceededPdbCount += int.Parse(m.Groups["succeeded"].Value, CultureInfo.InvariantCulture);
                    }
                }

                m = symFailedFileParser.Match(line);
                if (m.Success)
                {
                    lock (this)
                    {
                        FailedPdbs.Add(m.Groups["filename"].Value);
                    }
                }
            }
        }
    }
}
