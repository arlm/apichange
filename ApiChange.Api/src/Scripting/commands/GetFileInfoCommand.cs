
using System.Collections.Generic;
using System.Linq;
using ApiChange.ExternalData;
using ApiChange.Infrastructure;

namespace ApiChange.Api.Scripting
{
    class GetFileInfoCommand : CommandBase
    {
        public const string Argument = "getfileinfo";

        SheetInfo myOutputHeader= new SheetInfo
        {
            Columns = new List<ColumnInfo>
            {
                new ColumnInfo { Name = "File Name", Width = SourceFileWidth },
                new ColumnInfo { Name = "User Name", Width = 25},
                new ColumnInfo { Name = "Mail", Width = 30},
                new ColumnInfo { Name = "Phone", Width = 20},
                new ColumnInfo { Name = "Department", Width = 25},
            },

            SheetName = "File Infos"
        };

        public GetFileInfoCommand(CommandData parsedData) :
            base(parsedData)
        {
        }

        protected override void Validate()
        {
            base.Validate();

            if (myParsedArgs.Queries1.Count == 0)
            {
                AddErrorMessage("Missing file name.");
                SetInvalid();
            }

            if (myParsedArgs.Queries1.GetFiles().Count() == 0)
            {
                AddErrorMessage("The file {0} was not found. Cannot get info.", myParsedArgs.Queries1.GetQueries());
                ShowFullHelp = false;
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

            ClearCaseToADMapper prov = new ClearCaseToADMapper();

            Writer.SetCurrentSheet(myOutputHeader);

            foreach(string file in myParsedArgs.Queries1.GetFiles())
            {
                UserInfo infos = prov.GetInformationFromFile(file);

                if (infos == null)
                {
                    Out.WriteLine("Error: Could not get file infos for file {0}", file);
                    return;
                }

                Writer.PrintRow("{0}; {1}; {2}; {3}; {4}", 
                    null,
                    file, 
                    infos.DisplayName, 
                    infos.Mail,
                    infos.Phone, 
                    infos.Department);
            }
        }
    }
}
