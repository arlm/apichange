
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ApiChange.Api.Introspection.Diff
{
    public class DiffPrinter
    {
        TextWriter Out;

        /// <summary>
        /// Print diffs to console
        /// </summary>
        public DiffPrinter()
        {
            Out = Console.Out;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiffPrinter"/> class.
        /// </summary>
        /// <param name="outputStream">The output stream to print the change diff.</param>
        public DiffPrinter(TextWriter outputStream)
        {
            if (outputStream == null)
            {
                throw new ArgumentNullException("outputStream");
            }

            Out = outputStream;
        }

        internal void Print(AssemblyDiffCollection diff)
        {
            PrintAddedRemovedTypes(diff.AddedRemovedTypes);

            if (diff.ChangedTypes.Count > 0)
            {
                foreach (var typeChange in diff.ChangedTypes)
                {
                    PrintTypeChanges(typeChange);
                }
            }
        }

        private void PrintTypeChanges(TypeDiff typeChange)
        {
            Out.WriteLine("\t" + typeChange.TypeV1.Print());
            if (typeChange.HasChangedBaseType)
            {
                Out.WriteLine("\t\tBase type changed: {0} -> {1}", 
                    typeChange.TypeV1.IsNotNull( () => 
                        typeChange.TypeV1.BaseType.IsNotNull( () => typeChange.TypeV1.BaseType.FullName)),
                    typeChange.TypeV2.IsNotNull(()=> 
                        typeChange.TypeV2.BaseType.IsNotNull( () => typeChange.TypeV2.BaseType.FullName))
                );
            }

            if (typeChange.Interfaces.Count > 0)
            {
                foreach (var addedItf in typeChange.Interfaces.Added)
                {
                    Out.WriteLine("\t\t+ interface: {0}", addedItf.ObjectV1.FullName);
                }
                foreach (var removedItd in typeChange.Interfaces.Removed)
                {
                    Out.WriteLine("\t\t- interface: {0}", removedItd.ObjectV1.FullName);
                }
            }

            foreach(var addedEvent in typeChange.Events.Added)
            {
                Out.WriteLine("\t\t+ {0}", addedEvent.ObjectV1.Print());
            }

            foreach(var remEvent in typeChange.Events.Removed)
            {
                Out.WriteLine("\t\t- {0}", remEvent.ObjectV1.Print());
            }

            foreach(var addedField in typeChange.Fields.Added)
            {
                Out.WriteLine("\t\t+ {0}", addedField.ObjectV1.Print(FieldPrintOptions.All));
            }

            foreach(var remField in typeChange.Fields.Removed)
            {
                Out.WriteLine("\t\t- {0}", remField.ObjectV1.Print(FieldPrintOptions.All));
            }

            foreach(var addedMethod in typeChange.Methods.Added)
            {
                Out.WriteLine("\t\t+ {0}", addedMethod.ObjectV1.Print(MethodPrintOption.Full));
            }

            foreach(var remMethod in typeChange.Methods.Removed)
            {
                Out.WriteLine("\t\t- {0}", remMethod.ObjectV1.Print(MethodPrintOption.Full));
            }
        }

        private void PrintAddedRemovedTypes(DiffCollection<Mono.Cecil.TypeDefinition> diffCollection)
        {
            if (diffCollection.RemovedCount > 0)
            {
                Out.WriteLine("\tRemoved {0} public type/s", diffCollection.RemovedCount);
                foreach (var remType in diffCollection.Removed)
                {
                    Out.WriteLine("\t\t- {0}", remType.ObjectV1.Print());
                }
            }

            if (diffCollection.AddedCount > 0)
            {
                Out.WriteLine("\tAdded {0} public type/s", diffCollection.AddedCount);
                foreach (var addedType in diffCollection.Added)
                {
                    Out.WriteLine("\t\t+ {0}", addedType.ObjectV1.Print());
                }
            }
        }
    }
}
