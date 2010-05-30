
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiChange.Api.Introspection;
using Mono.Cecil;
using System.IO;

namespace ApiChange.Api.Scripting
{
    class WhoUsesTypeCommand : CommandBase
    {
        internal const string Argument = "whousestype";
        SheetInfo mySearchHeader = new SheetInfo
        {
            Columns = new List<ColumnInfo>
            {
                new ColumnInfo { Name = "Type", Width = TypeWidth },
                new ColumnInfo { Name = "Assembly", Width = AssemblyWidth},
                new ColumnInfo { Name = SourceFileCol, Width = SourceFileWidth}
            },

            SheetName = "Search Fields"
        };

        SheetInfo myResultHeader = new SheetInfo
        {
            Columns = new List<ColumnInfo>
            {
                 new ColumnInfo { Name = "Assembly", Width = AssemblyWidth},
                 new ColumnInfo { Name = "Type", Width = TypeWidth },
                 new ColumnInfo { Name = "Method", Width = MethodWidth },
                 new ColumnInfo { Name = "Field", Width = FieldWidth },
                 new ColumnInfo { Name = "Match Reason", Width = ReasonWidth},
                 new ColumnInfo { Name = "Match Item", Width = MatchItemWidth},
                 new ColumnInfo { Name = SourceFileCol, Width = SourceFileWidth},
                 new ColumnInfo { Name = "Line", Width = SourceLineWidth}
            },

            SheetName = "Results"
        };

        public WhoUsesTypeCommand(CommandData parsedArgs)
            : base(parsedArgs)
        {
            AddAdditionalColumnsWhenEnabled(mySearchHeader);
            AddAdditionalColumnsWhenEnabled(myResultHeader);
        }

        protected override void Validate()
        {
            base.Validate();
            base.ShowFullHelp = false;

            ValidateFileQuery(myParsedArgs.Queries1,
                "WhoUsesType the file query to search for type definitions is missing.",
                "WhoUsesType Invalid directory for file query {0} query.",
                "WhoUsesType The file query {0} did not match any files.");

            ValidateFileQuery(myParsedArgs.Queries2,
                  "-in <files> is missing. Search in these files for users of one or more types.",
                  "Invalid directory in -in {0} query.",
                  "The -in query {0} did not match any files.");

            if (String.IsNullOrEmpty(myParsedArgs.TypeQuery))
            {
                AddErrorMessage("The type query is missing. E.g string or System.String");
                SetInvalid();
            }
        }

        public override void Execute()
        {
            base.Execute();
            if (!IsValid)
            {
                Out.WriteLine("Expected: ApiChange -whousesType [typeQuery] [files] -in [files]");
                Help();
                return;
            }

            var typeQueries = TypeQuery.GetQueries(myParsedArgs.TypeQuery,TypeQueryMode.All);

            List<TypeDefinition> searchTypes = new List<TypeDefinition>();
            
            Writer.SetCurrentSheet(mySearchHeader);

            LoadAssemblies(myParsedArgs.Queries1, (cecilAssembly, file) =>
            {
                using (var pdbReader = new PdbInformationReader(myParsedArgs.SymbolServer))
                {
                    foreach (TypeDefinition matchingType in typeQueries.GetMatchingTypes(cecilAssembly))
                    {
                        var fileline = pdbReader.GetFileLine(matchingType);
                        lock (this)
                        {
                            Writer.PrintRow("{0}; {1}; {2}",
                                () => GetFileInfoWhenEnabled(fileline.Key),
                                matchingType.Print(),
                                Path.GetFileName(file),
                                fileline.Key);

                            searchTypes.Add(matchingType);
                        }
                    }
                }
            });

            if (searchTypes.Count == 0)
            {
                Out.WriteLine("Error: Could not find any matching types to search for. Aborting");
                return;
            }
            
            Writer.SetCurrentSheet(myResultHeader);

            LoadAssemblies(myParsedArgs.Queries2, (cecilAssembly, file) =>
            {
                using (UsageQueryAggregator agg = new UsageQueryAggregator(myParsedArgs.SymbolServer))
                {
                    new WhoUsesType(agg, searchTypes);
                    string fileName = Path.GetFileName(file);

                    agg.Analyze(cecilAssembly);

                    /*
                     "Assembly" "Type", "Method" "Field" "Match Reason" "Match Item"
                     "Source File" "Line"
                     */

                    lock (this)
                    {
                        foreach (var match in agg.MethodMatches)
                        {
                            Writer.PrintRow("{0}; {1}; {2}; {3}; {4}; {5}; {6}; {7}",
                                () => GetFileInfoWhenEnabled(match.SourceFileName),
                                fileName,
                                ((TypeDefinition)match.Match.DeclaringType).Print(),
                                match.Match.Print(MethodPrintOption.Full),
                                "",
                                match.Annotations.Reason,
                                match.Annotations.Item,
                                match.SourceFileName,
                                match.LineNumber);
                        }

                        foreach (var match in agg.TypeMatches)
                        {
                            Writer.PrintRow("{0}; {1}; {2}; {3}; {4}; {5}; {6}; {7}",
                                () => GetFileInfoWhenEnabled(match.SourceFileName),
                                fileName,
                                match.Match.Print(),
                                "",
                                "",
                                match.Annotations.Reason,
                                match.Annotations.Item,
                                match.SourceFileName,
                                "");
                        }

                        foreach (var match in agg.FieldMatches)
                        {
                            Writer.PrintRow("{0}; {1}; {2}; {3}; {4}; {5}; {6}; {7}",
                                () => GetFileInfoWhenEnabled(match.SourceFileName),
                                fileName,
                                ((TypeDefinition)match.Match.DeclaringType).Print(),
                                "",
                                match.Match.Print(FieldPrintOptions.All),
                                match.Annotations.Reason,
                                match.Annotations.Item,
                                match.SourceFileName,
                                "");
                        }
                    }
                }
            });
        }
    }
}
