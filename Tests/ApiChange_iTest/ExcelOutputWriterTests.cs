#region File Header
/*[ Compilation unit ----------------------------------------------------------
    Component       : syngo
    Name            : ExcelOutputWriterTests.cs
    Last Author     : Alois Kraus
    Language        : C#
    Creation Date   : 06. November 2009
    Description     : 

     Copyright (C) Siemens AG 2009-2010 All Rights Reserved
-----------------------------------------------------------------------------*/
/*] END */
#endregion
using System;
using System.Collections.Generic;
using NUnit.Framework;
using ApiChange.Api.Scripting;
using System.Threading;
using Microsoft.Office.Interop.Excel;
using System.Globalization;

namespace IntegrationTests.Output
{
    [TestFixture]
    public class ExcelOutputWriterTests
    {
        const ExcelOptions DefaultExcelOption = ExcelOptions.CloseOnExit; /*|ExcelOptions.Visible*/

        SheetInfo mySearchHeader = new SheetInfo
        {
            Columns = new List<ColumnInfo>
            {
                new ColumnInfo { Name = "Type", Width = 80 },
                new ColumnInfo { Name = "Field", Width = 40 },
                new ColumnInfo { Name = "Assembly", Width = 60},
                new ColumnInfo { Name = CommandBase.SourceFileCol, Width = 100}
            },

            SheetName = "Search Fields"
        };

        [TearDown]
        public void Close_Excel_Instances()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        [Test]
        public void Can_Add_WorkSheet()
        {
            using (ExcelOutputWriter writer = new ExcelOutputWriter("Test.xls", DefaultExcelOption))
            {
                writer.AddSheet("Test Sheet");
            }
        }

        [Test]
        public void Can_Add_WorkSheet_And_Columns()
        {
            using (ExcelOutputWriter writer = new ExcelOutputWriter("Test.xls", DefaultExcelOption))
            {
                writer.SetCurrentSheet(mySearchHeader);
                Assert.AreEqual("Type", writer.GetCell(writer.myCurrentSheet, "A4").Value2.ToString(), "Expected addtional content 1");
                Assert.AreEqual("Field", writer.GetCell(writer.myCurrentSheet, "B4").Value2.ToString(), "Expected addtional content 2");
            }
        }

        [Test]
        public void Can_Add_Data_To_WorkSheet()
        {
            using (ExcelOutputWriter writer = new ExcelOutputWriter("Test.xls", DefaultExcelOption))
            {
                writer.SetCurrentSheet(mySearchHeader);
                writer.PrintRow("none", null, "field1", "field2", "field3", "field4");
                writer.PrintRow("none", () => null, "field1", "field2", "field3", "field4");
                writer.PrintRow("none", () => new List<string>(), "field1", "field2", "field3", "field4");
                writer.PrintRow("none", null, "field1", "field2", "field3", "field4");

                while (writer.myLinesWritten != 4) Thread.Sleep(50);

                Assert.AreEqual("Type", writer.GetCell(writer.myCurrentSheet, "A4").Value2.ToString(), "Expected addtional content 1");
                Assert.AreEqual("Field", writer.GetCell(writer.myCurrentSheet, "B4").Value2.ToString(), "Expected addtional content 2");

                Assert.AreEqual("field1", writer.GetCell(writer.myCurrentSheet, "A5").Value2.ToString(), "Expected addtional content 1");
                Assert.AreEqual("field1", writer.GetCell(writer.myCurrentSheet, "A8").Value2.ToString(), "Expected addtional content 2");

                Assert.AreEqual("field4", writer.GetCell(writer.myCurrentSheet, "D5").Value2.ToString(), "Expected addtional content 1");
                Assert.AreEqual("field4", writer.GetCell(writer.myCurrentSheet, "D8").Value2.ToString(), "Expected addtional content 2");
            }
        }

        [Test]
        public void Can_Add_Additional_Columns()
        {
            using (ExcelOutputWriter writer = new ExcelOutputWriter("Test.xls", DefaultExcelOption))
            {
                string ExtCol1 = "ext1";
                string ExtCol2 = "ext2";

                writer.SetCurrentSheet(mySearchHeader);
                writer.PrintRow("none", () => new List<string> { ExtCol1, ExtCol2 }, "field1", "field2", "field3", "field4");
                while (writer.myLinesWritten == 0) Thread.Sleep(50);

                Assert.AreEqual(ExtCol1, writer.GetCell(writer.myCurrentSheet, "E5").Value2.ToString(), "Expected addtional content 1");
                Assert.AreEqual(ExtCol2, writer.GetCell(writer.myCurrentSheet, "F5").Value2.ToString(), "Expected addtional content 2");
            }
        }

        [Test]
        public void Can_Write_ToSheet_From_Many_Threads()
        {
            Worksheet currentSheet = null;
            ApplicationClass excel = null;

            using (ExcelOutputWriter writer = new ExcelOutputWriter(null, ExcelOptions.None))
            {
                string ExtCol1 = "ext1";
                string ExtCol2 = "ext2";

                writer.SetCurrentSheet(mySearchHeader);
                currentSheet = writer.myCurrentSheet;

                excel = writer.myExcel;

                int LinesToWrite = 30;

                System.Action acc = () =>
                    {
                        while (true)
                        {
                            if (LinesToWrite <= 0)
                                break;

                            writer.PrintRow("none", () => new List<string> { ExtCol1, ExtCol2 },
                                Thread.CurrentThread.ManagedThreadId.ToString(), LinesToWrite.ToString(), "field3", "field4");

                            Interlocked.Decrement(ref LinesToWrite);
                        }
                    };

                for (int i = 0; i < 5; i++)
                {
                    acc.BeginInvoke(null, null);
                }
                while (LinesToWrite > 0) Thread.Sleep(50);
                Console.WriteLine("Did enqeue all items");
            }

            Assert.IsNotNull(currentSheet.get_Range("B34", Type.Missing).Value2);
            excel.Quit();

        }

        [Test]
        public void Can_WriteSheets_With_No_SourceFile_Column()
        {
            SheetInfo noSourceFilecol = new SheetInfo
            {
                Columns = new List<ColumnInfo>
            {
                new ColumnInfo { Name = "Type", Width = 80 },
                new ColumnInfo { Name = "Assembly", Width = 60},
            },

                SheetName = "Test Sheet"
            };


            using (ExcelOutputWriter writer = new ExcelOutputWriter(null, DefaultExcelOption))
            {
                writer.SetCurrentSheet(noSourceFilecol);
                writer.PrintRow("{0} {1}", null, "Col1", "Col2");
            }
        }

        [Test]
        public void Can_Execute_Excel_From_Non_US_Thread()
        {
            CultureInfo old = Thread.CurrentThread.CurrentCulture;
            try
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;


                SheetInfo noSourceFilecol = new SheetInfo
                {
                    Columns = new List<ColumnInfo>
                    {
                        new ColumnInfo { Name = "Type", Width = 80 },
                        new ColumnInfo { Name = "Assembly", Width = 60},
                    },

                    SheetName = "Test Sheet"
                };


                using (ExcelOutputWriter writer = new ExcelOutputWriter(null, DefaultExcelOption))
                {
                    writer.SetCurrentSheet(noSourceFilecol);
                    writer.PrintRow("{0} {1}", null, "Col1", "Col2");
                }

            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = old;
            }

        }
    }
}
