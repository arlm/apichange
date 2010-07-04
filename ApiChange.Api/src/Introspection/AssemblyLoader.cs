
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Binary;
using System.IO;
using System.Diagnostics;
using ApiChange.Infrastructure;

namespace ApiChange.Api.Introspection
{
    public class AssemblyLoader
    {
        static TypeHashes myType = new TypeHashes(typeof(AssemblyLoader));

        static bool IsManagedCppAssembly(AssemblyDefinition assembly)
        {
            foreach (ModuleDefinition mod in assembly.Modules)
            {
                foreach (AssemblyNameReference assemblyRef in mod.AssemblyReferences)
                {
                    if (assemblyRef.Name == "Microsoft.VisualC")
                    {
                        // Managed C++ targets are not supported by Mono Cecil skip all targets 
                        // which reference the C-Runtime
                        return true;
                    }
                }
            }

            return false;
        }

        public static AssemblyDefinition LoadCecilAssembly(string fileName)
        {
            using (Tracer t = new Tracer(Level.L5, myType, "LoadCecilAssembly"))
            {
                if (new FileInfo(fileName).Length == 0)
                {
                    t.Info("File {0} has zero byte length", fileName);
                    return null;
                }

                try
                {
                    var assemblyDef = AssemblyFactory.GetAssembly(fileName);

                    // Managed C++ assemblies are not supported by Mono Cecil
                    if (IsManagedCppAssembly(assemblyDef))
                    {
                        t.Info("File {0} is a managed C++ assembly", fileName);
                        return null;
                    }

                    return assemblyDef;
                }
                catch (ImageFormatException) // Ignore invalid images
                {

                }
                catch (BadImageFormatException) // ignore invalid PE files
                {

                }
                catch (IndexOutOfRangeException)
                {
                    t.Info("File {0} is a managed C++ assembly", fileName);
                }
                catch (NullReferenceException) // ignore managed c++ targets
                {
                    t.Info("File {0} is a managed C++ assembly", fileName);
                }
                catch (ArgumentOutOfRangeException)
                {
                    t.Info("File {0} is a managed C++ assembly", fileName);
                }
                catch (Exception ex)
                {
                    t.Error(Level.L1, "Could not read assembly {0}: {1}", fileName, ex);
                }

                return null;
            }
        }
    }
}
