
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using Microsoft.Office.Interop.Excel;
using System.Threading;
using ApiChange.Infrastructure;

namespace ApiChange.Api.Scripting
{
    class ExcelOutputWriter : IOutputWriter
    {
        static TypeHandle myType = new TypeHandle(typeof(ExcelOutputWriter));

        internal const string myColIdx = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        internal protected ApplicationClass myExcel = new ApplicationClass();
        internal protected Workbook myWorkbook;
        internal protected string myExcelFileName;

        // flag to signal if the default sheets have already been removed.
        internal protected bool bRemovedDefaultSheets = false;

        bool mybCloseOnDispose;

        internal SheetInfo myCurrentSheetInfo;
        internal Worksheet myCurrentSheet;
        internal int myRowIndex = 1;

        internal int myTotalLinesWritten = 0;
        internal int myLinesWritten = 0;

        /// Helper object for asynchronous processing
        internal AsyncWriter<List<string>> myWriter;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExcelOutputWriter"/> class.
        /// </summary>
        /// <param name="outFileName">Name of the out file. If null no excel file will be created</param>
        /// <param name="options">Excel startup options</param>
        public ExcelOutputWriter(string outFileName, ExcelOptions options)
        {
            using (Tracer t = new Tracer(myType, "ExcelOutputWriter"))
            {
                mybCloseOnDispose = ((options & ExcelOptions.CloseOnExit) == ExcelOptions.CloseOnExit);
                myExcelFileName = outFileName;
                myExcel.DisplayAlerts = false;
                myExcel.Visible = ((options & ExcelOptions.Visible) == ExcelOptions.Visible);
                myWorkbook = myExcel.Workbooks.Add(1);
                t.Info("CloseOnDispose: {0}, OutFileName: {1}, Visible: {2}", mybCloseOnDispose, outFileName, myExcel.Visible);

                myWriter = new AsyncWriter<List<string>>(WriteLine);
            }
        }

        /// <summary>
        /// Called by users of our Interface to request to print one line
        /// </summary>
        /// <param name="fmtString"></param>
        /// <param name="additionalColumnDataProvider"></param>
        /// <param name="args"></param>
        public void PrintRow(string fmtString,Func<List<string>> additionalColumnDataProvider, params object[] args)
        {
            lock (this)
            {
                if (fmtString == "")
                    return;

                if (args == null || args.Length == 0)
                    throw new ArgumentException("No row data was provided");

                myTotalLinesWritten++;

                List<string> columnData = new List<string>();
                
                foreach(object arg in args)
                {
                    columnData.Add(arg.ToString());
                }


                if (additionalColumnDataProvider != null)
                {
                    List<string> additionalCols = additionalColumnDataProvider();
                    if (additionalCols != null)
                    {
                        columnData.AddRange(additionalCols);
                    }
                }

                myWriter.Write(columnData);
            }
        }

        /// <summary>
        /// Does the actual writing to excel on another thread via AsyncWriter.
        /// </summary>
        /// <param name="columns"></param>
        void WriteLine(List<string> columns)
        {
            lock (this)
            {
                if (columns != null)
                {
                    int lineNumber = myRowIndex + myCurrentSheetInfo.HeaderRow;

                    SetRowData(columns, lineNumber);

                    int sourceFileIdx = myCurrentSheetInfo.Columns.FindIndex((colInfo) => colInfo.Name == CommandBase.SourceFileCol);
                    // If we have a source file column we add a hyperlink to it.
                    if (sourceFileIdx != -1)
                    {
                        Range cell = GetCell(myCurrentSheet, sourceFileIdx, lineNumber);
                        string content = (string)cell.Value2;
                        if (!String.IsNullOrEmpty(content))
                            myCurrentSheet.Hyperlinks.Add(cell, content, Type.Missing, "", content);
                    }

                    myRowIndex++;
                }

                Interlocked.Increment(ref myLinesWritten);
            }
        }

        void SetRowData(List<string> data, int row)
        {
            string rowRange = String.Format("A{0}:{1}{2}", row, myColIdx[data.Count-1], row);
            Range r = myCurrentSheet.get_Range(rowRange, Type.Missing);

            object[,] rowData = new object[1, data.Count];
            for (int i = 0; i < data.Count; i++)
            {
                rowData[0, i] = data[i];
            }

            r.Value2 = rowData;
        }

