
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using ApiChange.Api.Introspection;
using System.Threading;
using System.Diagnostics;
using ApiChange.ExternalData;
using System.Reflection;
using ApiChange.Infrastructure;

namespace ApiChange.Api.Scripting
{
    class CommandBase : ICommandLineAction
    {
        public const int AssemblyWidth = 60;
        public const int MethodWidth = 80;
        public const int FieldWidth = 40;
        public const int SourceFileWidth = 90;
        public const int SourceLineWidth = 6;
        public const int TypeWidth = 80;
        public const int ReasonWidth = 20;
        public const int MatchItemWidth = 80;

        public const string SourceFileCol = "Source File";

        // Aditional columns which are added when 
        List<ColumnInfo> Columns = new List<ColumnInfo>
        {
            new ColumnInfo { Name = "User Name", Width = 25},
            new ColumnInfo { Name = "Mail", Width = 30},
            new ColumnInfo { Name = "Phone", Width = 20},
            new ColumnInfo { Name = "Department", Width = 25},
        };

        protected CommandData myParsedArgs;
        public TextWriter Out = Console.Out;
        List<string> myErrorStrings = new List<string>();
        protected bool ShowFullHelp = true;
        protected bool IsVerbose = false;
        bool myIsValid;

        protected IOutputWriter Writer
        {
            get;
            set;
        }

        IFileInformationProvider myFileInfoProvider; 

        protected void SetInvalidIfInvalid(bool state)
        {
            if (myIsValid)
                myIsValid = state;
        }

        protected void SetInvalid()
        {
            myIsValid = false;
        }

        public bool IsValid
        {
            get
            {
                return myIsValid;
            }
        }

        public void AddErrorMessage(string fmt, params object[] args)
        {
            myErrorStrings.Add(String.Format(fmt, args));
        }

        protected virtual List<string> GetFileInfoWhenEnabled(string file)
        {
            List<string> ret = new List<string>();

            // If the pdb is not present we have empty file names from which we wont get any infos
            if (myFileInfoProvider != null && !String.IsNullOrEmpty(file))
            {
                UserInfo infos = myFileInfoProvider.GetInformationFromFile(file);
                if (infos != null)
                {
                    ret = new List<string> { infos.DisplayName, infos.Mail, infos.Phone, infos.Department };
                }
            }

            return ret;
        }

        protected void AddAdditionalColumnsWhenEnabled(SheetInfo sheet)
        {
            if (myParsedArgs.WithFileInfo)
            {
                sheet.Columns.AddRange(Columns);
            }
        }

        public CommandBase(CommandData parsedArgs)
        {
            myParsedArgs = parsedArgs;
            IsVerbose = parsedArgs.Verbose;
            myIsValid = true;
        }

        protected bool ValidateFileQuery(List<FileQuery> queries, string missingmessage, string wrongDir, string noFiles)
        {
            bool lret = true;

            if (queries.Count == 0) // necessary option missing
            {
                AddErrorMessage(missingmessage);
                lret = false;
            }

            if (lret)
            {
                // queries with wrong directory
                foreach (FileQuery q in queries)
                {
                    try
                    {
                        q.BeginSearch();
                    }
                    catch (DirectoryNotFoundException)
                    {
                        AddErrorMessage(wrongDir, q.Query);
                        ShowFullHelp = false;
                        lret = false;
                    }
                }
            }

            if(lret && !queries.HasMatches()) // query which matches no files
            {
                AddErrorMessage(noFiles, queries.GetQueries());
                ShowFullHelp = false;
                lret = false;
            }

            SetInvalidIfInvalid(lret);

            return lret;
        }

        #region ICommandeLineAction Members

        public virtual void Execute()
        {
            Validate();

            if (myIsValid)  // construct output files only if command itself is valid
            {
                if (myParsedArgs.OutputToExcel)
                {
                    Writer = new ExcelOutputWriter(myParsedArgs.ExcelOutputFileName,
                        myParsedArgs.ExcelOutputFileName == null ? ExcelOptions.Visible : ExcelOptions.CloseOnExit);
                }
                else
                {
                    Writer = new TextOutputWriter(this);
                }

                if (myParsedArgs.WithFileInfo)
                {
                    myFileInfoProvider = new ClearCaseToADMapper();
                }

            }
        }
        #endregion

        protected virtual void Validate()
        {
            if (!String.IsNullOrEmpty(myParsedArgs.InvalidComandLineSwitch))
            {
                AddErrorMessage(myParsedArgs.InvalidComandLineSwitch);
                SetInvalid();
            }

            if (!String.IsNullOrEmpty(myParsedArgs.UnknownCommandLineSwitch))
            {
                AddErrorMessage("The command line switch -{0} is unknown", myParsedArgs.UnknownCommandLineSwitch);
                SetInvalid();
            }
        }



        protected void GetFilesFromQueryMultiThreaded(List<FileQuery> fileQueries, Action<string> fileOperator)
        {
            BlockingQueue<string> [] queues = (from query in fileQueries
                                               select query.EnumerateFiles).ToArray();

            BlockingQueueAggregator<string> aggregator = new BlockingQueueAggregator<string>(queues);

            var dispatcher = new WorkItemDispatcher<string>(myParsedArgs.ThreadCount, fileOperator, "File Loader", aggregator, WorkItemOptions.AggregateExceptions);

            // Wait until work is done
            dispatcher.Dispose();
        }

        protected void LoadAssemblies(List<FileQuery> fileQueries, Action<AssemblyDefinition,string> action)
        {
            GetFilesFromQueryMultiThreaded(fileQueries, (file) =>
            {
                var cecilAssembly = AssemblyLoader.LoadCecilAssembly(file);
                if (cecilAssembly == null)
                {
                    return;
                }

                if (IsVerbose)
                {
                    Out.WriteLine("Search in {0}", file);
                }

                action(cecilAssembly, file);

             });
        }

        public void Help()
        {
            if (ShowFullHelp)
            {
                Out.WriteLine(SiteConstants.HelpStr);
            }

            if (myParsedArgs.Cwd != null)
            {
                Out.WriteLine("Current working directory was: {0}", myParsedArgs.Cwd);
            }

            foreach (var error in myErrorStrings)
                Out.WriteLine("Error: " + error);
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (Writer != null)
            {
                Writer.Dispose();
                Writer = null;
            }
        }

        #endregion
    }
}
