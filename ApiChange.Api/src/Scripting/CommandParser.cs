
using System;
using System.Collections.Generic;
using System.Diagnostics;
using ApiChange.Infrastructure;
using ApiChange.ExternalData;

namespace ApiChange.Api.Scripting
{
    public class CommandParser
    {
        static TypeHashes myType = new TypeHashes(typeof(CommandParser));

        string[] myArgs;
        int myParseIdx;
        const int SignificantCharsOfParameter = 30;

        /// <summary>
        /// Returns for a command line argument the missing options. 
        /// Example:  /F "a" "b" "c"
        /// will return a list with "a", "b", "c"
        /// </summary>
        /// <returns>List of optional command line arguments. If no options could be found an empty list 
        /// is returned.</returns>
        List<string> NextOptionForArgument()
        {
            var ret = new List<string>();

            while( (myParseIdx+1) != myArgs.Length &&   // no more arguments
                   !IsArgument(myArgs[myParseIdx+1]) )  // No option for current argument
            {
                ret.Add(myArgs[++myParseIdx]);
            }

            return ret;
        }

        bool IsArgument(string arg)
        {
            if (arg.Length > 0 && (arg[0] == '-' || arg[0] == '/'))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        string MakeArgLowerCase(string arg)
        {
            if (IsArgument(arg))
            {
                return arg.Substring(1, 
                    (arg.Length <= SignificantCharsOfParameter) ? arg.Length - 1 : SignificantCharsOfParameter).ToLower();
            }
            else
                return arg;
        }

        void AddQuery(List<FileQuery> fileQuery,CommandData cmdArgs)
        {
            var hookCmd = NextOptionForArgument();
            AddQuery(fileQuery, cmdArgs, hookCmd);
        }



        internal string ExpandDefaultFileQueries(string filequery)
        {
            string ret = filequery;
            // since dictionary does not retain ordering we do two passes 
            for (int i = 0; i < 2; i++)
            {
                foreach (var replacement in SiteConstants.DefaultFileQueryReplacements)
                {
                    ret = ret.Replace(replacement.Key, replacement.Value);
                }
            }

            // since dictionary does not retain ordering we do two passe
            for (int i = 0; i < 2; i++)
            {
                foreach (var replacement in SiteConstants.DefaultFileQueryReplacements2)
                {
                    ret = ret.Replace(replacement.Key, replacement.Value);
                }
            }

            return ret;
        }


        void AddQuery(List<FileQuery> fileQuery, CommandData cmdArgs, List<string> hookCmd)
        {
            if (hookCmd.Count == 0)
            {
                return;
            }
            else if (hookCmd.Count > 1)
            {
                cmdArgs.InvalidComandLineSwitch =
                    String.Format("Unexpected file query parameter: {0}. Hint: Multiple file queries must be separated with ; as separator.", hookCmd[1]);
                return;
            }

            fileQuery.AddRange(FileQuery.ParseQueryList(ExpandDefaultFileQueries(hookCmd[0])));
        }


        void DoNotExpectAdditionalParameters(CommandData data)
        {
            var extracmds = NextOptionForArgument();
            if (extracmds.Count > 0)
            {
                data.InvalidComandLineSwitch = String.Format("The argument {0} was not expected.", extracmds[0]);
            }
        }

        static Dictionary<string, string> myShortToLong = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            {"cf","corflags"},
            {"df","diff"},
            {"gf","getfileinfo"},
            {"gp","getpdbs"},
            {"fi","fileinfo"},
            {"im","imbalance"},
            {"si","searchin"},
            {"tr","trace"},
            {"we","whousesevent"},
            {"wf","whousesfield"},
            {"wi","whoimplementsinterface"},
            {"wm","whousesmethod"},
            {"ws","whousesstring"},
            {"wt","whousestype"},
            {"wr","whoreferences"},
            {"sr","showreferences"},
            {"sn","showstrongname"},
            {"st","showrebuildtargets"},
            {"v","verbose"},
        };

