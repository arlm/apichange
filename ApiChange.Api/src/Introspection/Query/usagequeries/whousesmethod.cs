
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using System.IO;
using Mono.Cecil.Cil;
using Mono.Cecil.Pdb;
using ApiChange.Infrastructure;

namespace ApiChange.Api.Introspection
{
    public class WhoUsesMethod : UsageVisitor
    {
        static TypeHashes myType = new TypeHashes(typeof(WhoUsesMethod));
        const MethodPrintOption myMethodFormat = MethodPrintOption.ReturnType | MethodPrintOption.ShortNames | MethodPrintOption.Parameters;

        // Do fast compares with Hashsets instead of string comparisons
        Dictionary<string, List<MethodDefinition>> myMethodNames = new Dictionary<string, List<MethodDefinition>>();

        public WhoUsesMethod(UsageQueryAggregator aggregator, List<MethodDefinition> methods):base(aggregator)
        {
            if (methods == null )
            {
                throw new ArgumentNullException("The method list to query for was null.");
            }

            foreach (var method in methods)
            {
                Aggregator.AddVisitScope(method.DeclaringType.Module.Assembly.Name.Name);
                List<MethodDefinition> typeMethods = null;
                if(!myMethodNames.TryGetValue(method.DeclaringType.Name, out typeMethods))
                {
                    typeMethods = new List<MethodDefinition>();
                    myMethodNames[method.DeclaringType.Name] = typeMethods;
                }
                myMethodNames[method.DeclaringType.Name].Add(method);
            }
        }

        bool IsMatchingMethod(MethodReference methodReference, out MethodDefinition matchingMethod)
        {
                bool lret = false;
                matchingMethod = null;

                string declaringType = "";
                if (methodReference != null && methodReference.DeclaringType != null &&
                    methodReference.DeclaringType.GetOriginalType() != null)
                {
                    declaringType = methodReference.DeclaringType.GetOriginalType().Name;
                }

                List<MethodDefinition> typeMethods = null;
                if (myMethodNames.TryGetValue(declaringType, out typeMethods))
                {
                    foreach (MethodDefinition searchMethod in typeMethods)
                    {
                        if (methodReference.IsEqual(searchMethod, false))
                        {
                            lret = true;
                            matchingMethod = searchMethod;
                            break;
                        }
                    }
                }

                return lret;
        }

        public override void VisitMethod(MethodDefinition method)
        {
            if (method.Body == null)
                return;

            MethodDefinition matchingMethod = null;
            foreach (Instruction ins in method.Body.Instructions)
            {
                if (Code.Callvirt == ins.OpCode.Code) // normal instance call
                {
                    if (IsMatchingMethod((MethodReference)ins.Operand, out matchingMethod))
                    {
                        MatchContext context = new MatchContext("Called method", matchingMethod.Print(myMethodFormat));
                        context["Type"] = matchingMethod.DeclaringType.FullName;
                        Aggregator.AddMatch(ins, method, false, context);
                    }
                }

                if (Code.Call == ins.OpCode.Code) // static function call
                {
                    if (IsMatchingMethod((MethodReference)ins.Operand, out matchingMethod))
                    {
                        MatchContext context = new MatchContext("Called method", matchingMethod.Print(myMethodFormat));
                        context["Type"] = matchingMethod.DeclaringType.FullName;
                        Aggregator.AddMatch(ins, method, false, context);
                    }
                }

                if (Code.Ldftn == ins.OpCode.Code) // Load Function Pointer for delegate call
                {
                    if (IsMatchingMethod((MethodReference)ins.Operand, out matchingMethod))
                    {
                        MatchContext context = new MatchContext("Called method", matchingMethod.Print(myMethodFormat));
                        context["Type"] = matchingMethod.DeclaringType.FullName;
                        Aggregator.AddMatch(ins, method, false, context);
                    }
                }
            }
        }


    }
}
