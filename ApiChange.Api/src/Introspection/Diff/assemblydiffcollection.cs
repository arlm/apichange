
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Mono.Cecil;

namespace ApiChange.Api.Introspection
{
    [DebuggerDisplay("Add {AddedRemovedTypes.AddedCount} Remove {AddedRemovedTypes.RemovedCount} Changed {ChangedTypes.Count}")]
    public class AssemblyDiffCollection
    {
        public DiffCollection<TypeDefinition> AddedRemovedTypes;
        public List<TypeDiff> ChangedTypes;

        public AssemblyDiffCollection()
        {
            AddedRemovedTypes = new DiffCollection<TypeDefinition>();
            ChangedTypes = new List<TypeDiff>();
        }
    }
}
