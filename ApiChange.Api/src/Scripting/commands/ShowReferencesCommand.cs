
using System;
using Mono.Cecil;
using ApiChange.Api.Introspection;
using System.IO;
using System.Reflection;
using ApiChange.Infrastructure;

namespace ApiChange.Api.Scripting
{
    class ShowReferencesCommand : CommandBase
    {
        public ShowReferencesCommand(CommandData parsedArgs) 
            : base(parsedArgs)
        {
        }

        protected override void Validate()
        {
            base.Validate();

            ValidateFileQuery(myParsedArgs.Queries1,
                    "Command -showreferences expects at least one file to list its assembly references when it is a managed target.",
                    "Invalid directory in -showreferences {0} query.",
                    "Command -showreferences. The query {0} did not match any files.");

            if (myParsedArgs.OutputToExcel)
            {
                AddErrorMessage("Excel output is not supported by this comand.");
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

            foreach (string file in myParsedArgs.Queries1.GetFiles())
            {
                AssemblyDefinition assembly = AssemblyLoader.LoadCecilAssembly(file);
                if (assembly == null) // ignore unmanaged and MC++ targets
                    continue;
                Out.WriteLine("{0} ->", Path.GetFileName(file));
                foreach (ModuleDefinition mod in assembly.Modules)
                {
                    foreach (AssemblyNameReference reference in mod.AssemblyReferences)
                    {
                        // Mono Cecil has problems to identify assembly references
                        // which do contain the public key and hash value instead of
                        // the usual publickeytoken. Some ASP.NET assemblies seem to have such strange references
                        // It treats the publichkey as publickeytoken wrongly
                        if (reference.PublicKeyToken != null && reference.PublicKeyToken.Length == 160)
                        {
                                Out.WriteLine("\t{0}", PrintStrangeRef(reference));
                        }
                        else
                        {
                            Out.WriteLine("\t{0}", reference);
                        }
                    }
                }
            }
        }

        private string PrintStrangeRef(AssemblyNameReference reference)
        {
            string lret = "";
            AssemblyName name = new AssemblyName();
            name.Name = "t";
            name.SetPublicKey(reference.PublicKeyToken);
            string token = name.ToString();
            token = token.Substring(18, token.Length - 18);
            lret = String.Format("{0}, Version={1}, Culture={2}, PublicKeytoken={3}", reference.Name, reference.Version,
                reference.Culture == "" ? "neutral": reference.Culture, token);
            return lret;
        }
    }
}