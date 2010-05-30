
using System;
using System.Collections.Generic;
using System.IO;
using ApiChange.Infrastructure;
using ApiChange.ExternalData;

namespace ApiChange.Api.Scripting
{
    public class CommandData
    {
        public Commands Command
        {
            get;
            set;
        }

        public string PdbDownloadDir
        {
            get;
            set;
        }

        public bool Verbose
        {
            get;
            set;
        }

        public bool OutputToExcel
        {
            get;
            set;
        }

        public string ExcelOutputFileName
        {
            get;
            set;
        }

        public List<FileQuery> Queries1
        {
            get;
            set;
        }

        public bool EventSubscriptionImbalance
        {
            get;
            set;
        }

        public List<FileQuery> OldFiles
        {
            get { return Queries1; }
        }

        /// <summary>
        /// Used by ShowRebuildTargets to search in different locations for matching files
        /// </summary>
        public List<FileQuery> OldFiles2
        {
            get;
            set;
        }

        public List<FileQuery> NewFiles
        {
            get { return Queries2; }
        }

        public List<FileQuery> Queries2
        {
            get;
            set;
        }

        public List<FileQuery> SearchInQuery
        {
            get;
            set;
        }

        public string TypeAndInnerQuery
        {
            get;
            set;
        }

        public string TypeQuery
        {
            get;
            set;
        }

        /// <summary>
        /// Used by WhoUsesString command
        /// </summary>
        public string StringConstant
        {
            get;
            set;
        }

        /// <summary>
        /// Used by WhoUsesString command
        /// </summary>
        public bool WordMatch
        {
            get;
            set;
        }

        /// <summary>
        /// Used by WhoUsesString command
        /// </summary>
        public bool CaseSensitiveMatch
        {
            get;
            set;
        }

        public string Cwd
        {
            get;
            set;
        }

        public int ThreadCount
        {
            get;
            set;
        }

        internal static string mySymbolServer = Environment.GetEnvironmentVariable("SYMSERVER") ?? SiteConstants.DefaultSymbolServer;
        public string SymbolServer
        {
            get { return mySymbolServer;  }
            set { mySymbolServer = value; }
        }

        string myUnknownCommandLineSwitch;
        public string UnknownCommandLineSwitch
        {
            get
            {
                return myUnknownCommandLineSwitch;
            }

            internal set
            {
                // only flag the first unkown command line switch
                if (myUnknownCommandLineSwitch == null)
                    myUnknownCommandLineSwitch = value;
            }
        }

        /// <summary>
        /// If true retrieve for all matches also information about source file
        /// </summary>
        public bool WithFileInfo
        {
            get;
            set;
        }

        public string InvalidComandLineSwitch
        {
            get;
            internal set;
        }

        public CommandData()
        {
            this.Queries1 = new List<FileQuery>();
            this.Queries2 = new List<FileQuery>();
            this.SearchInQuery = new List<FileQuery>();
            this.OldFiles2 = new List<FileQuery>();
            ThreadCount = SiteConstants.DefaultThreadCount;
        }

        public ICommandLineAction GetCommand()
        {
            CommandBase cmd = null;

            switch (Command)
            {
                case Commands.CorFlags:
                    cmd = new CorFlagsCommand(this);
                    break;
                case Commands.Diff:
                    cmd = new DiffAssembliesCommand(this);
                    break;
                case Commands.MethodUsage:
                    cmd = new WhoUsesMethodCommand(this);
                    break;
                case Commands.WhoReferences:
                    cmd = new WhoReferencesCommand(this);
                    break;
                case Commands.DownloadPdbs:
                    cmd = new DowndLoadPdbsCommand(this);
                    break;
                case Commands.ShowRebuildTargets:
                    cmd = new ShowRebuildTargetsCommand(this);
                    break;
                case Commands.ShowStrongName:
                    cmd = new ShowStrongNameCommand(this);
                    break;
                case Commands.ShowReferences:
                    cmd = new ShowReferencesCommand(this);
                    break;
                case Commands.WhoImplementsInterface:
                    cmd = new WhoImplementsInterfaceCommand(this);
                    break;
                case Commands.WhoUsesField:
                    cmd = new WhoUsesFieldCommand(this);
                    break;
                case Commands.WhoUsesEvent:
                    cmd = new WhoUsesEventCommand(this);
                    break;
                case Commands.WhoUsesStringConstant:
                    cmd = new WhoUsesStringConstantCommand(this);
                    break;
                case Commands.WhoUsesType:
                    cmd = new WhoUsesTypeCommand(this);
                    break;
                case Commands.GetFileInfo:
                    cmd = new GetFileInfoCommand(this);
                    break;
                case Commands.None:
                    cmd = new NoneCommand(this);
                    break;

                default:
                    throw new NotSupportedException(String.Format("Command {0} is not supported", Command));
            }

            if (this.Cwd != null)
            {
                try
                {
                    Directory.SetCurrentDirectory(this.Cwd);
                }
                catch (IOException ex)
                {
                    Console.WriteLine("Error: Could not set current working directory to {0}. Reason: {1}",
                        this.Cwd, ex.Message);
                    cmd = new NoneCommand(this);
                }

                // Adjust File Queries root directory
                foreach (FileQuery query in this.Queries1)
                {
                    query.UseCwd = true;
                }
                foreach (FileQuery query in this.Queries2)
                {
                    query.UseCwd = true;
                }
                foreach (FileQuery query in this.SearchInQuery)
                {
                    query.UseCwd = true;
                }
            }

            return cmd;
        }
    }
}
