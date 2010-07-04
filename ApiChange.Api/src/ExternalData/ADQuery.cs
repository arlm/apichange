
using System;
using System.Collections.Generic;
using System.Linq;
using System.DirectoryServices;
using System.Diagnostics;
using ApiChange.Infrastructure;

namespace ApiChange.ExternalData
{
    public class ADQuery
    {
        static TypeHashes myType = new TypeHashes(typeof(ADQuery));

        // Fields
        private static Dictionary<string, UserInfo> myADCache = new Dictionary<string, UserInfo>();

        const char SepChar = '~';
        char[] mySeparators = new char[] { ' ', SepChar };


        const string Ldap_Phone = "telephoneNumber";
        const string Ldap_Department = "department";
        const string Ldap_Mail = "mail";

        // Methods
        private bool AddMatchesFromQuery(SearchResultCollection coll, UserInfo info)
        {
            int i;
            bool lret = false;
            if (coll != null)
            {
                // ignore more than 3 matches
                if (coll.Count > 3)
                    return lret;

                for (i = 0; i < coll.Count; i++)
                {
                    SearchResult res = coll[i];
                    string mail = this.GetPropertyFromResult(res, Ldap_Mail);
                    if (!string.IsNullOrEmpty(mail))
                    {
                        info.Mail = info.Mail + mail + SepChar;
                        info.DisplayName = this.GetDisplayName(res);
                        this.AddSureAndGivenName(res, info);
                        lret = true;
                    }
                    if (info.DisplayName == "")
                    {
                        info.DisplayName = this.GetDisplayName(res);
                        this.AddSureAndGivenName(res, info);
                    }

                    string phone = this.GetPropertyFromResult(res, Ldap_Phone );
                    if (!String.IsNullOrEmpty(phone))
                    {
                        info.Phone += phone + SepChar;
                    }

                    string department = this.GetPropertyFromResult(res, Ldap_Department);
                    if (!String.IsNullOrEmpty(department))
                    {
                        info.Department += department + SepChar;
                    }
                }
            }
            string[] uniqeMailArray = new HashSet<string>(info.Mail.Split(mySeparators, StringSplitOptions.RemoveEmptyEntries)).ToArray<string>();
            info.Mail = "";
            for (i = 0; i < uniqeMailArray.Length; i++)
            {
                info.Mail = info.Mail + uniqeMailArray[i];
                if (i != (uniqeMailArray.Length - 1))
                {
                    info.Mail = info.Mail + "~ ";
                }
            }

            info.Phone = info.Phone.TrimEnd(mySeparators);
            info.Department = info.Department.TrimEnd(mySeparators);

            return lret;
        }

        private void AddSureAndGivenName(SearchResult res, UserInfo info)
        {
            ResultPropertyValueCollection givenNameColl = res.Properties["givenName"];
            ResultPropertyValueCollection sureNameColl = res.Properties["sn"];
            if ((((givenNameColl != null) && (sureNameColl != null)) && (givenNameColl.Count > 0)) && (sureNameColl.Count > 0))
            {
                info.SureName = sureNameColl[0].ToString();
                info.GivenName = givenNameColl[0].ToString();
            }
        }

        public void ClearCache()
        {
            lock (myADCache)
            {
                myADCache.Clear();
            }
        }

        internal string CreateFilterQueryForUser(string userName)
        {
            string[] userParts = userName.Split(new char[] { '.' });
            string loginName = null;
            string givenName = null;
            string sureName = null;
            if (userParts.Length == 1)
            {
                loginName = userParts[0];
                return string.Format("((&(anr={0})(objectCategory=person)))", userName);
            }
            givenName = userParts[0];
            sureName = userParts[1];
            Debug.Assert(!string.IsNullOrEmpty(givenName), string.Format("Given name must be present for user {0}", userName));
            Debug.Assert(!string.IsNullOrEmpty(sureName), string.Format("Sure name must be present for user {0}", userName));
            return string.Format("((&(|(&(sn={1})(givenName={0}))(&(sn={0})(givenName={1})) )(|(objectCategory=person)(objectCategory=scdPerson))))", sureName, givenName);
        }

        internal DirectoryEntry GetDirectoryEntry()
        {
            return new DirectoryEntry { Path = SiteConstants.ADQuery, AuthenticationType = AuthenticationTypes.Secure };
        }

        private string GetDisplayName(SearchResult res)
        {
            string ret = "none";
            ResultPropertyValueCollection displayNameColl = res.Properties["displayName"];
            if ((displayNameColl != null) && (displayNameColl.Count > 0))
            {
                ret = displayNameColl[0].ToString();
            }
            return ret;
        }

        internal DirectoryEntry GetGCDirectoryEntry()
        {
            return new DirectoryEntry { Path = SiteConstants.GCQuery, AuthenticationType = AuthenticationTypes.Secure };
        }

        private string GetPropertyFromResult(SearchResult res,string name)
        {
            string ret = "";
            ResultPropertyValueCollection mailRes = res.Properties[name];
            if (mailRes.Count > 0)
            {
                ret = mailRes[0].ToString();
            }
            return ret;
        }


        public UserInfo GetUserInfoFromName(string userName)
        {
            UserInfo ret = null;
            bool bCached = false;

            lock(myADCache)
            {
                bCached = myADCache.TryGetValue(userName, out ret);
            }
            if (!bCached)
            {
                ret = new UserInfo
                {
                    UserName = userName
                };

                bool bFound = false;
                DirectorySearcher searcher;
                using(searcher = this.InitSearcher(this.GetGCDirectoryEntry(), userName))
                {
                    try
                    {
                        bFound = this.AddMatchesFromQuery(searcher.FindAll(), ret);
                    }
                    catch (Exception)
                    {
                    }
                }

                if (!bFound)
                {
                    using (searcher = this.InitSearcher(this.GetDirectoryEntry(), userName))
                    {
                        bFound = this.AddMatchesFromQuery(searcher.FindAll(), ret);
                    }
                }

                Tracer.Info(Level.L3, myType, "GetUserInfoFromName", "Got for user {0}: Mail: {1}, DisplayName {2}", ret.UserName, ret.Mail, ret.DisplayName);

                lock (myADCache)
                {
                    myADCache[userName] = ret;
                }
            }
            return ret;
        }


        internal DirectorySearcher InitSearcher(DirectoryEntry ent, string userName)
        {
            DirectorySearcher searcher = new DirectorySearcher(ent)
            {
                SearchScope = SearchScope.Subtree,
                Filter = this.CreateFilterQueryForUser(userName)
            };
            searcher.PropertiesToLoad.AddRange(new string[] { "displayname", "cn", Ldap_Mail, "sn", "givenName", Ldap_Phone, Ldap_Department });
            return searcher;
        }

        private void PrintProperties(SearchResult res)
        {
            DirectoryEntry ent = res.GetDirectoryEntry();
            Console.WriteLine("Properties of {0}", ent.Name);
            Console.WriteLine("Path: {0}", ent.Path);
            foreach (string name in ent.Properties.PropertyNames)
            {
                Console.Write("\t{0} Value: ", name);
                PropertyValueCollection valuecoll = ent.Properties[name];
                foreach (object obj in valuecoll)
                {
                    Console.Write("\t{0} ", obj);
                }
                Console.WriteLine("");
            }
        }
    }
}
