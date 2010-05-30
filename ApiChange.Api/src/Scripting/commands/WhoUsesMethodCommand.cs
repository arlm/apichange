
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using ApiChange.Api.Introspection;
using System.Text.RegularExpressions;
using System.IO;
using Mono.Cecil.Pdb;

namespace ApiChange.Api.Scripting
{
    class WhoUsesMethodCommand : QueryCommandBase
    {
        public const string Argument = "whousesmethod";

        MethodQuery myMethodQuery;
        SheetInfo mySearchHeader = new SheetInfo
        {
            Columns = new List<ColumnInfo>
            {
                new ColumnInfo { Name = "Type", Width = TypeWidth },
                new ColumnInfo { Name = "Method", Width = MethodWidth },
                new ColumnInfo { Name = "Assembly", Width = AssemblyWidth},
                new ColumnInfo { Name = SourceFileCol, Width = SourceFileWidth}
            },

            SheetName = "Searched Methods"
        };

        SheetInfo myResultHeader = new SheetInfo
        {
            Columns = new List<ColumnInfo>
            {
                 new ColumnInfo { Name = "Type", Width = TypeWidth },
                 new ColumnInfo { Name = "Using Method", Width = MethodWidth },
                 new ColumnInfo { Name = "Used Method", Width = MethodWidth },
                 new ColumnInfo { Name = "Assembly", Width = AssemblyWidth},
                 new ColumnInfo { Name = SourceFileCol, Width = SourceFileWidth},
                 new ColumnInfo { Name = "Line", Width = SourceLineWidth}
            },

            SheetName = "Results"
        };

        public WhoUsesMethodCommand(CommandData cmdArgs)
            : base(cmdArgs)
        {
            AddAdditionalColumnsWhenEnabled(myResultHeader);
            AddAdditionalColumnsWhenEnabled(mySearchHeader);
        }


        protected override void Validate()
        {
            base.Validate();

            ValidateFileQuery(myParsedArgs.Queries1,
                "Command -whousesmethod <files> is missing. The methods defined in the query are searched in the assembly given here.",
                "Invalid directory in  -whousesmethod {0} query.",
                "The -whousesmethod query {0} did not match any files.");

            ValidateFileQuery(myParsedArgs.Queries2,
                    "-in <files> is missing. These assemblies are searched for their method usage.",
                    "Invalid directory in -in {0} query.",
                    "The -in query {0} did not match any files.");

            if(myParsedArgs.TypeAndInnerQuery == null)
            {
                AddErrorMessage("The method query was empty. Please enter a query of the form typeName([<visiblity>] <returnType> <FunctioName>(<arguments>))." + Environment.NewLine +
                                "Example: FTrace(*) searches for all methods in any class of FTrace defined in the assembly given in the -in query");
                SetInvalid();
            }
        }

        public override bool ExtractAndValidateTypeQuery(string query)
        {
            bool lret = base.ExtractAndValidateTypeQuery(query);

            if (lret == false)
            {
                Out.WriteLine("The Type/Method query must be of the form <TypeName>( [modifier] <returnType> <methodName>(<parameters>))");
            }
            else
            {
                myMethodQuery = new MethodQuery(base.myInnerQuery);
            }

            return lret;
        }

        public override void Execute()
        {
            base.Execute();
            if (!IsValid)
            {
                Help();
                return;
            }

            if (ExtractAndValidateTypeQuery(myParsedArgs.TypeAndInnerQuery) == false)
                return;

            List<MethodDefinition> methodsToSearch = new List<MethodDefinition>();
            
            Writer.SetCurrentSheet(mySearchHeader);

            LoadAssemblies(myParsedArgs.Queries1, (cecilAssembly, file) =>
            {
                using (PdbInformationReader pdbReader = new PdbInformationReader(myParsedArgs.SymbolServer))
                {
                    foreach(var type in myTypeQueries.GetMatchingTypes(cecilAssembly))
                    {
                        myMethodQuery.GetMethods(type).ForEach(
                        (method) =>
                        {
                            var fileLine = pdbReader.GetFileLine(method);
                            Writer.PrintRow("{0,-60}; {1,-100}; {2}; {3}",
                                () => GetFileInfoWhenEnabled(fileLine.Key),
                                type.Print(),
                                method.Print(MethodPrintOption.Full),
                                Path.GetFileName(file),
                                fileLine.Key
                                );

                            lock (methodsToSearch)
                            {
                                methodsToSearch.Add(method);
                            }
                        });
                    }
                }
            });

            if (methodsToSearch.Count == 0)
            {
                Out.WriteLine("Error: No methods to query found. Aborting query.");
                return;
            }

            Writer.PrintRow("",null);
            Writer.PrintRow("",null);
            Writer.SetCurrentSheet(myResultHeader);

            LoadAssemblies(myParsedArgs.Queries2, (cecilAssembly, file) =>
            {
                using (UsageQueryAggregator aggregator = new UsageQueryAggregator(myParsedArgs.SymbolServer))
                {
                    new WhoUsesMethod(aggregator, methodsToSearch);
                    aggregator.Analyze(cecilAssembly);
                    aggregator.MethodMatches.ForEach((result) =>
                    {
                        Writer.PrintRow("{0,-60};{1,-100}; {2}; {3}; {4}; {5}",
                            () => GetFileInfoWhenEnabled(result.SourceFileName),
                            result.Match.DeclaringType.FullName,
                            result.Match.Print(MethodPrintOption.Full),
                            result.Annotations.Item,
                            Path.GetFileName(file),
                            result.SourceFileName,
                            result.LineNumber
                            );
                    });
                }
            });
        }
    }
}
