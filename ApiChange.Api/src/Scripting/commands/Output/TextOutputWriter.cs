
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ApiChange.Api.Scripting
{
    class TextOutputWriter : IOutputWriter
    {
        CommandBase myCmd;
        public TextOutputWriter(CommandBase cmd)
        {
            myCmd = cmd;
        }

        #region IOutputWriter Members

        public void PrintRow(string fmtString,Func<List<string>> additionalColumnDataProvider, params object[] args)
        {
            lock (this)
            {

                myCmd.Out.Write(fmtString, args);
                if (additionalColumnDataProvider != null)
                {
                    List<string> additionalcols = additionalColumnDataProvider();
                    if (additionalcols != null)
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach(string col in additionalcols)
                        {
                            sb.Append("; ");
                            sb.Append(col);
                        }
                        myCmd.Out.Write(sb.ToString());
                    }
                }

                myCmd.Out.WriteLine();
            }
        }

        public void SetCurrentSheet(SheetInfo header)
        {
            lock (this)
            {
                StringBuilder headerline = new StringBuilder();
                foreach (var col in header.Columns)
                {
                    headerline.Append(col.Name);
                    headerline.Append("; ");
                }
                string line = headerline.ToString().TrimEnd(new char[] { ' ', ';' });
                myCmd.Out.WriteLine(line);
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
        }

        #endregion
    }
}
