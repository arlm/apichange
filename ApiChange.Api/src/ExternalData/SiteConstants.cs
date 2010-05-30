
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ApiChange.ExternalData
{
    internal static class SiteConstants
    {
        // To query your Active directory you need to add
        // your local domain aaa.bbb.com into the form
        // LDAP://DC=aaa,DC=bbb,DC=com
        public const string ADQuery = "";

        /// <summary>
        /// Global Catalog query which uses the fast Active Directory Cache
        /// If you login domain is testDomain it is of the form
        /// GC://testDomain
        /// </summary>
        public const string GCQuery = "";

        static readonly string Net2Dir = Path.Combine(Environment.GetEnvironmentVariable("WINDIR"),
                                                 @"Microsoft.NET\Framework\v2.0.50727");
        static readonly string Net3Dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                                @"Reference Assemblies\Microsoft\Framework\v3.0");
        static readonly string Net35Dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                                @"Reference Assemblies\Microsoft\Framework\v3.5");
        static readonly string Net4Dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                             @"Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0");

        public static readonly Dictionary<string, string> DefaultFileQueryReplacements = new Dictionary<string, string>
        {
            {"$net2dir", Net2Dir},
            {"$net3dir", Net3Dir},
            {"$net35dir", Net35Dir},
            {"$net4dir", Net4Dir}
        };

        public static readonly Dictionary<string, string> DefaultFileQueryReplacements2 = new Dictionary<string, string>
        {
            {"$net2",  Path.Combine(Net2Dir,"*.dll") +";"+
                       Path.Combine(Net3Dir,"*.dll") +";"+
                       Path.Combine(Net35Dir,"*.dll")
            },
            {"$net4",  Path.Combine(Net4Dir, "*.dll")},
        };

        public const string DefaultSymbolServer = @"http://msdl.microsoft.com/download/symbols";
        public const string DefaultSymbolServerLocation = "Microsoft";

        public const int DefaultThreadCount = 7;

        public readonly static string HelpStr =
             "ApiChange.exe (c) by Alois Kraus" + Environment.NewLine +
             "ApiChange -ShowrebuildTargets -new <file/s> -old <file/s>| -diff -new <file/s> -old <file/s>| -whousesmethod <file/s> -in <file/s>]| -whoreferences <file/s> -in <file/s>| -getpdbs <file/s>  -cwd <baseDir> ..." + Environment.NewLine +
             "Options: " + Environment.NewLine +
             "   Note: The big letters in the options are the shortcut names. E.g. -DifF is equivalent to -df or -diff" + Environment.NewLine +
             "   -DifF -old <filesV1> -new <filesV2> " + Environment.NewLine +
             "                           Do a binary compare of each assembly. Only public types are diffed." + Environment.NewLine + Environment.NewLine +
             "   -WhousesMethod <typemethodquery> <declaring file/s> -in <using file/s>" + Environment.NewLine +
             "                           Find matching methods from the files after the query. Then search for their method usage in the -in <using file/s>." + Environment.NewLine +
             "                           When pdbs are present also the source and file line is printed out. See -getpdbs command." + Environment.NewLine +
             "                           <typemethodquery> is of the form \"searchType(methodquery)\". E.g. *(...) searches for all types, String*(...) searches for all classes which begin with String as typename." + Environment.NewLine +
             "                           public string Method(int arg, List<string> b) match exact method including parameter names. public * *(*) search for all public methods, !public *(*) search for all not public methods" + Environment.NewLine +
             "   -WhoimplementsInterface <typequery> <defining interface file/s> -in <impl file/s>" + Environment.NewLine +
             "                           Search in the files for the interface defined by typequery and searches in the -in <impl file/s> for all implementers of the given interface." + Environment.NewLine +
             "   -WhousesType <typequery> <defing type file/s> -in <using file/s>" + Environment.NewLine +
             "                           Search in the files for the type and then looks where the type/s are used in the files of the -in clause." + Environment.NewLine +
             "                           Type usage means that the type is used as base type/interface, generic argument to a generic base type/interface, method from the type is called, field from the type is accessed, type is used as field type or " + Environment.NewLine +
             "                           part of a method declaration (return type, method arguments including generic parameters). It searches for the type also in function local variables." + Environment.NewLine +
             "   -WhousesField <typefieldquery> <defining file/s> -in <using file/s>" + Environment.NewLine +
             "                           Search for all read and assignment calls for the given field." + Environment.NewLine +
             "                           The query format is similar to the whousesmethod query. EnclosingType(field declaration) e.g. *(string *) searches in all classes for string fields and checks who is using them." + Environment.NewLine +
             "   -WhousesEvent <typeeventquery> <defining event file/s> -in <using file/s> [-imbalance]" + Environment.NewLine +
             "                           Search for all event subscriber and unsubscribers. If -imbalance is added a summary of all imbalanced event un/subscriptions is printed out." + Environment.NewLine +
             "                           The query format is EnclosingType(event declaration). E.g. *(*) searches for all events in all types, *(public * *) searches for all public event of any type and name." + Environment.NewLine +
             "   -WhousesString \"substring\" [defining assemblies] -in <using file/s> [-word] [-case]" + Environment.NewLine +
             "                           Search for users of string constants. The defining assemblies list is optional to speedup the query to search only in assemblies which reference the " + Environment.NewLine +
             "                           defining assemblies. It is not possible to find out who uses a string constant directly but it is possible to search in string constants for substrings which could have been " + Environment.NewLine +
             "                           constructed of other string constants. If the string is sufficiently unique you can find" + Environment.NewLine +
             "                           all users of your string constant by using this method." + Environment.NewLine +
             "   -WhoReferences <file> -in <file/s>" + Environment.NewLine +
             "                           Show all dlls which reference the file." + Environment.NewLine + Environment.NewLine +
             "   -ShowrebuildTargets -new <file/s> -old <file/s> [-old2 <file/s>] -searchin <file/s>" + Environment.NewLine +
             "                           Diff the new files old versus old files. If breaking changes are found serach in all -searchin files for affected targets which should be rebuilt." + Environment.NewLine +
             "                           The file to compare is first searched in the -old file list and then in the -old2 file list." + Environment.NewLine +
             "   -GetFileinfo <file/s>   " + Environment.NewLine +
             "                           Get from the file check in user, email, phone and department." + Environment.NewLine +
             "   -GetPdbs <file/s> [PdbStoreDirectory]" + Environment.NewLine + String.Format(
             "                           Get pdbs from symbol server. If the pdb store directory is omitted the pdbs are downloaded beside the binary. Default is {0} ({1}). " + Environment.NewLine +
             "                           You need to set the environment variable %SYMSERVER% to use a different one.",DefaultSymbolServerLocation,  DefaultSymbolServer) + Environment.NewLine +
             "   -ShowstrongName <file/s>" + Environment.NewLine +
             "                           Display the strong name of the assembly files" + Environment.NewLine +
             "   -ShowReferences <file/s>" + Environment.NewLine +
             "                           Display all referenced assemblies from <file/s>." + Environment.NewLine +
             "   -CorFlags <file/s>      Display same data as the CorFlags tool (32/64 bit, signed, pure IL, ...) " + Environment.NewLine +
             "   [-excel [output file]]  Optional: Redirect the output to an excel sheet instead to the console. If the output file name is omitted excel will display the results directly." + Environment.NewLine +
             "   [-cwd <baseDir>]        Optional: Set the current working directory for the file queries." + Environment.NewLine + String.Format(
             "   [-threads <nn>]         Optional: Execute the query with multiple threads. Default is: {0}. In case of hangs it is useful to set it to 1.", DefaultThreadCount) + Environment.NewLine +
             "   [-TRace]                Optional: Print traces to the console. Used for debugging." + Environment.NewLine + Environment.NewLine +
             " Type Queries: " + Environment.NewLine +
             "   <typequery>             A type query can be a simple * or a full C# type declaration with placeholders. E.g. \"public interface *;public class System.Diag*.*\" is a valid type query." + Environment.NewLine +
             " File Queries: " + Environment.NewLine +
             "   <file/s>                Is a file query which can contain wildcards, absolute and relative path names as well as the GAC as search location. Multiple queries are separated with a ;" + Environment.NewLine +
            @"                           E.g. *.dll;*.exe or Dll\*.dll;Bin\*.exe or GAC:\mscorlib.dll or C:\MySearchDir\myfile.dll" + Environment.NewLine +
             "   $net2 = " + DefaultFileQueryReplacements2["$net2"] + Environment.NewLine +
             "   $net4 = " + DefaultFileQueryReplacements2["$net4"] + Environment.NewLine +
             "   $net2dir = " + DefaultFileQueryReplacements["$net2dir"] + Environment.NewLine +
             "   $net3dir = " + DefaultFileQueryReplacements["$net3dir"] + Environment.NewLine +
             "   $net35dir = " + DefaultFileQueryReplacements["$net35dir"] + Environment.NewLine +
             "   $net4dir " + DefaultFileQueryReplacements["$net4dir"] + Environment.NewLine +
             " Environment Variables: " + Environment.NewLine +
             "    SYMSERVER     Set an alternative symbol server to retrieve the pdbs with the -getpdbs command." + Environment.NewLine +
             " Examples: " + Environment.NewLine +
             " Download the pdbs for the given binaries from " + DefaultSymbolServerLocation + "symbol server." + Environment.NewLine +
             "     ApiChange -getpdbs $net2 c:\\net2pdbs" + Environment.NewLine +
             " Check for breaking changes from .Net 2 to .Net 4 in mscorlib.dll" + Environment.NewLine +
             "    ApiChange -diff -old $net2dir\\System.dll -new $net4dir\\System.dll" + Environment.NewLine +
             " Who is using the Stopwatch Seek Method within the .Net Framework?" + Environment.NewLine +
             "    ApiChange -whousesmethod \"Stopwatch(* Start())\" $net2dir\\System.dll -in $net2 -excel" + Environment.NewLine +
             " Who references System.dll" + Environment.NewLine +
             "    ApiChange -whoreferences $net2dir\\System.dll -in $net2" + Environment.NewLine +
             " Who references System.Configuration.dll located in the GAC." + Environment.NewLine +
            @"    ApiChange -whoreferences gac:\system.configuration.dll -in C:\Windows\Microsoft.NET\Framework\v2.0.50727\*.dll" + Environment.NewLine +
             " Check after building a dll into the NewDll folder for breaking changes and list all affected targets which need potentially a rebuild." + Environment.NewLine +
             "    ApiChange -showrebuildtargets -new NewBuild\\*.dll -old v1.0\\*.dll -searchin v1.0\\*.dll -cwd \\ProjectDir" + Environment.NewLine +
             " Find all users of e.g an enum type. It can find practically all enum usage locatons (if,switch case, ...). Locations where the enum is casted to e.g. an int cannot be found (e.g. int v=(int) xxx) because the compiler assigns simply the value to it." + Environment.NewLine +
             "    ApiChange -whousestype \"public enum System.IO.FileShare\" $net2dir\\mscorlib.dll -in $net2 -excel" + Environment.NewLine +
             " Check who is using all non constant public fields. Note: Const fields are never used directly. The compiler embeds the const value directly into the target location which makes it impossible to track the usage of const values inside IL code. " + Environment.NewLine +
             " Instead a source code search should be performed. In case of enums you can track their usage with a -whousestype query to find all enum instance declarations." + Environment.NewLine +
             "    ApiChange -whousesfield \"*(public !const * *)\" $net2dir\\mscorlib.dll -in $net2 -excel" + Environment.NewLine +
             " Search for subscribers/unsubscribers from all public types with public events. To check for imbalances you can add the -imbalance switch." + Environment.NewLine +
             "    ApiChange -whousesevent \"public *(public event * *)\" GAC:\\System.Windows.Forms.dll -in $net2 -excel" + Environment.NewLine +
             " Search for users of the null string constant. It does search simply for null as substring (case insensitive) in all string constants in all files." + Environment.NewLine +
             "    ApiChange -ws null -in $net2 -excel" + Environment.NewLine +
             "";
    }
}
