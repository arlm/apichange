
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Mono.Cecil;

namespace ApiChange.Api.Introspection
{
    public class FieldQuery : BaseQuery
    {
        bool? myIsConst;
        bool? myIsReadonly;
        bool  myExcludeCompilerGeneratedFields;

        string FieldTypeFilter
        {
            get;
            set;
        }

        static FieldQuery myAllFields = new FieldQuery();
        public static FieldQuery AllFields
        {
            get { return myAllFields; }
        }

        static FieldQuery myAllFieldsIncludingCompilerGenerated = new FieldQuery("!nocompilergenerated * *");
        public static FieldQuery AllFieldsIncludingCompilerGenerated
        {
            get { return myAllFieldsIncludingCompilerGenerated; }
        }

        const string All = " * *";
        static FieldQuery myPublicFields = new FieldQuery("public " + All );
        public static FieldQuery PublicFields
        {
            get { return myPublicFields; }
        }

        static FieldQuery myProtectedFields = new FieldQuery("protected " + All);
        public static FieldQuery ProtectedFields
        {
            get { return myProtectedFields; }
        }


        static FieldQuery myInternalFields = new FieldQuery("internal " + All);
        public static FieldQuery InteralFields
        {
            get { return myInternalFields; }
        }

        static FieldQuery myPrivateFields = new FieldQuery("private " + All);
        public static FieldQuery PrivateFields
        {
            get { return myPrivateFields; }
        }

        /// <summary>
        /// Searches for all fields in a class
        /// </summary>
        public FieldQuery()
            : this("* *")
        {
        }

        /// <summary>
        /// Queries for specific fields in a class
        /// </summary>
        /// <param name="query">Query string</param>
        /// <remarks>
        /// The field query must contain at least the field type and name to query for. Access modifier
        /// are optional
        /// Example: 
        /// public * *
        /// protectd * *
        /// static readonly protected * *
        /// string m_*
        /// * my* // Get all fields which field name begins with my
        /// </remarks>
        public FieldQuery(string query):base(query)
        {
            Parser = FieldQueryParser;

            var match = Parser.Match(query);
            if (!match.Success)
            {
                throw new ArgumentException(
                    String.Format("The field query string {0} was not a valid query.", query));
            }

            myExcludeCompilerGeneratedFields = true;
            SetModifierFilter(match);
            FieldTypeFilter = GenericTypeMapper.ConvertClrTypeNames(Value(match, "fieldType"));
            
            if (!FieldTypeFilter.StartsWith("*"))
                FieldTypeFilter = "*" + FieldTypeFilter;

            if (FieldTypeFilter == "*")
                FieldTypeFilter = null;

            NameFilter = Value(match, "fieldName");
        }

        protected override void SetModifierFilter(Match match)
        {
            base.SetModifierFilter(match);
            myIsReadonly = Captures(match, "readonly");
            myIsConst = Captures(match, "const");
            bool ?excludeCompilerGenerated = Captures(match, "nocompilergenerated");
            myExcludeCompilerGeneratedFields = excludeCompilerGenerated == null ? true : excludeCompilerGenerated.Value;
        }

        public List<FieldDefinition> GetMatchingFields(TypeDefinition type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            List<FieldDefinition> retFields = new List<FieldDefinition>();

            foreach (FieldDefinition field in type.Fields)
            {
                if (Match(field,type))
                {
                    retFields.Add(field);
                }
            }

            return retFields;
        }

        internal bool Match(FieldDefinition field, TypeDefinition type)
        {
            bool lret = true;

            lret = MatchFieldModifiers(field);

            if (lret)
                lret = MatchFieldType(field);

            if (lret)
                lret = MatchName(field.Name);

            if (lret && myExcludeCompilerGeneratedFields )
                lret = !IsEventFieldOrPropertyBackingFieldOrEnumBackingField(field, type);

            return lret;
        }

        private bool IsEventFieldOrPropertyBackingFieldOrEnumBackingField(FieldDefinition field, TypeDefinition def)
        {
            // Is Property backing field
            if (field.Name.EndsWith(">k__BackingField"))
                return true;

            if (field.IsSpecialName)
                return true;

            // Is event backing field for event delegate
            foreach (EventDefinition ev in def.Events)
            {
                if (ev.Name == field.Name)
                    return true;
            }


            return false;
        }

        private bool MatchFieldType(FieldDefinition field)
        {
            if (String.IsNullOrEmpty(FieldTypeFilter) || FieldTypeFilter == "*")
            {
                return true;
            }

            return Matcher.MatchWithWildcards(this.FieldTypeFilter, field.FieldType.FullName, StringComparison.OrdinalIgnoreCase);
        }

        bool MatchFieldModifiers(FieldDefinition field)
        {
            bool lret = true;

            if (lret && myIsConst.HasValue) // Literal fields are always constant so there is no need to make a distinction here
                lret = myIsConst == field.HasConstant;

            if (lret && myIsInternal.HasValue)
                lret = myIsInternal == field.IsAssembly;

            if (lret && myIsPrivate.HasValue)
                lret = myIsPrivate == field.IsPrivate;

            if (lret && myIsProtected.HasValue)
                lret = myIsProtected == field.IsFamily;

            if (lret && myIsProtectedInernal.HasValue)
                lret = myIsProtectedInernal == field.IsFamilyOrAssembly;

            if (lret && myIsPublic.HasValue)
                lret = myIsPublic == field.IsPublic;

            if (lret && myIsReadonly.HasValue)
                lret = myIsReadonly == field.IsInitOnly;

            if (lret && myIsStatic.HasValue)
                lret = myIsStatic == (field.IsStatic && !field.HasConstant);

            return lret;
        }
    }
}
