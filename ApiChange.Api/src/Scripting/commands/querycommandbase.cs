
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ApiChange.Api.Introspection;
using System.Text.RegularExpressions;

namespace ApiChange.Api.Scripting
{
    abstract class QueryCommandBase : CommandBase
    {
        protected List<TypeQuery> myTypeQueries;
        protected string myInnerQuery;
 
        public QueryCommandBase(CommandData parsedArgs)
            : base(parsedArgs)
        {
        }

        int CountChars(char searchChar, string str)
        {
            int ret = 0;
            foreach (char c in str)
            {
                if (c == searchChar)
                    ret++;
            }

            return ret;
        }

        public virtual bool ExtractAndValidateTypeQuery(string query)
        {
            Match m = new Regex(@" *(?<typeName>.*?) *\( *(?<innerQuery>.*) *\) *").Match(query);
            if (m.Success)
            {
                myTypeQueries = TypeQuery.GetQueries(m.Groups["typeName"].Value, TypeQueryMode.All);
                myInnerQuery = m.Groups["innerQuery"].Value;
                return true;
            }
            else
            {
                int opening = CountChars('(', query);
                int closing = CountChars(')', query);
                if (opening == 0)
                {
                    Out.WriteLine("The query should contain at least one opening brace");
                }
                else if (opening - closing != 0)
                {
                    Out.WriteLine("The query {0} has not all braces closed.", query);
                }

                return false;
            }
        }
        
    }
}
