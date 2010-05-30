
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiChange.Api.Introspection;
using System.IO;
using ApiChange.Infrastructure;

namespace ApiChange.Api.Scripting
{
    class WhoUsesStringConstantCommand : CommandBase
    {
        public const string Argument = "whousesstring";

        SheetInfo mySearchHeader = new SheetInfo
        {
            Columns = new List<ColumnInfo>
            {
                new ColumnInfo { Name = "Searched String", Width = TypeWidth },
            },

            SheetName = "Searched Strings"
        };


        const int StringWidth = 50;

        SheetInfo myResultHeader = new SheetInfo
        {
            Columns = new List<ColumnInfo>
            {
                 new ColumnInfo { Name = "Type", Width = TypeWidth },
                 new ColumnInfo { Name = "Method", Width = MethodWidth },
                 new ColumnInfo { Name = "Field", Width = FieldWidth },
                 new ColumnInfo { Name = "Loaded String", Width = StringWidth },
                 new ColumnInfo { Name = "Matched String", Width = StringWidth },
                 new ColumnInfo { Name = "Assembly", Width = AssemblyWidth},
                 new ColumnInfo { Name = SourceFileCol, Width = SourceFileWidth},
                 new ColumnInfo { Name = "Line", Width = SourceLineWidth}
            },

            SheetName = "Results"
        };


        public WhoUsesStringConstantCommand(CommandData cmdArgs)
            : base(cmdArgs)
        {
            AddAdditionalColumnsWhenEnabled(myResultHeader);
            AddAdditionalColumnsWhenEnabled(mySearchHeader);
        }


        protected override void Validate()
        {
            base.Validate();

            if( myParsedArgs.Queries1.Count > 0 )
            {
                ValidateFileQuery(myParsedArgs.Queries1,
                    "Command -whosuesstring <files> is missing. The methods defined in the query are searched in the assembly given here.",
                    "Invalid directory in  -whousesmethod {0} query.",
                    "The -whousesmethod query {0} did not match any files.");
            }

            ValidateFileQuery(myParsedArgs.Queries2,
                    "-in <files> is missing. These assemblies are searched for their string constant usage.",
                    "Invalid directory in -in {0} query.",
                    "The -in query {0} did not match any files.");

            if (String.IsNullOrEmpty(myParsedArgs.StringConstant))
            {
                base.SetInvalid();
                AddErrorMessage("Command {0} expects a search string.", Argument);
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

            Writer.SetCurrentSheet(mySearchHeader);
            Writer.PrintRow("{0,-60}", null, myParsedArgs.StringConstant);

            List<string> searchScope = myParsedArgs.Queries1.GetFiles().ToList();

            Writer.SetCurrentSheet(myResultHeader);
            base.LoadAssemblies(myParsedArgs.Queries2, (assembly, file) =>
                {
                    using (UsageQueryAggregator agg = new UsageQueryAggregator(myParsedArgs.SymbolServer))
                    {
                        new WhoUsesStringConstant(agg, 
                            myParsedArgs.StringConstant, 
                            myParsedArgs.WordMatch,
                            myParsedArgs.CaseSensitiveMatch ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);

                        if (searchScope.Count == 0)
                        {
                            // Force aggregator to search inside all files if no definig assemblies have been passsed.
                            agg.AddVisitScope(file);
                        }
                        else
                        {
                            searchScope.ForEach((dll) => agg.AddVisitScope(dll));
                        }

                        agg.Analyze(assembly);
                        foreach (var result in agg.MethodMatches)
                        {
                           Writer.PrintRow("{0,-60};{1,-100}; {2}; {3}; {4}; {5}; {6}; {7}",
                               () => GetFileInfoWhenEnabled(result.SourceFileName),
                               result.Match.DeclaringType.FullName,
                               result.Match.Print(MethodPrintOption.Full),
                               "",
                               result.Annotations["String"], 
                               result.Annotations.Item,
                               Path.GetFileName(file),
                               result.SourceFileName,
                               result.LineNumber
                               );
                        }

                        foreach (var result in agg.FieldMatches)
                        {
                            Writer.PrintRow("{0,-60};{1,-100}; {2}; {3}; {4}; {5}; {6}; {7}",
                                () => GetFileInfoWhenEnabled(result.SourceFileName),
                                result.Match.DeclaringType.FullName,
                                "",
                                result.Match.Print(FieldPrintOptions.Modifiers | FieldPrintOptions.SimpleType | FieldPrintOptions.Visibility),
                                result.Annotations["String"],
                                result.Annotations.Item,
                                Path.GetFileName(file),
                                result.SourceFileName,
                                ""
                                );

                        }
                    }
                });
        }
    }
}