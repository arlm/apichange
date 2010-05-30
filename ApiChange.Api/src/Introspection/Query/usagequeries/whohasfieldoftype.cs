
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace ApiChange.Api.Introspection
{
    public class WhoHasFieldOfType : UsageVisitor
    {
        HashSet<string> mySearchTypeNames = new HashSet<string>();
        List<TypeDefinition> mySearchTypes;

        public WhoHasFieldOfType(UsageQueryAggregator aggregator, TypeDefinition fieldType)
            : this(aggregator, new List<TypeDefinition> { ThrowIfNull<TypeDefinition>("fieldType", fieldType) })
        {
        }

        public WhoHasFieldOfType(UsageQueryAggregator aggregator, List<TypeDefinition> fieldTypes)
            : base(aggregator)
        {
            if (fieldTypes == null)
                throw new ArgumentNullException("fieldTypes");

            mySearchTypes = fieldTypes;

            foreach (TypeDefinition fieldType in fieldTypes)
            {
                mySearchTypeNames.Add(fieldType.Name);
                Aggregator.AddVisitScope(fieldType.Module.Assembly.Name.Name);
            }
        }

        public override void VisitField(FieldDefinition field)
        {
            TypeDefinition matchingType = null;
            if (IsMatching(mySearchTypeNames, mySearchTypes, field.FieldType, out matchingType))
            {
                MatchContext context = new MatchContext("Has Field Of Type", matchingType.Print());
                Aggregator.AddMatch(field,context);
                return;
            }

            GenericInstanceType genType = field.FieldType as GenericInstanceType;
            if (genType != null)
            {
                foreach (TypeReference generic in genType.GenericArguments)
                {
                    if (IsMatching(mySearchTypeNames, mySearchTypes, generic, out matchingType))
                    {
                        MatchContext context = new MatchContext("Has Field Of Type", matchingType.Print());
                        Aggregator.AddMatch(field, context);
                        return;
                    }
                }
            }
        }
    }
}
