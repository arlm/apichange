
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using ApiChange.Api.Introspection;
using System.IO;

    static class TestConstants
    { 
#if DEBUG
        public const string BuildMode = "Debug\\";
#else
        public const string BuildMode = "Release\\";
#endif
        public static readonly string SolutionRootDir = Directory.GetParent(Directory.GetParent(Directory.GetCurrentDirectory()).FullName).FullName;
        public static readonly string BaseLibV1 = SolutionRootDir + @"\Tests\BaseLibraryV1\bin\" + BuildMode + "BaseLibraryV1.dll";
        public static readonly string BaseLibV2 = SolutionRootDir + @"\Tests\BaseLibraryV2\bin\" + BuildMode + "BaseLibraryV1.dll";
        public static readonly string DependantLibV1 = SolutionRootDir + @"\Tests\DependantLibV1\bin\" + BuildMode + "DependantLibV1.dll";
        public static readonly string DependantLibV2 = SolutionRootDir + @"\Tests\DependantLibV2\bin\" + BuildMode + "DependantLibV1.dll";

        public const string BaseNs = "BaseLibrary";
        public const string PublicClassWithManyFields = BaseNs + ".FieldQuery." + "PublicClassWithManyFields";
        public const string GenericClass = BaseNs + ".TypeEquivalence." + "Class1<A,B,C>";

        public const string BaseLibV1Interface1Internal = "BaseLibrary.TypeQuery.Interface1Internal";

        public const string TypeEquivalenceNS = BaseNs + ".TypeEquivalence";


        public static TypeDefinition GetGenericClass(AssemblyDefinition assembly)
        {
            TypeDefinition def = TypeQuery.GetTypeByName(assembly,BaseNs + ".TypeEquivalence.Class1`3");
            return def;
        }

        public static TypeDefinition PublicBaseClassTypeV1
        {
            get
            {
                return TypeQuery.GetTypeByName(TestConstants.BaseLibV1Assembly, "BaseLibrary.ApiChanges.PublicBaseClass");
            }
        }

        static AssemblyDefinition myBaseLibV1Assembly;
        public static AssemblyDefinition BaseLibV1Assembly
        {
            get
            {
                if( myBaseLibV1Assembly == null )
                    myBaseLibV1Assembly = AssemblyFactory.GetAssembly(BaseLibV1);
                return myBaseLibV1Assembly;
            }
        }

        static AssemblyDefinition myBaseLibV2Assembly;
        public static AssemblyDefinition BaseLibV2Assembly
        {
            get
            {
                if( myBaseLibV2Assembly == null )
                    myBaseLibV2Assembly = AssemblyFactory.GetAssembly(BaseLibV2);
                return myBaseLibV2Assembly;
            }
        }

        static AssemblyDefinition myMscorlibAssembly;
        public static AssemblyDefinition MscorlibAssembly
        {
            get
            {
                if( myMscorlibAssembly == null )
                    myMscorlibAssembly = AssemblyFactory.GetAssembly(typeof(IDisposable).Assembly.CodeBase.Substring(8).Replace('/', '\\'));
                return myMscorlibAssembly;
            }
        }

        static AssemblyDefinition mySystemCoreAssembly;
        public static AssemblyDefinition SystemCoreAssembly
        {
            get
            {
                if( mySystemCoreAssembly == null )
                    mySystemCoreAssembly = AssemblyFactory.GetAssembly(typeof(Func<int>).Assembly.CodeBase.Substring(8).Replace('/', '\\'));
                return mySystemCoreAssembly;
            }
        }

        static AssemblyDefinition myDependandLibV1Assembly;
        public static AssemblyDefinition DependandLibV1Assembly
        {
            get
            {
                if( myDependandLibV1Assembly == null )
                    myDependandLibV1Assembly = AssemblyFactory.GetAssembly(DependantLibV1);
                return myDependandLibV1Assembly;
            }
        }

        static AssemblyDefinition myDependandLibV2Assembly;
        public static AssemblyDefinition DependandLibV2Assembly
        {
            get
            {
                if( myDependandLibV2Assembly == null )
                    myDependandLibV2Assembly = AssemblyFactory.GetAssembly(DependantLibV2);
                return myDependandLibV2Assembly;
            }
        }

    }