        string StripTooBigContent(string value)
        {
            string ret = value;
            if (ret.Length > 1024)
            {
                ret = ret.Substring(0, 1024);
            }

            return ret;
        }

        public void SetCurrentSheet(SheetInfo header)
        {
            using (Tracer t = new Tracer(myType, "SetCurrentSheet"))
            {
                if (myCurrentSheet != null)
                {
                    t.Info("Wait until previous sheet data has been written");
                    myWriter.Close();
                    t.Info("Wait finished");
                    myWriter = new AsyncWriter<List<string>>(this.WriteLine);
                }

                lock (this)
                {
                    if (myCurrentSheet != null)
                    {
                        EnableAutoFilter();
                    }
                    myCurrentSheetInfo = header;
                    myCurrentSheet = AddSheet(header.SheetName);
                    myRowIndex = 1;

                    for (int i = 0; i < header.Columns.Count; i++)
                    {
                        var colum = header.Columns[i];
                        Range r = GetCell(myCurrentSheet, i, header.HeaderRow);
                        r.Value2 = colum.Name;
                        r.ColumnWidth = colum.Width;
                    }

                    SetHeaderFont(header);
                }
            }
        }

        public Worksheet GetWorkSheet(string sheetName)
        {
            return (Worksheet)myWorkbook.Worksheets.get_Item(sheetName);
        }

        public Worksheet AddSheet(string sheetName)
        {
            using (Tracer t = new Tracer(myType, "AddSheet"))
            {
                Worksheet sheet = (Worksheet)myWorkbook.Worksheets.Add(Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                sheet.Name = sheetName;
                return sheet;
            }
        }

        public void EnableAutoFilter()
        {
            if( myCurrentSheet != null )
            {
                ((_Workbook)myWorkbook).Activate();
                Range header = GetRange(myCurrentSheet, 0, myCurrentSheetInfo.HeaderRow, myCurrentSheetInfo.Columns.Count-1, myCurrentSheetInfo.HeaderRow+myRowIndex);
                // it is important to fill for the unkown things Type.Missing to excel otherwise all rows are
                // hidden!
                header.AutoFilter(1, Type.Missing, XlAutoFilterOperator.xlAnd, Type.Missing, true);
            }
        }

        public Range GetCell(Worksheet sheet, string cell)
        {
            return sheet.get_Range(cell, Type.Missing);
        }

        public Range GetCell(Worksheet sheet, int x, int y)
        {
            return GetCell(sheet, myColIdx[x].ToString() + y.ToString());
        }

        public Range GetRange(Worksheet sheet, int x1, int y1, int x2, int y2)
        {
            return sheet.get_Range(myColIdx[x1].ToString() + y1.ToString(), myColIdx[x2].ToString() + y2.ToString());
        }

        protected void SetHeaderFont(SheetInfo header)
        {
            Range r = GetRange(myCurrentSheet, 0, header.HeaderRow, header.Columns.Count - 1, header.HeaderRow);
            r.Interior.ColorIndex = 0x2d; // orange
            r.Interior.Pattern = XlPattern.xlPatternSolid;
            r.WrapText = true;
            r.Font.Size = 14;
            r.Font.Bold = true;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public virtual void Dispose(bool isDisposing)
        {
            if (myExcel != null)
            {
                myWriter.Dispose();
                myWriter = null;

                EnableAutoFilter();
                Save();
                myWorkbook = null;

                // Close excel when nothing has been written.
                if (mybCloseOnDispose || myTotalLinesWritten == 0)
                {
                    myExcel.Quit();
                }

                myExcel = null;
            }
        }

        public virtual void Save()
        {
            if (myWorkbook != null && !String.IsNullOrEmpty(myExcelFileName))
            {
                DeleteOldExcelFile();
                this.myWorkbook.SaveAs(myExcelFileName, XlFileFormat.xlWorkbookNormal, "", "", false, false, XlSaveAsAccessMode.xlExclusive, XlSaveConflictResolution.xlLocalSessionChanges, false, 0, 0, 0);
            }
        }

        void DeleteOldExcelFile()
        {
            // delete old excel file if existing
            if (!String.IsNullOrEmpty(myExcelFileName))
            {
                if (File.Exists(myExcelFileName))
                {
                    try
                    {
                        File.Delete(myExcelFileName);
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceInformation("Could not delete existing excel file {0}. Reason {1}", myExcelFileName, ex);
                    }
                }
            }
        }

    }
}
