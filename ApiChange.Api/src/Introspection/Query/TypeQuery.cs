
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace ApiChange.Api.Introspection
{
    public static class TypeQueryExtensions
    {
        public static IEnumerable<TypeDefinition> GetMatchingTypes(this List<TypeQuery> list, AssemblyDefinition assembly)
        {
            foreach (TypeQuery query in list)
            {
                var matchingTypes = query.GetTypes(assembly);
                foreach (TypeDefinition matchingType in matchingTypes)
                {
                    yield return matchingType;
                }
            }
        }

    }

    /// <summary>
    /// Query object to search inside an assembly for specific types.
    /// </summary>
    public class TypeQuery
    {
        TypeQueryMode myQueryMode;

        /// <summary>
        /// Restrict the returned types to its visibility and or if the are
        /// compiler generated types.
        /// </summary>
        public TypeQueryMode SearchMode
        {
            get { return myQueryMode; }
            set { 
                    CheckSearchMode(value);
                    myQueryMode = value; 
                }
        }

        void CheckSearchMode(TypeQueryMode mode)
        {
            if (!IsEnabled(mode, TypeQueryMode.Internal) && !IsEnabled(mode, TypeQueryMode.Public))
            {
                throw new ArgumentException("You must set QueryMode.Internal and/or QueryMode.Public find something.");
            }

            if (!IsEnabled(mode, TypeQueryMode.Interface) && !IsEnabled(mode, TypeQueryMode.Class) && !IsEnabled(mode, TypeQueryMode.ValueType))
            {
                throw new ArgumentException("You must search for a class, interface or value type in your module. See QueryMode enumeration for values");
            }
        }

        /// <summary>
        /// Gets or sets the namespace filter. The filter string can container wild cards at
        /// the start and end of the namespace filter. 
        /// E.g. *Common (ends with) or *Common* (contains) or Common* (starts with)
        /// or Common (exact match) are possible combinations. An null filter 
        /// is treated as no filter.
        /// </summary>
        /// <value>The namespace filter.</value>
        public string NamespaceFilter
        {
            get;
            set;
        }


        string myTypeNameFilter;

        /// <summary>
        /// Get or set the type name filter. The filter string can contain wild cards at the 
        /// start and end of the filter query.
        /// </summary>
        public string TypeNameFilter
        {
            get
            {
                return myTypeNameFilter;
            }
            set
            {
                myTypeNameFilter = value;

                if( value == null )
                {
                    return;
                }

                int idx = value.IndexOf('<');
                if (idx != -1)
                {
                    int nestingCount = 0;
                    int genericParameterCount = 1;
                    for (int i = idx; i < value.Length; i++)
                    {
                        if (value[i] == '<')
                            nestingCount++;
                        if (value[i] == '>')
                            nestingCount--;
                        if (value[i] == ',')
                        {
                            genericParameterCount++;
                        }
                    }

                    myTypeNameFilter = String.Format("{0}`{1}", value.Substring(0, idx), genericParameterCount);
                }
            }
        }

        static TypeQueryFactory myQueryFactory = new TypeQueryFactory();

        /// <summary>
        /// Parse a query string which can contain a list of type queries separated by ;. A type query can be like a
        /// normal class declaration. E.g. "public class *;public interface *" would be a valid type query list.
        /// </summary>
        /// <param name="typeQueryList">Type query list</param>
        /// <param name="defaultFlags">Default query flags to use if none are part of the query. A query does not need to specify visbility falgs.</param>
        /// <returns>List of parsed queries</returns>
        /// <exception cref="ArgumentException">When the query was empty or invalid.</exception>
        /// <exception cref="ArgumentNullException">The input string was null</exception>
        public static List<TypeQuery> GetQueries(string typeQueryList, TypeQueryMode defaultFlags)
        {
            return myQueryFactory.GetQueries(typeQueryList, defaultFlags);
        }

        /// <summary>
        /// Query for all types in the assembly
        /// </summary>
        public TypeQuery(): this(TypeQueryMode.All, null)
        {
        }

        /// <summary>
        /// Query for all types in a specific namespace which can contain wildcards
        /// </summary>
        /// <param name="namespaceFilter"></param>
        public TypeQuery(string namespaceFilter):this(TypeQueryMode.All, namespaceFilter)
        {
        }

        /// <summary>
        /// Query for all types in a specific namespace with a specific name
        /// </summary>
        /// <param name="namespaceFilter"></param>
        /// <param name="typeNameFilter"></param>
        public TypeQuery(string namespaceFilter, string typeNameFilter):this(TypeQueryMode.All, namespaceFilter, typeNameFilter)
        {
        }

        /// <summary>
        /// Query for specific types like interfaces, public classes, ...
        /// </summary>
        /// <param name="mode"></param>
        public TypeQuery(TypeQueryMode mode):this(mode, null)
        {
        }

        /// <summary>
        /// Query for specific types in a namespace
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="nameSpaceFilter"></param>
        public TypeQuery(TypeQueryMode mode, string nameSpaceFilter)
            : this(mode, nameSpaceFilter, null)
        {
        }

        /// <summary>
        /// Query for specifc types in a specific namespace
        /// </summary>
        /// <param name="mode">The mode.</param>
        /// <param name="namespaceFilter">The namespace filter.</param>
        /// <param name="typeNameFilter">The type name filter.</param>
        public TypeQuery(TypeQueryMode mode, string namespaceFilter, string typeNameFilter)
        {
            myQueryMode = mode;

            // To search for nothing makes no sense. Seearch for types with any visibility
            if(!IsEnabled(TypeQueryMode.Public) && !IsEnabled(TypeQueryMode.Internal))
            {
                myQueryMode |= TypeQueryMode.Internal | TypeQueryMode.Public;
            }

            // If no type interface,struct,class is searched enable all by default
            if (!IsEnabled(TypeQueryMode.Class) && !IsEnabled(TypeQueryMode.Interface) && !IsEnabled(TypeQueryMode.ValueType) && 
                !IsEnabled(TypeQueryMode.Enum))
            {
                myQueryMode |= TypeQueryMode.Class | TypeQueryMode.Interface | TypeQueryMode.ValueType |
                               TypeQueryMode.Enum;
            }

            NamespaceFilter = namespaceFilter;
            TypeNameFilter = typeNameFilter;
        }

        /// <summary>
        /// Helper method to get only one specific type by its full qualified type name.
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static TypeDefinition GetTypeByName(AssemblyDefinition assembly, string typeName)
        {
            var types = new TypeQuery().GetTypes(assembly);
            foreach (TypeDefinition type in types)
            {
                if (type.FullName == typeName)
                {
                    return type;
                }
            }

            return null;
        }

        public IEnumerable<TypeDefinition> Filter(IEnumerable<TypeDefinition> typeList)
        {
            foreach (TypeDefinition def in typeList)
            {
                if (TypeMatchesFilter(def))
                {
                    yield return def;
                }
            }
        }

        /// <summary>
        /// Gets the types matching the current type query.
        /// </summary>
        /// <param name="assembly">The loaded Mono.Cecil assembly.</param>
        /// <returns>list of matching types</returns>
        public List<TypeDefinition> GetTypes(AssemblyDefinition assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException("assembly");
            }

            List<TypeDefinition> ret = new List<TypeDefinition>();

            foreach (ModuleDefinition module in assembly.Modules)
            {
                foreach (TypeDefinition typedef in module.Types)
                {
                    if (TypeMatchesFilter(typedef))
                    {
                        ret.Add(typedef);
                    }
                }
            }

            return ret;
        }

        bool HasCompilerGeneratedAttribute(TypeDefinition typeDef)
        {
            bool lret = false;
            if (typeDef.CustomAttributes != null)
            {
                foreach (CustomAttribute att in typeDef.CustomAttributes)
                {
                    if (att.Constructor.DeclaringType.Name == "CompilerGeneratedAttribute")
                    {
                        lret = true;
                        break;
                    }
                }
            }

            return lret;
        }

        private bool TypeMatchesFilter(TypeDefinition typeDef)
        {
            bool lret = false;

            if (CheckVisbility(typeDef))
            {
                // throw away compiler generated types
                if (IsEnabled(TypeQueryMode.NotCompilerGenerated))
                {
                    if( typeDef.IsSpecialName ||
                        typeDef.Name == "<Module>" ||
                        HasCompilerGeneratedAttribute(typeDef)
                      )
                    {
                        goto End;
                    }
                }


                // Nested types have no declaring namespace only the not nested declaring type
                // CAN have it. 
                string typeNS = typeDef.Namespace;
                TypeReference decl = typeDef.DeclaringType;
                while (decl != null && decl.Namespace == "")
                {
                    decl = decl.DeclaringType;
                }

                if (decl != null)
                {
                    typeNS = decl.Namespace;
                }

                if (!Matcher.MatchWithWildcards(NamespaceFilter, typeNS, StringComparison.OrdinalIgnoreCase))
                {
                    goto End;
                }

                if (!Matcher.MatchWithWildcards(TypeNameFilter, typeDef.Name, StringComparison.OrdinalIgnoreCase))
                {
                    goto End;
                }

                if (IsEnabled(TypeQueryMode.Interface) && typeDef.IsInterface)
                {
                    lret = true;
                }

                if (IsEnabled(TypeQueryMode.Class) && (typeDef.IsClass && !(typeDef.IsInterface || typeDef.IsValueType)))
                {
                    lret = true;
                }

                if (IsEnabled(TypeQueryMode.ValueType) && typeDef.IsValueType && !typeDef.IsEnum)
                {
                    lret = true;
                }

                if (IsEnabled(TypeQueryMode.Enum) && typeDef.IsEnum)
                {
                    lret = true;
                }

            }

        End:
            return lret;
        }

        bool IsEnabled(TypeQueryMode current, TypeQueryMode requested)
        {
            return (current & requested) == requested;
        }

        bool IsEnabled(TypeQueryMode mode)
        {
            return IsEnabled(myQueryMode, mode);
        }

        bool CheckVisbility(TypeDefinition typedef)
        {
            bool lret = false;
            if (IsEnabled(TypeQueryMode.Public) && typedef.IsPublic)
            {
                lret = true;
            }
            if (IsEnabled(TypeQueryMode.Internal) && !typedef.IsPublic )
            {
                lret = true;
            }

            return lret;
        }
    }
}
