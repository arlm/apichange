
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using ApiChange.Api.Introspection.Diff;
using System.Diagnostics;
using ApiChange.Api.Introspection;
using ApiChange.Infrastructure;

namespace ApiChange.Api.Scripting
{
    class DiffAssembliesCommand : CommandBase
    {
        static TypeHashes myType = new TypeHashes(typeof(DiffAssembliesCommand));

        #region ICommandeLineAction Members

        public DiffAssembliesCommand(CommandData cmdArgs):base(cmdArgs)
        {
        }

        protected override void Validate()
        {
            base.Validate();

            ValidateFileQuery(myParsedArgs.NewFiles,
                        "-new <filequery> is missing.",
                        "Invalid directory in -new {0} query.",
                        "The -new query {0} did not match any files.");

            ValidateFileQuery(myParsedArgs.OldFiles,
                        "-old <filequery> is missing.",
                        "Not existing directory in -old {0} query.",
                        "The -old query {0} did not match any files.");

            if (myParsedArgs.OutputToExcel)
            {
                AddErrorMessage("Excel output is not supported by this comand");
                SetInvalid();
            }
        }

        public override void Execute()
        {
            using (Tracer t = new Tracer(myType, "Execute"))
            {
                base.Execute();
                if (!IsValid)
                {
                    Help();
                    return;
                }

                Out.WriteLine("Compare from {0} against {1}", myParsedArgs.Queries1.GetSearchDirs(), myParsedArgs.Queries2.GetSearchDirs());
                int removedTypes = 0;
                int changedTypes = 0;

                List<string> removedFiles = myParsedArgs.Queries1.GetNotExistingFilesInOtherQuery(myParsedArgs.Queries2);
                if (removedFiles.Count > 0)
                {
                    Out.WriteLine("Removed {0} files", removedFiles.Count);
                    foreach (string str in removedFiles)
                    {
                        Console.WriteLine("\t{0}", Path.GetFileName(str));
                    }
                }

                HashSet<string> query1 = new HashSet<string>(myParsedArgs.Queries1.GetFiles(), new FileNameComparer());
                HashSet<string> query2 = new HashSet<string>(myParsedArgs.Queries2.GetFiles(), new FileNameComparer());

                // Get files which are present in one set and the other
                query1.IntersectWith(query2);
                DiffPrinter printer = new DiffPrinter(Out);

                foreach (string fileName1 in query1)
                {

                    if (fileName1.EndsWith(".XmlSerializers.dll", StringComparison.OrdinalIgnoreCase))
                    {
                        t.Info("Ignore xml serializer dll {0}", fileName1);
                        continue;
                    }

                    string fileName2 = myParsedArgs.Queries2.GetMatchingFileByName(fileName1);

                    var assemblyV1 = AssemblyLoader.LoadCecilAssembly(fileName1);
                    var assemblyV2 = AssemblyLoader.LoadCecilAssembly(fileName2);

                    if (assemblyV1 != null && assemblyV2 != null)
                    {
                        AssemblyDiffer differ = new AssemblyDiffer(assemblyV1, assemblyV2);
                        AssemblyDiffCollection diff = differ.GenerateTypeDiff(QueryAggregator.PublicApiQueries);
                        removedTypes += diff.AddedRemovedTypes.RemovedCount;
                        changedTypes += diff.ChangedTypes.Count;

                        if (diff.AddedRemovedTypes.Count > 0 || diff.ChangedTypes.Count > 0)
                        {
                            Out.WriteLine("{0} has {1} changes", Path.GetFileName(fileName1), diff.AddedRemovedTypes.Count +
                                diff.ChangedTypes.Sum(type => type.Events.Count + type.Fields.Count + type.Interfaces.Count + type.Methods.Count));

                            printer.Print(diff);
                        }
                    }
                }

                Out.WriteLine("From {0} assemblies were {1} types removed and {2} changed.",
                    myParsedArgs.Queries1.GetFiles().Count(), removedTypes, changedTypes);
            }
        }

        #endregion
    }
}
