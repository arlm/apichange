
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BaseLibrary.ApiChanges;

namespace BaseLibrary.TypeUsageQuery
{
    class Class1<T>
    {
    }

    class Generic<PublicBaseClass>
    {


    }

    class ConcreteClass : Class1<PublicBaseClass>
    {

    }

    class FieldClass
    {
        PublicBaseClass myField;
        Func<PublicBaseClass> myGenericField;

        void Method(PublicBaseClass arg, int b)
        {

        }

        void MethodWithGenericArg(Func<int,PublicBaseClass> arg, int b, int c)
        {

        }

        PublicBaseClass MethodWithReturnType()
        {
            return null;
        }

        public static Func<PublicBaseClass> MethodWithGenericReturnType()
        {
            return null;
        }
    }

    class IntraMethodClass
    {
        void LocalInstance()
        {
            PublicBaseClass inst = null;
            inst.Dispose();
        }

        void IndirectUsage()
        {
            FieldClass.MethodWithGenericReturnType();
        }
    }

    class SwitchOfEnumValues
    {
        void UsingSwitchWithEnum()
        {
            StringSplitOptions option = StringSplitOptions.None;
            switch (option)
            {
                case StringSplitOptions.None:
                    break;
                case StringSplitOptions.RemoveEmptyEntries:
                    break;
                default:
                    throw new InvalidCastException();

            }
        }

        void CastEnumToInteger()
        {
            int castedEnum = (int) StringComparison.Ordinal;
        }
    }

    interface ITestInterface
    {
        string InterfaceProperty
        {
            get;
        }

        void MethodUsingString(string arg);
    }
}