        string TranslateShortToLong(string arg)
        {
            string ret = null;
            if (!myShortToLong.TryGetValue(arg, out ret))
            {
                ret = arg;
            }

            return ret;
        }

        public CommandData Parse(string[] args)
        {
            CommandData cmdArgs = new CommandData();

            myArgs = args;

            for (myParseIdx = 0; myParseIdx < args.Length; myParseIdx++)
            {
                string curArg = MakeArgLowerCase(args[myParseIdx]);
                List<string> hookCmd = null;

                curArg = TranslateShortToLong(curArg);
                switch (curArg)
                {

                    case "corflags":
                        cmdArgs.Command = Commands.CorFlags;
                        hookCmd = NextOptionForArgument();
                        if (hookCmd.Count == 0)
                        {
                            break;
                        }
                        AddQuery(cmdArgs.Queries1, cmdArgs, hookCmd);
                        break;
                    case "cwd":
                        var rootDir = NextOptionForArgument();
                        if (rootDir.Count == 0)
                        {
                            cmdArgs.InvalidComandLineSwitch = "cwd";
                            break;
                        }

                        cmdArgs.Cwd = rootDir[0];
                        break;

                    case "diff":
                        cmdArgs.Command = Commands.Diff;
                        DoNotExpectAdditionalParameters(cmdArgs);
                        break;

                    case "in":
                        AddQuery(cmdArgs.Queries2, cmdArgs);
                        break;
                    case "old":
                        AddQuery(cmdArgs.Queries1, cmdArgs);
                        break;
                    case "old2":
                        AddQuery(cmdArgs.OldFiles2, cmdArgs);
                        break;
                    case "searchin":
                        AddQuery(cmdArgs.SearchInQuery, cmdArgs);
                        break;
                    case "new":
                        AddQuery(cmdArgs.Queries2, cmdArgs);
                        break;
                    case "imbalance":
                        cmdArgs.EventSubscriptionImbalance = true;
                        break;
                    case "trace":
                        TracerConfig.Reset("console;* Level1");
                        break;
                    case WhoUsesMethodCommand.Argument:
                        cmdArgs.Command = Commands.MethodUsage;
                        hookCmd = NextOptionForArgument();
                        if (hookCmd.Count == 0)
                        {
                            break;
                        }

                        string potentialQuery = hookCmd[0];
                        if (potentialQuery.Contains("("))
                        {
                            cmdArgs.TypeAndInnerQuery = potentialQuery;
                        }
                        AddQuery(cmdArgs.Queries1, cmdArgs, hookCmd.GetRange(1, hookCmd.Count - 1));
                        break;
                    case WhoUsesTypeCommand.Argument:
                        cmdArgs.Command = Commands.WhoUsesType;
                        hookCmd = NextOptionForArgument();
                        if (hookCmd.Count == 0)
                        {
                            break;
                        }

                        cmdArgs.TypeQuery = hookCmd[0];
                        AddQuery(cmdArgs.Queries1, cmdArgs, hookCmd.GetRange(1, hookCmd.Count - 1));
                        break;
                    case WhoUsesFieldCommand.Argument:
                        cmdArgs.Command = Commands.WhoUsesField;
                        hookCmd = NextOptionForArgument();
                        if (hookCmd.Count == 0)
                        {
                            break;
                        }
                        cmdArgs.TypeAndInnerQuery = hookCmd[0];
                        AddQuery(cmdArgs.Queries1, cmdArgs, hookCmd.GetRange(1, hookCmd.Count - 1));
                        break;
                    case WhoUsesEventCommand.Argument:
                        cmdArgs.Command = Commands.WhoUsesEvent;
                        hookCmd = NextOptionForArgument();
                        if (hookCmd.Count == 0)
                        {
                            break;
                        }
                        cmdArgs.TypeAndInnerQuery = hookCmd[0];
                        AddQuery(cmdArgs.Queries1, cmdArgs, hookCmd.GetRange(1, hookCmd.Count - 1));
                        break;
                    case WhoImplementsInterfaceCommand.Argument:
                        cmdArgs.Command = Commands.WhoImplementsInterface;
                        hookCmd = NextOptionForArgument();
                        if( hookCmd.Count == 0 )
                        {
                            break;
                        }

                        cmdArgs.TypeQuery = hookCmd[0];
                        AddQuery(cmdArgs.Queries1, cmdArgs, hookCmd.GetRange(1, hookCmd.Count - 1));
                        break;
                    case WhoUsesStringConstantCommand.Argument:
                        cmdArgs.Command = Commands.WhoUsesStringConstant;
                        hookCmd = NextOptionForArgument();
                        if (hookCmd.Count == 0)
                        {
                            break;
                        }

                        cmdArgs.StringConstant = hookCmd[0];
                        if (hookCmd.Count > 1)
                        {
                            AddQuery(cmdArgs.Queries1, cmdArgs, hookCmd.GetRange(1, hookCmd.Count - 1));
                        }
                        break;
                    case "word":
                        cmdArgs.WordMatch = true;
                        break;
                    case "case":
                        cmdArgs.CaseSensitiveMatch = true;
                        break;
                    case GetFileInfoCommand.Argument:
                        cmdArgs.Command = Commands.GetFileInfo;
                        hookCmd = NextOptionForArgument();
                        if (hookCmd.Count == 0)
                        {
                            break;
                        }
                        AddQuery(cmdArgs.Queries1, cmdArgs, hookCmd);
                        break;
                    case "whoreferences":
                        cmdArgs.Command = Commands.WhoReferences;
                        hookCmd = NextOptionForArgument();
                        if (hookCmd.Count == 0)
                        {
                            break;
                        }
                        AddQuery(cmdArgs.Queries1, cmdArgs, hookCmd);
                        break;

                    case "getpdbs":
                        cmdArgs.Command = Commands.DownloadPdbs;
                        hookCmd = NextOptionForArgument();
                        if (hookCmd.Count == 0)
                        {
                            break;
                        }
                        AddQuery(cmdArgs.Queries1, cmdArgs, new List<string> { hookCmd[0] });
                        if (hookCmd.Count > 1)
                        {
                            cmdArgs.PdbDownloadDir = hookCmd[1];
                        }
                        if (hookCmd.Count > 2)
                        {
                            cmdArgs.InvalidComandLineSwitch = String.Format("The argument {0} is not valid for this command", hookCmd[2]);
                        }
                        break;
                    case "showrebuildtargets":
                        cmdArgs.Command = Commands.ShowRebuildTargets;
                        DoNotExpectAdditionalParameters(cmdArgs);
                        break;
                    case "showstrongname":
                        cmdArgs.Command = Commands.ShowStrongName;
                        AddQuery(cmdArgs.Queries1, cmdArgs);
                        break;
                    case "showreferences":
                        cmdArgs.Command = Commands.ShowReferences;
                        AddQuery(cmdArgs.Queries1, cmdArgs);
                        break;
                    case "verbose":
                        cmdArgs.Verbose = true;
                        break;
                    case "excel":
                        cmdArgs.OutputToExcel = true;
                        hookCmd = NextOptionForArgument();
                        if (hookCmd.Count == 0)
                        {
                            break;
                        }
                        cmdArgs.ExcelOutputFileName = hookCmd[0];
                        break;
                    case "threads":
                        hookCmd = NextOptionForArgument();
                        if (hookCmd.Count == 0)
                        {
                            break;
                        }
                        int newThreadCount =0;
                        if (int.TryParse(hookCmd[0], out newThreadCount) && newThreadCount > 0)
                        {
                            cmdArgs.ThreadCount = newThreadCount;
                        }
                        else
                        {
                            cmdArgs.InvalidComandLineSwitch = hookCmd[0];
                        }
                        break;
                    case "fileinfo":
                        cmdArgs.WithFileInfo = true;
                        break;
                    default:
                        cmdArgs.UnknownCommandLineSwitch = curArg;
                        break;
                }
            }

            return cmdArgs;
        }
    }
}
