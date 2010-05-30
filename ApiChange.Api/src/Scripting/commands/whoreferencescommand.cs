
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiChange.Api.Introspection;
using System.IO;

namespace ApiChange.Api.Scripting
{
    class WhoReferencesCommand : CommandBase
    {
        string myReferencedAssembly;

        public WhoReferencesCommand(CommandData parsedArgs)
            : base(parsedArgs)
        {
        }

        protected override void Validate()
        {
            base.Validate();

            bool bValidQuery = ValidateFileQuery(myParsedArgs.Queries1,
                    "Command -whoreferences needs a file. This is the file which is referenced by the others.",
                    "Directory not found for file {0}.",
                    "The referenced file {0} did not match any files.");

            if (bValidQuery && myParsedArgs.Queries1.Count > 1)
            {
                AddErrorMessage("You can search only for one reference at one time. Please specify only a simple filename without any * placeholders.");
                bValidQuery = false;
                SetInvalid();
            }

            
            if (bValidQuery)
            {
                if (myParsedArgs.Queries1[0].Files.Length>1)
                {
                    AddErrorMessage("Command -whoreferences {0} did match more than one file. You can search only for one reference at one time.", 
                        myParsedArgs.Queries1[0].Query);
                    SetInvalid();
                }
                else
                {
                    myReferencedAssembly = myParsedArgs.Queries1[0].EnumerateFiles.Dequeue();
                }
            }

            ValidateFileQuery(myParsedArgs.Queries2,
                "Command -whoreferences needs a -in <files> query. The files of the -in query are searched and all files which reference -from file are printed.",
                "Invalid directory in -in {0} query.",
                "Command -whoreferences -in {0} did not match any files.");

            if( myParsedArgs.OutputToExcel )
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

            int assemblyRefCount = 0;

            Out.WriteLine("The following assemblies reference {0}", Path.GetFileName(myReferencedAssembly));
            LoadAssemblies(myParsedArgs.Queries2, (cecilAssembly, file) =>
            {
                using (UsageQueryAggregator aggregator = new UsageQueryAggregator())
                {
                    new WhoReferencesAssembly(aggregator, myReferencedAssembly);
            
                    aggregator.Analyze(cecilAssembly);
                    if (aggregator.AssemblyMatches.Count > 0)
                    {
                        Out.WriteLine("{0}", aggregator.AssemblyMatches.ToList()[0]);
                    }

                    lock (this)
                    {
                        assemblyRefCount += aggregator.AssemblyMatches.Count;
                    }
                }
            });

            Out.WriteLine("Total References: {0}", assemblyRefCount);
        }
    }
}
