

using System;
using System.Diagnostics;
using ClearCase;
using System.IO;
using ApiChange.Infrastructure;

namespace ApiChange.ExternalData
{
    public class ClearCaseToADMapper : IFileInformationProvider
    {
        #region IFileInformationProvider Members
        static TypeHandle myType = new TypeHandle(typeof(ClearCaseToADMapper));
        ApplicationClass myCCApp = new ApplicationClass();
        ADQuery myActiveDir = new ADQuery();

        /// <summary>
        /// Gets the name of the exact path with real case sensitive file and directory names.
        /// This is necesary for ClearCase calls which rely on case sensitive file names.
        /// </summary>
        /// <param name="pathName">Name of the path.</param>
        /// <returns></returns>
        public static string GetExactPathName(string pathName)
        {
            if (!(File.Exists(pathName) || Directory.Exists(pathName)))
                 throw new FileNotFoundException(pathName);

            var di = new DirectoryInfo(pathName);

            if (di.Parent != null)
            {
                return Path.Combine(
                    GetExactPathName(di.Parent.FullName),
                    di.Parent.GetFileSystemInfos(di.Name)[0].Name);
            }
            else
            {
                return di.Name.ToUpper();
            }
        }


        /// <summary>
        /// Get the user which did check the given file version in.
        /// </summary>
        /// <param name="fileName">Full Path of file. It can contain a Clearcase file name of the form Drive:\VOB\Directories\filename@@\branch\dd.</param>
        /// <returns>Name of checkin user or String.Empty in case of error.</returns>
        public string GetCheckinUser(string fileName)
        {
            using (Tracer t = new Tracer(myType, "GetCheckinUser"))
            {
                t.Info("Get data for file {0}", fileName);
                string userName = "";

                try
                {
                    string exactFileName = GetExactPathName(fileName);
                    t.Info("Exact cased filename is {0}", exactFileName);
                    CCVersion version = myCCApp.get_Version(exactFileName);
                    var createInfo = version.CreationRecord;
                    userName = createInfo.UserLoginName;
                    if (String.Compare(createInfo.Group, "syngo", true) == 0 ||
                        String.Compare(createInfo.Group, "Domain Users", true) == 0)
                    {
                        string tmpUser = createInfo.UserFullName.Replace(' ', '.');
                        if (!String.IsNullOrEmpty(tmpUser))
                        {
                            userName = tmpUser;
                        }
                    }
                }
                catch(Exception ex)
                {
                    t.Error(Level.L1, ex, "Could not retrieve file data.");
                }

                return userName;
            }
        }


        public UserInfo GetInformationFromFile(string fileName)
        {
            using (Tracer t = new Tracer(myType, "GetInformationFromFile"))
            {
                string user = null;
                lock (this)
                {
                    user = GetCheckinUser(fileName);
                    t.Info("Checkin user was: {0}", user);
                    if (user == "")
                    {
                        return null;
                    }
                }

                return myActiveDir.GetUserInfoFromName(user);
            }
        }

        #endregion
    }
}
