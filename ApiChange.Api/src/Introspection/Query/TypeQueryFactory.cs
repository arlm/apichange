
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ApiChange.Api.Introspection
{
    /// <summary>
    /// Parser for a list of type queries separated by ;. A type query can 
    /// </summary>
    internal class TypeQueryFactory
    {
        static Regex myQueryParser = new Regex("^ *(?<modifiers>api +|nocompiler +|public +|internal +|class +|struct +|interface +|enum +)* *(?<typeName>[^ ]+) *$");

        /// <summary>
        /// Parse a list of type queries separated by ; and return the resulting type query list
        /// </summary>
        /// <param name="queries"></param>
        /// <returns></returns>
        public List<TypeQuery> GetQueries(string queries)
        {
            return GetQueries(queries, TypeQueryMode.None);
        }

        public List<TypeQuery> GetQueries(string typeQueries, TypeQueryMode additionalFlags)
        {
            if (typeQueries == null)
            {
                throw new ArgumentNullException("typeQueries");
            }

            string trimedQuery = typeQueries.Trim();
            if (trimedQuery == "")
            {
                throw new ArgumentException("typeQueries was an empty string");
            }

            string[] queries = trimedQuery.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            List<TypeQuery> ret = new List<TypeQuery>();

            foreach (string query in queries)
            {
                Match m = myQueryParser.Match(query);
                if (!m.Success)
                {
                    throw new ArgumentException(
                        String.Format("The type query \"{0}\" is not of the form [public|internal|class|interface|struct|enum|nocompiler|api] typename", query));
                }

                TypeQueryMode mode = GetQueryMode(m);
                var nameSpaceTypeName = SplitNameSpaceAndType(m.Groups["typeName"].Value);
                var typeQuery = new TypeQuery(mode, nameSpaceTypeName.Key, nameSpaceTypeName.Value);
                if (typeQuery.SearchMode == TypeQueryMode.None)
                {
                    typeQuery.SearchMode |= additionalFlags;
                }
                ret.Add(typeQuery);
            }

            return ret;
        }

        internal KeyValuePair<string, string> SplitNameSpaceAndType(string fullQualifiedTypeName)
        {
            if (String.IsNullOrEmpty(fullQualifiedTypeName))
            {
                throw new ArgumentNullException("fullQualifiedTypeName");
            }

            string[] parts = fullQualifiedTypeName.Trim().Split(new char[] { '.' });
            if (parts.Length > 1)
            {
                return new KeyValuePair<string, string>(String.Join(".", parts, 0, parts.Length - 1),
                                                       parts[parts.Length - 1]);
            }
            else
            {
                return new KeyValuePair<string, string>(null, parts[0]);
            }
        }

        private TypeQueryMode GetQueryMode(Match m)
        {
            TypeQueryMode mode = TypeQueryMode.None;

            if (Captures(m, "public"))
                mode |= TypeQueryMode.Public;
            if (Captures(m, "internal"))
                mode |= TypeQueryMode.Internal;
            if (Captures(m, "class"))
                mode |= TypeQueryMode.Class;
            if (Captures(m, "interface"))
                mode |= TypeQueryMode.Interface;
            if (Captures(m, "struct"))
                mode |= TypeQueryMode.ValueType;
            if (Captures(m, "enum"))
                mode |= TypeQueryMode.Enum;
            if (Captures(m, "nocompiler"))
                mode |= TypeQueryMode.NotCompilerGenerated;
            if (Captures(m, "api"))
                mode |= TypeQueryMode.ApiRelevant;

            return mode;
        }

        protected virtual internal bool Captures(Match m, string value)
        {
            foreach (Capture capture in m.Groups["modifiers"].Captures)
            {
                if (value == capture.Value.TrimEnd())
                    return true;
            }

            return false;
        }
    }
}