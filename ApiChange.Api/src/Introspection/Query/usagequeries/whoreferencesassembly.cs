
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using Mono.Cecil;

namespace ApiChange.Api.Introspection
{
    public class WhoReferencesAssembly : UsageVisitor
    {
        string myAssembly;

        public WhoReferencesAssembly(UsageQueryAggregator aggregator, string fileName):base(aggregator)
        {
            if (String.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("File name was null or empty.");
            }

            myAssembly = Path.GetFileNameWithoutExtension(fileName);
            aggregator.AddVisitScope(fileName);
        }


        public override void VisitAssemblyReference(AssemblyNameReference assemblyRef, AssemblyDefinition current)
        {
            if (String.Compare(assemblyRef.Name, myAssembly, true) == 0)
            {
                Aggregator.AddMatch(current);
            }
        }
    }
}
