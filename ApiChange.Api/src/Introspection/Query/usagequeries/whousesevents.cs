
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace ApiChange.Api.Introspection
{
    public class WhoUsesEvents : UsageVisitor
    {
        List<EventDefinition> myEvents;
        HashSet<string> myEventNames = new HashSet<string>();
        public const string DefiningAssemblyKey = "Assembly";
        public const string AddEventReason = "AddEvent";
        public const string RemoveEventReason = "RemoveEvent";

        public WhoUsesEvents(UsageQueryAggregator aggregator, EventDefinition ev) :
            this(aggregator, new List<EventDefinition> { ThrowIfNull<EventDefinition>("ev", ev) })
        {
        }

        public WhoUsesEvents(UsageQueryAggregator aggreagator, List<EventDefinition> events):base(aggreagator)
        {
            if (events == null)
            {
                throw new ArgumentException("The events list was null.");
            }

            myEvents = events;

            foreach (var ev in myEvents)
            {
                Aggregator.AddVisitScope(ev.AddMethod.DeclaringType.Module.Assembly.Name.Name);
                myEventNames.Add(ev.AddMethod.Name);
                myEventNames.Add(ev.RemoveMethod.Name);
            }

        }


        string GetPrettyEventName(EventDefinition ev)
        {
            return String.Format("{0}.{1}", ev.DeclaringType.FullName, ev.Name);
        }

        MatchContext DoesMatch(MethodReference method)
        {
            MatchContext context = null;

            if (!myEventNames.Contains(method.Name))
                return context;

            foreach (EventDefinition searchEvent in myEvents)
            {
                if (method.IsEqual(searchEvent.AddMethod,false))
                {
                    context = new MatchContext(AddEventReason, GetPrettyEventName(searchEvent));
                    context[DefiningAssemblyKey] = searchEvent.DeclaringType.Module.Image.FileInformation.Name;
                    break;
                }

                if (method.IsEqual(searchEvent.RemoveMethod, false))
                {
                    context = new MatchContext(RemoveEventReason, GetPrettyEventName(searchEvent));
                    context[DefiningAssemblyKey] = searchEvent.DeclaringType.Module.Image.FileInformation.Name;
                    break;
                }
            }

            return context;
        }


        public override void VisitMethod(MethodDefinition method)
        {
            if (method.Body == null)
                return;


            MatchContext context = null;
            foreach (Instruction ins in method.Body.Instructions)
            {
                if (Code.Callvirt == ins.OpCode.Code) // normal instance call
                {
                    context = DoesMatch((MethodReference)ins.Operand);
                    if(context != null)
                    {
                        Aggregator.AddMatch(ins, method, false, context);
                    }
                }

                if (Code.Call == ins.OpCode.Code) // static function call
                {
                    context = DoesMatch((MethodReference)ins.Operand);
                    if (context != null)
                    {
                        Aggregator.AddMatch(ins, method, false, context);
                    }
                }

                if (Code.Ldftn == ins.OpCode.Code) // Delegate assignment
                {
                    context = DoesMatch((MethodReference)ins.Operand);
                    if (context != null)
                    {
                        Aggregator.AddMatch(ins, method, false, context);
                    }
                }
            }
        }
    }
}
