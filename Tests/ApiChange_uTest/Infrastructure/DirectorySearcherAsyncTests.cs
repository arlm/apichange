
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ApiChange.Api.Scripting;
using System.IO;
using System.Diagnostics;
using ApiChange.Infrastructure;

namespace UnitTests.Infrastructure
{
    [TestFixture]
    public class DirectorySearcherAsyncTests
    {
        [Test]
        public void Fail_When_Directory_Does_Not_Exist()
        {
            Assert.Throws<DirectoryNotFoundException>( ()=> new DirectorySearcherAsync("c:\\NotExistingDir","*") );
        }

        [Test]
        public void Fail_When_SearchPath_Is_Null()
        {
            Assert.Throws<ArgumentNullException>(() => new DirectorySearcherAsync(null, "*"));
        }

        [Test]
        public void Fail_When_SearchPattern_Is_Null()
        {
            Assert.Throws<ArgumentNullException>(() => new DirectorySearcherAsync("C:\\", null));
        }

        [Test]
        public void Can_Find_All_CS_Files_Recursively()
        {
            DirectorySearcherAsync searcher = new DirectorySearcherAsync(TestConstants.SolutionRootDir, "*.cs", SearchOption.AllDirectories);
            // do some warmup call
            string[] files = Directory.GetFiles(TestConstants.SolutionRootDir, "*.cs", SearchOption.AllDirectories);
            Stopwatch w = Stopwatch.StartNew();
            files = Directory.GetFiles(TestConstants.SolutionRootDir, "*.cs", SearchOption.AllDirectories);
            w.Stop();

            long firstFileMs = -1;
            List<string> asyncfiles = new List<string>();
            Stopwatch async = Stopwatch.StartNew();
            foreach (string file in searcher)
            {
                if( firstFileMs == -1)
                    firstFileMs = async.ElapsedMilliseconds;
                asyncfiles.Add(file);
            }
            async.Stop();
            Console.WriteLine("Directory.GetFiles did take {0}ms. Async {1}ms, Async/Directory = {2}, FirstFile = {3}ms",
                w.ElapsedMilliseconds, async.ElapsedMilliseconds, async.ElapsedMilliseconds / w.ElapsedMilliseconds,
                firstFileMs);
            Assert.AreEqual(files.Length, asyncfiles.Count, "Mismatch in fould file count");
        }
    }
}
