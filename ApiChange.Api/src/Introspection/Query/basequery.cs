
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ApiChange.Api.Introspection
{
    public class BaseQuery
    {
        protected internal bool? myIsPrivate;
        protected internal bool? myIsPublic;
        protected internal bool? myIsInternal;
        protected internal bool? myIsProtected;
        protected internal bool? myIsProtectedInernal;
        protected internal bool? myIsStatic;


        static Regex myEventQueryParser;

        // Common Regular expression part shared by the different queries
        const string CommonModifiers = "!?static +|!?public +|!?protected +internal +|!?protected +|!?internal +|!?private +";

        internal static Regex EventQueryParser
        {
            get
            {
                if (myEventQueryParser == null)
                {
                    myEventQueryParser = new Regex(
                        "^ *(?<modifiers>!?virtual +|event +|" + CommonModifiers + ")*" +
                        @" *(?<eventType>[^ ]+(<.*>)?) +(?<eventName>[^ ]+) *$");
                }

                return myEventQueryParser;
            }
        }

        static Regex myFieldQueryParser;
        internal static Regex FieldQueryParser
        {
            get
            {
                if (myFieldQueryParser == null)
                {
                    myFieldQueryParser = new Regex(
                        " *(?<modifiers>!?nocompilergenerated +|!?const +|!?readonly +|" + CommonModifiers + ")*" +
                        @" *(?<fieldType>[^ ]+(<.*>)?) +(?<fieldName>[^ ]+) *$");
                }

                return myFieldQueryParser;
            }
        }

        static Regex myMethodDefParser;
        internal static Regex MethodDefParser
        {
            get
            {
                if (myMethodDefParser == null)
                {
                    myMethodDefParser = new Regex
                     (
                        @" *(?<modifiers>!?virtual +|" + CommonModifiers + ")*" +
                        @"(?<retType>.*<.*>( *\[\])?|[^ (\)]*( *\[\])?) +(?<funcName>.+)\( *(?<args>.*?) *\) *"
                     );
                }

                return myMethodDefParser;
            }
        }

        protected internal Regex Parser
        {
            get;
            set;
        }

        protected internal string NameFilter
        {
            get;
            set;
        }

        protected BaseQuery(string query)
        {
            if (String.IsNullOrEmpty(query))
            {
                throw new ArgumentNullException("query string was empty");
            }
        }


        protected virtual internal bool IsMatch(Match m, string key)
        {
            return m.Groups[key].Success;
        }

        protected virtual internal bool? Captures(Match m, string value)
        {
            string notValue = "!" + value;
            foreach (Capture capture in m.Groups["modifiers"].Captures)
            {
                if (value == capture.Value.TrimEnd())
                    return true;
                if (notValue == capture.Value.TrimEnd())
                    return false;
            }

            return null;
        }

        protected string Value(Match m, string groupName)
        {
            return m.Groups[groupName].Value;
        }

        protected virtual void SetModifierFilter(Match m)
        {
            myIsProtected = Captures(m, "protected");
            myIsInternal = Captures(m, "internal");
            myIsProtectedInernal = Captures(m, "protected internal");
            myIsPublic = Captures(m, "public");
            myIsPrivate = Captures(m, "private");
            myIsStatic = Captures(m, "static");
        }

        protected virtual bool MatchName(string name)
        {
            if (String.IsNullOrEmpty(NameFilter) || NameFilter == "*")
            {
                return true;
            }

            return Matcher.MatchWithWildcards(this.NameFilter, name, StringComparison.OrdinalIgnoreCase);
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
    }
}
