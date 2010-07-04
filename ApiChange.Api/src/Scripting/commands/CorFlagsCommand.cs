
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiChange.Api.Introspection;
using System.IO;
using ApiChange.Infrastructure;

namespace ApiChange.Api.Scripting
{
    class CorFlagsCommand : CommandBase
    {
        static TypeHashes myType = new TypeHashes(typeof(CorFlagsCommand));

        SheetInfo mySheetLayout = new SheetInfo
        {
            Columns = new List<ColumnInfo>
            {
                new ColumnInfo { Name = "File Name", Width = SourceFileWidth },
                new ColumnInfo { Name = "Type", Width = 20 },
                new ColumnInfo { Name = "Size", Width = 10},
                new ColumnInfo { Name = "Processor", Width = 12},
                new ColumnInfo { Name = "IL Only", Width = 10},
                new ColumnInfo { Name = "Signed", Width = 10}
            },
            SheetName = "File Infos"
        };

        public CorFlagsCommand(CommandData parsedArgs)
            : base(parsedArgs)
        {
        }

        protected override void Validate()
        {
            base.Validate();

            ValidateFileQuery(myParsedArgs.Queries1,
                    "Command -corflags expects a file query to display their flags.",
                    "Invalid directory in -corflags {0} query.",
                    "Command -corflags. The query {0} did not match any files.");
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

                Writer.SetCurrentSheet(mySheetLayout);

                base.GetFilesFromQueryMultiThreaded(myParsedArgs.Queries1, LoadFiles);
            }
        }

        void LoadFiles(string fileName)
        {
            using (Tracer t = new Tracer(Level.L2, myType, "LoadFiles"))
            {
                using (FileStream fStream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                {
                    FileInfo info = new FileInfo(fileName);
                    if (info.Length == 0)
                    {
                        t.Warning("Did get 0 byte file: {0}", fileName);
                        return;
                    }

                    CorFlagsReader data = CorFlagsReader.ReadAssemblyMetadata(fStream);

                    string partialPath = Path.GetFileName(fileName);

                    if (data == null)
                    {
                        t.Info("File does not seem to be a valid PE File: {0}", data);
                        Writer.PrintRow("{0}; {1}; {2}", null, partialPath, "Unknown", info.Length);
                        return;
                    }

                    if (data.MajorRuntimeVersion > 0)
                    {
                        Writer.PrintRow("{0}; {1}; {2}; {3}; {4}", null, partialPath, "Managed",
                            info.Length,
                            data.ProcessorArchitecture,
                            data.IsPureIL ? "IL Only" : "Managed C++",
                            data.IsSigned ? "Signed" : "Unsigned");
                    }
                    else
                    {
                        Writer.PrintRow("{0}; {1}; {2}; {3}", null, partialPath,
                            "Unmanaged",
                            info.Length,
                            data.ProcessorArchitecture);
                    }
                };
            }

        }
    }
}
