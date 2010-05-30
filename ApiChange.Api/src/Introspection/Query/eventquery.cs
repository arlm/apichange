
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Mono.Cecil;
using System.ComponentModel;

namespace ApiChange.Api.Introspection
{
    public class EventQuery : MethodQuery
    {
        string EventTypeFilter
        {
            get;
            set;
        }

        static EventQuery myPublicEvents = new EventQuery("public * *");
        public static EventQuery PublicEvents
        {
            get { return myPublicEvents; }
        }

        static EventQuery myProtectedEvents = new EventQuery("protected * *");
        public static EventQuery ProtectedEvents
        {
            get { return myProtectedEvents; }
        }

        static EventQuery myInternalEvents = new EventQuery("internal * *");
        public static EventQuery InternalEvents
        {
            get { return myInternalEvents; }
        }

        static EventQuery myAllEvents = new EventQuery("* *");
        public static EventQuery AllEvents
        {
            get { return myAllEvents; }
        }


        public EventQuery():this("*")
        {
        }

        public EventQuery(string query):base("*")
        {
            if (String.IsNullOrEmpty(query))
            {
                throw new ArgumentNullException("query string was empty");
            }

            if (query == "*")
                return;

            // Get cached regex
            Parser = EventQueryParser;

            var match = Parser.Match(query);
            if(!match.Success)
            {
                throw new ArgumentException(
                    String.Format("The event query string {0} was not a valid query.", query));
            }

            SetModifierFilter(match);
            EventTypeFilter = GenericTypeMapper.ConvertClrTypeNames(Value(match, "eventType"));
            EventTypeFilter = PrependStarBeforeGenericTypes(EventTypeFilter);

            if (!EventTypeFilter.StartsWith("*"))
                EventTypeFilter = "*" + EventTypeFilter;

            if (EventTypeFilter == "*")
                EventTypeFilter = null;

            NameFilter = Value(match, "eventName");
        }

        private string PrependStarBeforeGenericTypes(string eventTypeFilter)
        {
            return eventTypeFilter.Replace("<", "<*").Replace("**", "*");
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override List<MethodDefinition> GetMethods(TypeDefinition type)
        {
            throw new NotSupportedException("The event query does not support a method match. Use GetMatchingEvents to get the query result");
        }

        public List<EventDefinition> GetMatchingEvents(TypeDefinition type)
        {
            if (type == null)
                throw new ArgumentException("The type instance to analyze was null. Can`t do that.");

            List<EventDefinition> events = new List<EventDefinition>();

            foreach (EventDefinition ev in type.Events)
            {
                if (IsMatchingEvent(ev))
                {
                    events.Add(ev);
                }
            }

            return events;
        }

        private bool IsMatchingEvent(EventDefinition ev)
        {
            bool lret = true;

            lret = MatchMethodModifiers(ev.AddMethod);
            if (lret)
            {
                lret = MatchName(ev.Name);
            }

            if (lret)
            {
                lret = MatchEventType(ev.EventType);
            }

            return lret;
        }

        private bool MatchEventType(TypeReference evType)
        {
            if (String.IsNullOrEmpty(EventTypeFilter) || EventTypeFilter == "*")
            {
                return true;
            }

            return Matcher.MatchWithWildcards(this.EventTypeFilter, evType.FullName, StringComparison.OrdinalIgnoreCase);
        }
    }
}
