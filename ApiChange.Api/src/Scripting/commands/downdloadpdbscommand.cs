
using System;
using System.Linq;
using System.Diagnostics;
using System.IO;
using ApiChange.Api.Introspection;
using ApiChange.Infrastructure;
using ApiChange.ExternalData;

namespace ApiChange.Api.Scripting
{
    class DowndLoadPdbsCommand : CommandBase
    {
        const float AverageSecondsPerPdb = 0.4f;
        internal PdbDownLoader myloader; 


        public DowndLoadPdbsCommand(CommandData parsedArgs):base(parsedArgs)
        {
            myloader = new PdbDownLoader(parsedArgs.ThreadCount);
        }

        protected override void Validate()
        {
            base.Validate();

            ValidateFileQuery(myParsedArgs.Queries1,
                     String.Format("File query for the dll/exe file is not present. E.g. *.dll to download for all dlls the pdbs from the symbol server {0}.", myParsedArgs.SymbolServer),
                     "Invalid directory in {0} query.",
                     "The file query {0} did not select any files!");

            if (!String.IsNullOrEmpty(myParsedArgs.PdbDownloadDir))
            {
                try
                {
                    Directory.CreateDirectory(myParsedArgs.PdbDownloadDir);
                }
                catch (Exception ex)
                {
                    Trace.TraceError("Could not create pdb download directory {0}: {1}", myParsedArgs.PdbDownloadDir, ex);
                }

                if (!Directory.Exists(myParsedArgs.PdbDownloadDir))
                {
                    AddErrorMessage("The pdb download directory does not exist and it could not be created. Perhaps it is an invalid directory name: {0}", myParsedArgs.PdbDownloadDir);
                    SetInvalid();
                }
            }

            if (myParsedArgs.OutputToExcel)
            {
                AddErrorMessage("Excel output is not supported by this comand");
                SetInvalid();
            }
        }


        public override void Execute()
        {
            base.Execute();
            if(!IsValid)
            {
                Help();
                return;
            }

            Out.WriteLine("Start downloading symbols.");
            Action async = () =>
                {
                    try
                    {
                        TimeSpan duration = new TimeSpan(0, 0, (int)(AverageSecondsPerPdb * myParsedArgs.Queries1.GetFiles().Count()));

                        Out.WriteLine("Estimated download time {0}:{1} minutes for {2} files from symbol server {3}",
                            duration.Minutes,
                            duration.Seconds,
                            myParsedArgs.Queries1.GetFiles().Count(),
                            myParsedArgs.SymbolServer);
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError("Got Exception: {0}", ex);
                    }
                };
            async.BeginInvoke(null, null);

            int total = 0;
            foreach (FileQuery query in myParsedArgs.Queries1)
            {
                myloader.DownloadPdbs(query, myParsedArgs.SymbolServer ?? SiteConstants.DefaultSymbolServer, myParsedArgs.PdbDownloadDir);
                total += query.Files.Length;
            }

            if (SymChkExecutor.bCanStartSymChk == false)
            {
                Out.WriteLine("Error: symchk.exe is not in the path. Plase download Windbg from Microsoft and point your PATH to the windbg location.");
            }
            else
            {
                Out.WriteLine("Did download {0}/{1} pdbs.", myloader.SucceededPdbCount, total);
                foreach (string failed in myloader.FailedPdbs)
                {
                    Out.WriteLine("Failed: {0}", failed);
                }
            }
        }
    }
}
