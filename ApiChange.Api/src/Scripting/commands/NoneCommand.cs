
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiChange.Api.Scripting
{
    class NoneCommand : CommandBase
    {
        #region ICommandeLineAction Members

        public NoneCommand(CommandData parsedArgs)
            : base(parsedArgs)
        {
        }

        protected override void Validate()
        {
            base.Validate();

            if (myParsedArgs.OutputToExcel)
            {
                AddErrorMessage("Excel output is not supported by this comand");
                SetInvalid();
            }
        }

        public override void Execute()
        {
            base.Execute();
            Help();
        }

        #endregion
    }
}
