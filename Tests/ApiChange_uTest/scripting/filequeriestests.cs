
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ApiChange.Api.Scripting;
using System.IO;
using ApiChange.Infrastructure;

namespace UnitTests.Infrastructure
{
    [TestFixture]
    public class FileQueriesTests
    {
        [Test]
        public void Can_Search_In_More_Than_One_Dir()
        {
            List<FileQuery> queries = FileQuery.ParseQueryList(@"%WINDIR%\*.exe;%COMSPEC%",null);
            Assert.AreEqual(2, queries.Count);
            Assert.Greater(queries[0].Files.Length, 10);
            Assert.AreEqual(1, queries[1].Files.Length);
        }

        [Test]
        public void Do_Ignore_Empty_Queries()
        {
            var list = FileQuery.ParseQueryList(";;",null);
            Assert.AreEqual(0, list.Count);
        }

        [Test]
        public void When_No_Directory_Is_Given_Use_Cwd()
        {
            string cwd = Directory.GetCurrentDirectory();
            try
            {
                Directory.SetCurrentDirectory(Environment.GetEnvironmentVariable("WINDIR"));
                FileQuery query = new FileQuery("winhelp.exe");
                Assert.AreEqual(1, query.Files.Length);
                StringAssert.EndsWith("winhelp.exe", query.Files[0].ToLower());
            }
            finally
            {
               Directory.SetCurrentDirectory(cwd);
            }
        }

        [Test]
        public void Relative_FileQueries_Are_Expanded()
        {
            FileQuery query = new FileQuery(@"%WINDIR%\system32\..\taskman.exe");
            StringAssert.EndsWith(Environment.GetEnvironmentVariable("WINDIR"), query.SearchDir);
        }

        [Test]
        public void Can_Search_In_Directories_Relative_To_Cwd()
        {
            string cwd = Directory.GetCurrentDirectory();
            try
            {
                Directory.SetCurrentDirectory(Environment.GetEnvironmentVariable("WINDIR"));

                FileQuery query = new FileQuery(@"system32\..\winhelp.exe");
                query.UseCwd = true;
                Assert.AreEqual(1, query.Files.Length, "Did not find winhelp.exe with a path relative to the current working directory");
                StringAssert.Contains("winhelp.exe", query.Files[0].ToLower(), "Wrong file returned");

            }
            finally
            {
                Directory.SetCurrentDirectory(cwd);
            }
        }

        [Test]
        public void Can_Search_In_Gac()
        {
            List<FileQuery> queries = FileQuery.ParseQueryList("GAC:\\mscorlib.dll");
            Assert.AreEqual(1, queries.GetFiles().Count());
        }

        [Test]
        public void Do_Not_Throw_When_Assembly_Is_Not_In_GAC()
        {
            FileQuery query = new FileQuery("GAC:\\systembla.dll");
            Assert.AreEqual(0, query.Files.Length);
        }

        [Test]
        public void Do_Throw_When_Not_Existing_Directory_Is_Queried()
        {
            FileQuery query = new FileQuery(@"C:\bladdde3id\*.dll");
            Assert.Throws<DirectoryNotFoundException>( () => query.Files.ToString() );
        }

        [Test]
        public void Do_Not_Throw_When_Invalid_File_Name_Is_Entered()
        {
            FileQuery query = new FileQuery(@"%temp%\");
            query.Files.ToString();
        }

        [Test]
        public void All_Files_From_All_Queries_Must_Be_Returned()
        {
            FileQuery query = new FileQuery(@"%WINDIR%\*.*");
            const int QueryRepeatCount = 20;

            var queryList = Enumerable.Repeat(query,QueryRepeatCount).ToList();

            string[] tmpFiles = Directory.GetFiles(Environment.GetEnvironmentVariable("WINDIR"), "*.*");
            int tmpFileCount = tmpFiles.Length * QueryRepeatCount;


            int fileCount = 0;
            foreach(string file in queryList.GetFiles())
            {
                Assert.IsNotNull(file);
                fileCount++;
            }

            Assert.AreEqual(tmpFileCount, fileCount);
        }
    }
}
