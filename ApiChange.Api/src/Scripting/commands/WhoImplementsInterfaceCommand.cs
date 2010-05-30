
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiChange.Api.Introspection;
using Mono.Cecil;
using System.IO;

namespace ApiChange.Api.Scripting
{
    class WhoImplementsInterfaceCommand : CommandBase
    {
        public const string Argument = "whoimplementsinterface";

        SheetInfo mySearchHeader = new SheetInfo
        {
            Columns = new List<ColumnInfo>
            {
                new ColumnInfo { Name = "Searched Interface", Width = TypeWidth },
                new ColumnInfo { Name = "Assembly", Width = AssemblyWidth},
            },

            SheetName = "Search Interfaces"
        };

        SheetInfo myResultHeader = new SheetInfo
        {
            Columns = new List<ColumnInfo>
            {
                 new ColumnInfo { Name = "Type", Width = TypeWidth },
                 new ColumnInfo { Name = "Declaring Assembly", Width = AssemblyWidth },
                 new ColumnInfo { Name = "Implemented Interface", Width = TypeWidth},
                 new ColumnInfo { Name = SourceFileCol, Width = SourceFileWidth},
            },

            SheetName = "Results"
        };

        public WhoImplementsInterfaceCommand(CommandData parsedArgs)
            : base(parsedArgs)
        {
            AddAdditionalColumnsWhenEnabled(myResultHeader);
        }

        protected override void Validate()
        {
            base.Validate();

            ValidateFileQuery(myParsedArgs.Queries1,
                  "WhoImplementsInterface <files> is missing. This are the files in which for the interface definition is searched.",
                  "WhoImplementsInterface invalid directory in {0} file query for interface definitions.",
                  "WhoImplementsInterface the query {0} did not match any files to look for an interface definition.");

            ValidateFileQuery(myParsedArgs.Queries2,
                  "-in <files> is missing. Search in these files for implementers of this interface.",
                  "Invalid directory in -in {0} query.",
                  "The -in query {0} did not match any files.");

            if (String.IsNullOrEmpty(myParsedArgs.TypeQuery))
            {
                AddErrorMessage("The interface query is missing. E.g IEnumerable or System.IEnumerable");
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

            var typeQueries = TypeQuery.GetQueries(myParsedArgs.TypeQuery,
                                                     TypeQueryMode.Interface |
                                                     TypeQueryMode.Public |
                                                     TypeQueryMode.Internal);


            List<TypeDefinition> searchInterfaces = new List<TypeDefinition>();

            Writer.SetCurrentSheet(mySearchHeader);
            LoadAssemblies(myParsedArgs.Queries1, (itfAssembly, file) =>
            {
                foreach (TypeDefinition matchingItf in typeQueries.GetMatchingTypes(itfAssembly))
                {
                    Writer.PrintRow("{0}; {1}",
                                null,
                                matchingItf.Print(), 
                                Path.GetFileName(file)
                        );

                    lock (searchInterfaces)
                    {
                        searchInterfaces.Add(matchingItf);
                    }
                }
            });

            if (searchInterfaces.Count == 0)
            {
                Out.WriteLine("No Interfaces found. Aborting query.");
                return;
            }

            Writer.PrintRow("",null);
            Writer.PrintRow("",null);

            Writer.SetCurrentSheet(myResultHeader);

            LoadAssemblies(myParsedArgs.Queries2, (implAssembly,file) =>
            {
                using (UsageQueryAggregator agg = new UsageQueryAggregator(myParsedArgs.SymbolServer))
                {
                    new WhoImplementsInterface(agg, searchInterfaces);
                    agg.Analyze(implAssembly);

                    foreach (var match in agg.TypeMatches)
                    {
                        Writer.PrintRow("{0,-60}; {1,-60}; {2}; {3}",
                            () => GetFileInfoWhenEnabled(match.SourceFileName),
                            match.Match.Print(),
                            Path.GetFileName(file),
                            match.Annotations.Item,
                            match.SourceFileName);
                    }
                }
            });
        }
    }
}
