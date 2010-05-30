
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiChange.Api.Introspection;
using Mono.Cecil;

namespace ApiChange.Api.Scripting
{
    class ShowStrongNameCommand : CommandBase
    {
        public ShowStrongNameCommand(CommandData parsedArgs) 
            : base(parsedArgs)
        {
        }

        protected override void Validate() 
        {
            base.Validate();

            ValidateFileQuery( myParsedArgs.Queries1,
                    "Command -showstrongname expects a file query to display their strong name.",
                    "Invalid directory in -showstrongname {0} query.",
                    "Command -showstrongname. The query {0} did not match any files.");

            if (myParsedArgs.OutputToExcel)
            {
                AddErrorMessage("Excel output is not supported by this comand");
                SetInvalid();
            }
        }

        public override void Execute()
        {
            base.Execute();
            if(!IsValid)
            {
                Help();
                return;
            }

            base.LoadAssemblies(myParsedArgs.Queries1, (assembly, fileName) =>
            {
                Out.WriteLine(assembly.Name.FullName);
            });
        }
    }
}
