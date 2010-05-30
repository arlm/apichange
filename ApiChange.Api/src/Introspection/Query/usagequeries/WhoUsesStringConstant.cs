
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil.Cil;

namespace ApiChange.Api.Introspection
{
    /// <summary>
    /// Search for the ldstr opcode which loads a string constant and check if the string constant matches 
    /// the search string.
    /// </summary>
    public class WhoUsesStringConstant : UsageVisitor
    {
        string mySearchString;
        bool   mybExatchMatch;
        StringComparison myComparisonMode;

        public WhoUsesStringConstant(UsageQueryAggregator aggregator, string searchString, bool bExactMatch, StringComparison compMode):
            base(aggregator)
        {
            if( String.IsNullOrEmpty(searchString) )
            {
                throw new ArgumentException("The search string was null or empty");
            }

            mySearchString = searchString;
            mybExatchMatch = bExactMatch;
            myComparisonMode = compMode;
        }

        public WhoUsesStringConstant(UsageQueryAggregator aggregator, string searchString)
            : this(aggregator, searchString, false, StringComparison.OrdinalIgnoreCase)
        {
        }

        public WhoUsesStringConstant(UsageQueryAggregator aggregator, string searchString, bool bExactMatch)
            : this(aggregator, searchString, bExactMatch, StringComparison.OrdinalIgnoreCase)
        {
        }

        public override void VisitMethodBody(Mono.Cecil.Cil.MethodBody body)
        {
            base.VisitMethodBody(body);
            foreach (Instruction instruction in body.Instructions)
            {
                if (instruction.OpCode.Code == Code.Ldstr)
                {
                    string str = (string)instruction.Operand;
                    if (IsMatch(str))
                    {
                        AddMatch(instruction, body, str);
                    }
                }
            }
        }

        bool IsMatch(string value)
        {
            bool lret = false;

            if (mybExatchMatch)
            {
                if (String.Compare(value, mySearchString, myComparisonMode) == 0)
                {
                    lret = true;
                }
            }
            else
            {
                if (value.IndexOf(mySearchString, myComparisonMode) != -1)
                {
                    lret = true;       
                }
            }

            return lret;
        }

        public override void VisitField(Mono.Cecil.FieldDefinition field)
        {
            base.VisitField(field);

            if (field.HasConstant)
            {
                string stringValue = field.Constant as string;
                if (stringValue != null && IsMatch(stringValue))
                {
                    MatchContext context = new MatchContext("String Match", mySearchString);
                    context["String"] = stringValue;
                    base.Aggregator.AddMatch(field, context);
                }
            }
        }

        void AddMatch(Instruction ins, MethodBody body,string matchedString)
        {
            MatchContext context = new MatchContext("String Match", mySearchString);
            context["String"] = matchedString;
            base.Aggregator.AddMatch(ins, body.Method, false, context);
        }
    }
}