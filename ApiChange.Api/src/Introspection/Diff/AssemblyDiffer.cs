
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace ApiChange.Api.Introspection
{   
    public class AssemblyDiffer
    {
        AssemblyDefinition myV1;
        AssemblyDefinition myV2;
        AssemblyDiffCollection myDiff = new AssemblyDiffCollection();

        public AssemblyDiffer(AssemblyDefinition v1, AssemblyDefinition v2)
        {
            if (v1 == null)
                throw new ArgumentNullException("v1");
            if (v2 == null)
                throw new ArgumentNullException("v2");

            myV1 = v1;
            myV2 = v2;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyDiffer"/> class.
        /// </summary>
        /// <param name="assemblyFileV1">The assembly file v1.</param>
        /// <param name="assemblyFileV2">The assembly file v2.</param>
        public AssemblyDiffer(string assemblyFileV1, string assemblyFileV2)
        {
            if (String.IsNullOrEmpty(assemblyFileV1))
                throw new ArgumentNullException("assemblyFileV1");
            if (String.IsNullOrEmpty(assemblyFileV2))
                throw new ArgumentNullException("assemblyFileV2");

            myV1 = AssemblyLoader.LoadCecilAssembly(assemblyFileV1);
            if (myV1 == null)
            {
                throw new ArgumentException(String.Format("Could not load assemblyV1 {0}", assemblyFileV1));
            }

            myV2 = AssemblyLoader.LoadCecilAssembly(assemblyFileV2);
            if( myV2 == null )
            {
                throw new ArgumentException(String.Format("Could not load assemblyV2 {0}", assemblyFileV2));
            }
        }

        void OnAddedType(TypeDefinition type)
        {
            var diff = new DiffResult<TypeDefinition>(type, new DiffOperation(true));
            myDiff.AddedRemovedTypes.Add(diff);
        }

        void OnRemovedType(TypeDefinition type)
        {
            var diff = new DiffResult<TypeDefinition>(type, new DiffOperation(false));
            myDiff.AddedRemovedTypes.Add(diff);
        }

        public AssemblyDiffCollection GenerateTypeDiff(QueryAggregator queries)
        {
            if (queries == null || queries.TypeQueries.Count == 0)
            {
                throw new ArgumentNullException("queries is null or contains no queries");
            }

            List<TypeDefinition> typesV1 = queries.ExeuteAndAggregateTypeQueries(myV1);
            List<TypeDefinition> typesV2 = queries.ExeuteAndAggregateTypeQueries(myV2);

            ListDiffer<TypeDefinition> differ = new ListDiffer<TypeDefinition>( ShallowTypeComapare );

            differ.Diff(typesV1, typesV2, OnAddedType, OnRemovedType);

            DiffTypes(typesV1, typesV2, queries);

            return myDiff;
        }

        bool ShallowTypeComapare(TypeDefinition v1, TypeDefinition v2)
        {
            return v1.FullName == v2.FullName;
        }


        private void DiffTypes(List<TypeDefinition> typesV1, List<TypeDefinition> typesV2,QueryAggregator queries)
        {
            TypeDefinition typeV2;
            foreach (var typeV1 in typesV1)
            {
                typeV2 = GetTypeByDefinition(typeV1, typesV2);
                if (typeV2 != null)
                {
                    TypeDiff diffed = TypeDiff.GenerateDiff(typeV1, typeV2, queries);
                    if (TypeDiff.None != diffed)
                    {
                        myDiff.ChangedTypes.Add(diffed);
                    }
                }
            }
        }

        TypeDefinition GetTypeByDefinition(TypeDefinition search, List<TypeDefinition> types)
        {
            foreach (var type in types)
            {
                if (type.IsEqual(search))
                    return type;
            }

            return null;
        }
    }
}
