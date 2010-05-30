
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace ApiChange.Api.Introspection
{
    class WhoImplementsInterface : UsageVisitor
    {
        Dictionary<string, TypeDefinition> myInterfaceNames = new Dictionary<string, TypeDefinition>();

        public WhoImplementsInterface(UsageQueryAggregator aggregator, TypeDefinition itf)
            :this(aggregator, new List<TypeDefinition>() { ThrowIfNull<TypeDefinition>("itf",itf) })
        {
        }

        public WhoImplementsInterface(UsageQueryAggregator aggreator, List<TypeDefinition> interfaces):base(aggreator)
        {
            if (interfaces == null || interfaces.Count == 0)
            {
                throw new ArgumentException("The interfaces collection was null.");
            }

            foreach (var type in interfaces)
            {
                if (!type.IsInterface)
                {
                    throw new ArgumentException(
                        String.Format("The type {0} is not an interface", type.Print()));
                }

                Aggregator.AddVisitScope(type.Module.Assembly.Name.Name);
                if (!myInterfaceNames.ContainsKey(type.Name))
                {
                    myInterfaceNames.Add(type.Name, type);
                }
            }

        }

        bool IsMatchingInterface(TypeReference itf, out TypeDefinition searchItf)
        {
            if (myInterfaceNames.TryGetValue(itf.Name, out searchItf))
            {
                if (itf.IsEqual(searchItf, false))
                    return true;
            }

            return false;
        }

        public override void VisitType(TypeDefinition type)
        {
            if (type.Interfaces == null)
                return;

            TypeDefinition searchItf = null;
            foreach(TypeReference itf in type.Interfaces)
            {
                if (IsMatchingInterface(itf, out searchItf))
                {
                    MatchContext context = new MatchContext();
                    context[MatchContext.MatchReason] = "Implements interface";
                    context[MatchContext.MatchItem] = searchItf.Print();
                    Aggregator.AddMatch(type, context);
                }
            }
        }
    }
}
