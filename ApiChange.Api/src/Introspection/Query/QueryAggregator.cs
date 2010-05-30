
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace ApiChange.Api.Introspection
{
    class TypeNameComparer : IEqualityComparer<TypeDefinition>
    {
        #region IEqualityComparer<TypeDefinition> Members

        public bool Equals(TypeDefinition x, TypeDefinition y)
        {
            return x.FullName == y.FullName;
        }

        public int GetHashCode(TypeDefinition obj)
        {
            return obj.Name.GetHashCode();
        }

        #endregion
    }

    class MethodComparer : IEqualityComparer<MethodDefinition>
    {

        #region IEqualityComparer<MethodDefinition> Members

        public bool Equals(MethodDefinition x, MethodDefinition y)
        {
            return x.IsEqual(y);
        }

        public int GetHashCode(MethodDefinition obj)
        {
            return obj.Name.GetHashCode();
        }

        #endregion
    }

    class FieldComparer : IEqualityComparer<FieldDefinition>
    {

        #region IEqualityComparer<FieldDefinition> Members

        public bool Equals(FieldDefinition x, FieldDefinition y)
        {
            return x.IsEqual(y);
        }

        public int GetHashCode(FieldDefinition obj)
        {
            return obj.Name.GetHashCode();
        }

        #endregion
    }

    class EventComparer : IEqualityComparer<EventDefinition>
    {
        #region IEqualityComparer<EventDefinition> Members

        public bool Equals(EventDefinition x, EventDefinition y)
        {
            return x.AddMethod.IsEqual(y.AddMethod);
        }

        public int GetHashCode(EventDefinition obj)
        {
            return obj.AddMethod.Name.GetHashCode();
        }

        #endregion
    }


    public class QueryAggregator
    {
        public List<TypeQuery> TypeQueries = new List<TypeQuery>();
        public List<MethodQuery> MethodQueries = new List<MethodQuery>();
        public List<FieldQuery> FieldQueries = new List<FieldQuery>();
        public List<EventQuery> EventQueries = new List<EventQuery>();

        /// <summary>
        /// Contains also internal types, fields and methods since the InteralsVisibleToAttribute
        /// can open visibility
        /// </summary>
        public static QueryAggregator PublicApiQueries
        {
            get
            {
                QueryAggregator agg = new QueryAggregator();

                agg.TypeQueries.Add(new TypeQuery(TypeQueryMode.ApiRelevant));

                agg.MethodQueries.Add( MethodQuery.PublicMethods );
                agg.MethodQueries.Add( MethodQuery.ProtectedMethods );

                agg.FieldQueries.Add( FieldQuery.PublicFields );
                agg.FieldQueries.Add( FieldQuery.ProtectedFields );

                agg.EventQueries.Add( EventQuery.PublicEvents );
                agg.EventQueries.Add( EventQuery.ProtectedEvents );
               

                return agg;
            }
        }

        public static QueryAggregator AllExternallyVisibleApis
        {
            get
            {
                QueryAggregator agg = PublicApiQueries;
                agg.TypeQueries.Add(new TypeQuery(TypeQueryMode.Internal));
                agg.MethodQueries.Add(MethodQuery.InternalMethods);
                agg.FieldQueries.Add(FieldQuery.InteralFields);
                agg.EventQueries.Add(EventQuery.InternalEvents);
                return agg;
            }

        }

        public QueryAggregator()
        {
        }

        public List<TypeDefinition> ExeuteAndAggregateTypeQueries(AssemblyDefinition assembly)
        {
            List<TypeDefinition> result = new List<TypeDefinition>();
            foreach (var query in TypeQueries)
            {
                result.AddRange(query.GetTypes(assembly));
            }

            var distinctResults = result;
            if (TypeQueries.Count > 1)
            {
                distinctResults = result.Distinct(new TypeNameComparer()).ToList();
            }

            return distinctResults;
        }

        public List<MethodDefinition> ExecuteAndAggregateMethodQueries(TypeDefinition type)
        {
            List<MethodDefinition> methods = new List<MethodDefinition>();
            foreach (var query in MethodQueries)
            {
                methods.AddRange(query.GetMethods(type));
            }

            var distinctResults = methods;
            if (MethodQueries.Count > 1)
            {
                distinctResults = methods.Distinct(new MethodComparer()).ToList();
            }

            return distinctResults;
        }

        public List<FieldDefinition> ExecuteAndAggregateFieldQueries(TypeDefinition type)
        {
            List<FieldDefinition> fields = new List<FieldDefinition>();
            foreach (var query in FieldQueries)
            {
                fields.AddRange(query.GetMatchingFields(type));
            }

            var distinctResults = fields;
            if (FieldQueries.Count > 1)
            {
                distinctResults = fields.Distinct(new FieldComparer()).ToList();
            }

            return distinctResults;
        }

        public List<EventDefinition> ExecuteAndAggregateEventQueries(TypeDefinition type)
        {
            List<EventDefinition> ret = new List<EventDefinition>();

            foreach (var query in EventQueries)
            {
                ret.AddRange(query.GetMatchingEvents(type));
            }

            var distinctEvents = ret;
            if( EventQueries.Count > 1 )
            {
                distinctEvents = ret.Distinct(new EventComparer()).ToList();
            }

            return distinctEvents;
        }
    }
}
