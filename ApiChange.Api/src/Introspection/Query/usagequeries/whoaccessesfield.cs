
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace ApiChange.Api.Introspection
{
    public class WhoAccessesField : UsageVisitor
    {
        HashSet<string> myDeclaringTypeNamesToSearch = new HashSet<string>();
        List<FieldDefinition> mySearchFields;

        public WhoAccessesField(UsageQueryAggregator aggreagator, FieldDefinition field):
            this(aggreagator, new List<FieldDefinition> { ThrowIfNull<FieldDefinition>("field", field) })
        {
        }

        public WhoAccessesField(UsageQueryAggregator aggregator, List<FieldDefinition> fields):base(aggregator)
        {
            if (fields == null)
            {
                throw new ArgumentException("The field list was null.");
            }

            mySearchFields = fields;

            foreach (FieldDefinition field in fields)
            {
                if (field.HasConstant)
                {
                    throw new ArgumentException(
                        String.Format("The field {0} is constant. Its value is compiled directly into the users of this constant which makes is impossible to search for users of it.",
                        field.Print(FieldPrintOptions.All)));
                }
                myDeclaringTypeNamesToSearch.Add(field.DeclaringType.Name);
                Aggregator.AddVisitScope(field.DeclaringType.Module.Assembly.Name.Name);
            }
        }

        void CheckFieldReferenceAndAddIfMatch(Instruction instr, MethodDefinition method, string operation)
        {
            FieldReference field = (FieldReference)instr.Operand;

            if (myDeclaringTypeNamesToSearch.Contains(field.DeclaringType.Name))
            {
                foreach (FieldDefinition searchField in mySearchFields)
                {
                    if (field.DeclaringType.IsEqual(searchField.DeclaringType,false) &&
                        field.Name == searchField.Name &&
                        field.FieldType.IsEqual(searchField.FieldType) )
                    {
                        MatchContext context = new MatchContext(operation,
                            String.Format("{0} {1}", searchField.DeclaringType.FullName, searchField.Name));
                        Aggregator.AddMatch(instr, method,false, context);
                    }
                }
            }
        }

        public override void VisitMethod(MethodDefinition method)
        {
            if (!method.HasBody)
                return;

            foreach (Instruction instr in method.Body.Instructions)
            {
                switch (instr.OpCode.Code)
                {
                    case Code.Ldfld: // Load instance field value
                        CheckFieldReferenceAndAddIfMatch(instr, method, "Read");
                        break;
                    case Code.Ldflda: // Load instance field address
                        CheckFieldReferenceAndAddIfMatch(instr, method, "Load Address");
                        break;
                    case Code.Ldsflda: // Load static field address
                        CheckFieldReferenceAndAddIfMatch(instr, method, "Load Address");
                        break;
                    case Code.Ldsfld: // Load static field value
                        CheckFieldReferenceAndAddIfMatch(instr, method, "Read");
                        break;
                    case Code.Stfld: // Store field
                        CheckFieldReferenceAndAddIfMatch(instr, method, "Assign");
                        break;
                    case Code.Stsfld: // Store static field
                        CheckFieldReferenceAndAddIfMatch(instr, method, "Assign");
                        break;

                    default:
                        break;

                }
            }
        }
    }
}
