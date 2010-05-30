
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace ApiChange.Api.Introspection
{
    public class WhoDerivesFromType : UsageVisitor
    {
        List<TypeDefinition> mySearchBaseTypes;

        public WhoDerivesFromType(UsageQueryAggregator aggregator, TypeDefinition typeDef)
            :this(aggregator, new List<TypeDefinition> { ThrowIfNull<TypeDefinition>("typeDef",typeDef) })
        {
        }

        public WhoDerivesFromType(UsageQueryAggregator aggregator, List<TypeDefinition> typeDefs):base(aggregator)
        {
            if (typeDefs == null)
                throw new ArgumentException("The type list to query for was null.");

            foreach (var type in typeDefs)
            {
                Aggregator.AddVisitScope(type.Module.Assembly.Name.Name);
            }

            mySearchBaseTypes = typeDefs;
        }

        public override void VisitType(TypeDefinition type)
        {
            if (type.BaseType == null)
                return;

            foreach(TypeDefinition searchType in mySearchBaseTypes)
            {
                if( type.BaseType.IsEqual(searchType,false) )
                {
                    var context = new MatchContext("Derives from", searchType.Print());
                    Aggregator.AddMatch(type, context);
                    break;
                }
            }
        }
    }
}
