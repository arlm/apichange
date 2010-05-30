
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiChange.Api.Introspection;
using Mono.Cecil;
using System.IO;

namespace ApiChange.Api.Scripting
{
    class WhoUsesFieldCommand : QueryCommandBase
    {
        public const string Argument = "whousesfield";
        FieldQuery myFieldQuery;

        SheetInfo mySearchHeader = new SheetInfo
        {
            Columns = new List<ColumnInfo>
            {
                new ColumnInfo { Name = "Type", Width = TypeWidth },
                new ColumnInfo { Name = "Field", Width = FieldWidth },
                new ColumnInfo { Name = "Assembly", Width = AssemblyWidth},
                new ColumnInfo { Name = SourceFileCol, Width = SourceFileWidth}
            },

            SheetName = "Search Fields"
        };

        SheetInfo myResultHeader = new SheetInfo
        {
            Columns = new List<ColumnInfo>
            {
                 new ColumnInfo { Name = "Using Type", Width = TypeWidth },
                 new ColumnInfo { Name = "Method", Width = MethodWidth },
                 new ColumnInfo { Name = "Assembly", Width = AssemblyWidth},
                 new ColumnInfo { Name = "Operation", Width = ReasonWidth},
                 new ColumnInfo { Name = "Accessed Field", Width = MatchItemWidth},
                 new ColumnInfo { Name = SourceFileCol, Width = SourceFileWidth},
                 new ColumnInfo { Name = "Line", Width = SourceLineWidth}
            },

            SheetName = "Results"
        };

        public WhoUsesFieldCommand(CommandData parsedArgs)
            : base(parsedArgs)
        {
            AddAdditionalColumnsWhenEnabled(mySearchHeader);
            AddAdditionalColumnsWhenEnabled(myResultHeader);
        }

        protected override void Validate()
        {
            base.Validate();

            ValidateFileQuery(myParsedArgs.Queries1,
              "Command -whousesfield <files> is missing. The fields defined in the query are searched in the assembly given here.",
               "Invalid directory in  -whousesfield {0} query.",
               "The -whousesfield query {0} did not match any files.");

            ValidateFileQuery(myParsedArgs.Queries2,
                    "-in <files> is missing. These assemblies are searched if they access any fields.",
                    "Invalid directory in -in {0} query.",
                    "The -in query {0} did not match any files.");

            if (myParsedArgs.TypeAndInnerQuery == null)
            {
                AddErrorMessage("The field query was empty. Please enter a query of the form typeName([<visiblity>] <type name> <field name>)." + Environment.NewLine +
                                "Example: *(public * *) searches for all public fields in the whole assembly defined in the assembly given in the -in query");
                SetInvalid();
            }
        }

        public override bool ExtractAndValidateTypeQuery(string query)
        {
            bool lret = base.ExtractAndValidateTypeQuery(query);

            if (lret == false)
            {
                Out.WriteLine("The Type/field query must be of the form typeName([<visiblity>] <type name> <field name>)");
                Out.WriteLine("Example: *(public * *) searches for all public fields in the whole assembly defined in the assembly given in the -in query");
            }
            else
            {
                if (base.myInnerQuery.Trim() == "*")
                {
                    myFieldQuery = FieldQuery.AllFields;
                }
                else
                {
                    myFieldQuery = new FieldQuery("nocompilergenerated " + base.myInnerQuery);
                }
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

            List<FieldDefinition> fieldsToSearch = new List<FieldDefinition>();
            Writer.SetCurrentSheet(mySearchHeader);

            LoadAssemblies(myParsedArgs.Queries1, (cecilAssembly, file) =>
            {
                using (PdbInformationReader pdbReader = new PdbInformationReader(myParsedArgs.SymbolServer))
                {
                    foreach (var type in myTypeQueries.GetMatchingTypes(cecilAssembly))
                    {
                        myFieldQuery.GetMatchingFields(type).ForEach(
                        (field) =>
                        {
                            var fileLine = pdbReader.GetFileLine((TypeDefinition)field.DeclaringType);
                            Writer.PrintRow(
                                "{0,-80}; {1,-50}; {2}; {3}",
                                () => GetFileInfoWhenEnabled(fileLine.Key),
                                type.Print(),
                                field.Print(FieldPrintOptions.All),
                                Path.GetFileName(file),
                                fileLine.Key
                                );

                            lock (this)
                            {
                                fieldsToSearch.Add(field);
                            }
                        });
                    }
                }
            });

            List<FieldDefinition> nonConstFields = (from field in fieldsToSearch
                                                    where !field.HasConstant
                                                    select field).ToList();

            if (nonConstFields.Count < fieldsToSearch.Count)
            {
                Out.WriteLine("Warning: It is not possible to track the usage of constant fields. Only non constant fields are considered in the search.");
                Out.WriteLine("         The only secure way is to search the source code for the constant field/s in question.");
            }

            if (nonConstFields.Count == 0)
            {
                Out.WriteLine("Error: No non constant fields are found which usage could be tracked");
            }

            Writer.PrintRow("",null);
            Writer.PrintRow("",null);

            Writer.SetCurrentSheet(myResultHeader);

            LoadAssemblies(myParsedArgs.Queries2, (cecilAssembly, file) =>
            {
                using (UsageQueryAggregator agg = new UsageQueryAggregator(myParsedArgs.SymbolServer))
                {
                    new WhoAccessesField(agg, nonConstFields);
                    agg.Analyze(cecilAssembly);

                    agg.MethodMatches.ForEach((result) =>
                    {
                        Writer.PrintRow("{0,-80};{1,-40}; {2}; {3}; {4}; {5}; {6}",
                            () => GetFileInfoWhenEnabled(result.SourceFileName),
                            result.Match.DeclaringType.FullName,
                            result.Match.Print(MethodPrintOption.Full),
                            Path.GetFileName(file),
                            result.Annotations.Reason,
                            result.Annotations.Item,
                            result.SourceFileName,
                            result.LineNumber
                            );
                    });
                }
            });
        }
    }
}
