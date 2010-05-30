
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace ApiChange.Infrastructure
{
    public static class Extensions
    {
        public static string GetSearchDirs(this List<FileQuery> queries)
        {
            if (queries == null)
            {
                throw new ArgumentNullException("queries was null.");
            }


            String ret = "";
            foreach (var q in queries)
            {
                ret += q.SearchDir + ";";
            }

            return ret.TrimEnd( new char [] { ';' });
        }

        public static string GetQueries(this List<FileQuery> queries)
        {
            if (queries == null)
            {
                throw new ArgumentNullException("queries was null.");
            }

            string lret = "";
            foreach (FileQuery q in queries)
            {
                lret += q.Query + " ";
            }
            return lret.Trim();
        }

        public static IEnumerable<string> GetFiles(this List<FileQuery> queries)
        {
            if (queries == null)
            {
                throw new ArgumentNullException("queries was null.");
            }

            foreach (var q in queries)
            {
                q.BeginSearch();
            }

            foreach (var q in queries)
            {
                foreach (var file in q.EnumerateFiles)
                {
                    yield return file;
                }
            }
        }

        public static bool HasMatches(this List<FileQuery> queries)
        {
            if (queries == null)
            {
                throw new ArgumentNullException("queries was null.");
            }

            bool lret = false;
            foreach (var q in queries)
            {
                lret = q.HasMatches;
                if (lret)
                    break;
            }

            return lret;
        }

        public static string GetMatchingFileByName(this List<FileQuery> queries, string fileName)
        {
            if (queries == null)
            {
                throw new ArgumentNullException("queries was null.");
            }

            if (String.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("fileName to filter for was null or empty");
            }

            string match = null;
            foreach (var q in queries)
            {
                match = q.GetMatchingFileByName(fileName);
                if (match != null)
                    break;
            }

            return match;
        }

        public static List<string> GetNotExistingFilesInOtherQuery(this List<FileQuery> queries, List<FileQuery> otherQueries)
        {
            if (queries == null)
            {
                throw new ArgumentNullException("queries");
            }

            if (otherQueries == null)
            {
                throw new ArgumentNullException("otherQueries");
            }

            HashSet<string> query1 = new HashSet<string>(queries.GetFiles(), new FileNameComparer());
            HashSet<string> query2 = new HashSet<string>(otherQueries.GetFiles(), new FileNameComparer());

            var removedFiles = new HashSet<string>(query1, new FileNameComparer());
            removedFiles.ExceptWith(query2);
            return removedFiles.ToList();
        }
    }


    public class FileQuery
    {

        SearchOption mySearchOption;

        string myQuery;
        public string SearchDir
        {
            get
            {
                // relative directory given use current working directory
                if (!myQuery.Contains('\\'))
                {
                    return Directory.GetCurrentDirectory();
                }
                else
                {
                    if (myQuery.StartsWith("GAC:\\", StringComparison.OrdinalIgnoreCase))
                    {
                        return "GAC:\\";
                    }

                    if (UseCwd)
                    {
                        return Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), Path.GetDirectoryName(myQuery)));
                    }
                    else
                    {
                        // absolute directory path is already fully specified
                        return Path.GetFullPath(Path.GetDirectoryName(myQuery));
                    }
                }
            }
        }

        internal bool UseCwd = false;

        public string FileMask
        {
            get
            {
                return Path.GetFileName(myQuery);
            }
        }

        public string Query
        {
            get
            {
                return myQuery;
            }
        }

        public string[] Files
        {
            get
            {
                string[] lret = new string[0];

                BeginSearch();

                if (mySearcher != null)
                {
                    List<string> matches = new List<string>();
                    foreach (string file in mySearcher.GetResultQueue())
                    {
                        matches.Add(file);
                    }
                    lret = matches.ToArray();
                }

                return lret;
            }
        }

        DirectorySearcherAsync mySearcher;

        public void BeginSearch()
        {
            if (mySearcher == null && SearchDir != "GAC:\\" && !String.IsNullOrEmpty(FileMask))
            {
                mySearcher = new DirectorySearcherAsync(SearchDir, FileMask, mySearchOption);
                mySearcher.BeginSearch();
            }
        }

        public bool HasMatches
        {
            get
            {
                BeginSearch();
                if (mySearcher != null)
                {
                    return mySearcher.HasMatchingFiles;
                }
                else
                    return false;
            }
        }


        public BlockingQueue<string> EnumerateFiles
        {
            get
            {
                BeginSearch();
                return mySearcher.GetResultQueue();
            }
        }

        public static List<FileQuery> ParseQueryList(string query)
        {
            return ParseQueryList(query, null);
        }

        static string GAC_32
        {
            get
            {
                return Path.Combine(Environment.GetEnvironmentVariable("WINDIR"), "assembly\\GAC_32");
            }
        }

        static string GAC_MSIL
        {
            get
            {
                return Path.Combine(Environment.GetEnvironmentVariable("WINDIR"), "assembly\\GAC_MSIL");
            }
        }

        static string GetFileNameWithOutDllExtension(string file)
        {
            if (file.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
            {
                return file.Substring(0, file.Length - 4);
            }
            else
                return file;
        }

         public static List<FileQuery> ParseQueryList(string query, string rootDir)
        {
            List<FileQuery> ret = new List<FileQuery>();

            string [] queries = query.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string q in queries)
            {
                string querystr = q.Trim();
                if (!String.IsNullOrEmpty(rootDir))
                {
                    querystr = Path.Combine(rootDir, q);
                }

                ret.Add(new FileQuery(querystr));
            }

            return ret;
        }

        public string GetMatchingFileByName(string fileName)
        {
            string pureFileName = Path.GetFileName(fileName);

            foreach (string file in EnumerateFiles)
            {
                if (String.Compare(Path.GetFileName(file), pureFileName, StringComparison.OrdinalIgnoreCase) == 0)
                    return file;
            }

            return null;
        }

        public FileQuery(string query):this(query, SearchOption.TopDirectoryOnly)
        {
        }

        public FileQuery(string query, SearchOption searchOption)
        {
            if (String.IsNullOrEmpty(query))
            {
                throw new ArgumentException("The file query was null or empty");
            }

            mySearchOption = searchOption;
            myQuery = Environment.ExpandEnvironmentVariables(query);


            int gacidx = query.IndexOf("gac:\\", StringComparison.OrdinalIgnoreCase);
            if (gacidx == 0)
            {
                if (query.Contains("*"))
                {
                    throw new ArgumentException(
                        String.Format("Wildcards are not supported in Global Assembly Cache search: {0}", query));
                }

                string fileName = query.Substring(5);

                string dirName = GetFileNameWithOutDllExtension(fileName);

                if (Directory.Exists(Path.Combine(GAC_32, dirName)))
                {
                    myQuery = Path.Combine(Path.Combine(GAC_32, dirName), fileName);
                }

                if( Directory.Exists(Path.Combine(GAC_MSIL, dirName)) )
                {
                    myQuery = Path.Combine(Path.Combine(GAC_MSIL, dirName),fileName);
                }

                mySearchOption = SearchOption.AllDirectories;
            }
        }

        public FileQuery(string searchDir, string filemask)
        {
            if (string.IsNullOrEmpty(searchDir))
            {
                throw new ArgumentNullException("searchdir");
            }
            if (string.IsNullOrEmpty(filemask))
            {
                throw new ArgumentNullException("filemask");
            }

            myQuery = Path.Combine(Environment.ExpandEnvironmentVariables(searchDir), filemask);
        }
    }
}
