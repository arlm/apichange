
using System;
using System.Collections.Generic;
using System.Linq;
using ApiChange.Api.Introspection;
using Mono.Cecil;
using ApiChange.Api.Introspection.Diff;
using System.IO;
using ApiChange.Infrastructure;

namespace ApiChange.Api.Scripting
{
    class ShowRebuildTargetsCommand : CommandBase
    {
        public ShowRebuildTargetsCommand(CommandData parsedArgs)
            : base(parsedArgs)
        {
        }

        protected override void Validate()
        {
            base.Validate();

            ValidateFileQuery(myParsedArgs.NewFiles,
                "-new <filequery> is missing. You need to select some files you want to compare against to search for breaking changes.",
                "Invalid directory in -new {0} query.",
                "The -new query {0} did not match any files.");

            List<FileQuery> allOldQueries = new List<FileQuery>(myParsedArgs.OldFiles);
            allOldQueries.AddRange(myParsedArgs.OldFiles2);

            ValidateFileQuery(allOldQueries,
                "-old/-old2 <filequery> is missing. You need to select some files you want to compare against to search for breaking changes.",
                "Invalid directory in -old/-old2 {0} query.",
                "The -old/-old2 query {0} did not match any files.");

             ValidateFileQuery(myParsedArgs.SearchInQuery,
                    "-searchin <filequery> is missing. You need to select some files you want to compare against to search for breaking changes.",
                    "Invalid directory in -searchin {0} query.",
                    "The -searchin query {0} did not match any files.");

            if( IsValid ) 
            {
                HashSet<string> removedFiles1 = new HashSet<string>(myParsedArgs.NewFiles.GetNotExistingFilesInOtherQuery(myParsedArgs.OldFiles),new FileNameComparer());
                HashSet<string> removedFiles2 = new HashSet<string>(myParsedArgs.NewFiles.GetNotExistingFilesInOtherQuery(myParsedArgs.OldFiles2),new FileNameComparer());

                removedFiles1.IntersectWith(removedFiles2);

                if (removedFiles1.Count > 0)
                {
                    Out.WriteLine("Warning: {0} files are not present.", removedFiles1.Count);
                    foreach(string file in removedFiles1)
                    {
                        Out.WriteLine("\tNot found or removed {0}", file);
                    }
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
            if (!IsValid)
            {
                Help();
                return;
            }

            List<AssemblyDiffCollection> assemblyDiffs = new List<AssemblyDiffCollection>();
            DiffPrinter printer = new DiffPrinter(Out);

            foreach (string newFile in myParsedArgs.NewFiles.GetFiles())
            {
                string oldFile = myParsedArgs.OldFiles.GetMatchingFileByName(newFile);
                if (oldFile == null)
                {
                    oldFile = myParsedArgs.OldFiles2.GetMatchingFileByName(newFile);
                }

                if (oldFile == null)
                {
                    Out.WriteLine("Warning: {0} seems to be a new file. Cannot diff.", Path.GetFileName(newFile));
                    continue;
                }


                try
                {
                    AssemblyDiffer differ = new AssemblyDiffer(oldFile, newFile);
                    AssemblyDiffCollection diff = differ.GenerateTypeDiff(QueryAggregator.AllExternallyVisibleApis);
                    if( diff.AddedRemovedTypes.Count > 0 ||
                        diff.ChangedTypes.Count > 0 )
                    {
                        Out.WriteLine("Changes of {0}", Path.GetFileName(newFile));
                        printer.Print(diff);
                        assemblyDiffs.Add(diff);
                    }
                }
                catch (ArgumentException)
                {
                    // ignore C++ and other targets in diff
                }
            }

            UsageQueryAggregator usage = new UsageQueryAggregator();

            BreakingChangeSearcher breaking = new BreakingChangeSearcher(assemblyDiffs, usage);

            foreach (string fileV2 in myParsedArgs.SearchInQuery.GetFiles())
            {
                AssemblyDefinition aV2 = AssemblyLoader.LoadCecilAssembly(fileV2);
                if (aV2 != null)
                {
                    usage.Analyze(aV2);
                }
            }

            Out.WriteLine("Detected {0} assemblies which need a recompilation", usage.AssemblyMatches.Count);

            foreach(var needsRecompilation in usage.AssemblyMatches)
            {
                Out.WriteLine("{0}", Path.GetFileName(needsRecompilation));
            }
        }
    }
}
