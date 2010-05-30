
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace ApiChange.Api.Introspection
{
    public class QueryResult<T> where T:IMemberReference
    {
        public T Match
        {
            get;
            private set;
        }

        public string SourceFileName
        {
            get;
            private set;
        }

        public int LineNumber
        {
            get;
            private set;
        }

        MatchContext myAnnotations;
        public MatchContext Annotations
        {
            get
            {
                if (myAnnotations == null)
                    myAnnotations = new MatchContext();

                return myAnnotations;
            }
        }
        
        public QueryResult(T match, string fileName, int lineNumber)
        {
            if (match == null)
            {
                throw new ArgumentNullException("result");
            }

            Match = match;
            SourceFileName = fileName;
            LineNumber = lineNumber;
        }

        public QueryResult(T match, string fileName, int lineNumber, MatchContext context):this(match,fileName,lineNumber)
        {
            if (context == null)
            {
                throw new ArgumentNullException("match context was null");
            }

            foreach (var kvp in context)
            {
                this.Annotations[kvp.Key] = kvp.Value;
            }
        }
    }
}
