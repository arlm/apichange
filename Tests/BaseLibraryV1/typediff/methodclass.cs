
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BaseLibrary.TypeDiff
{
    public class MethodClass
    {

        public MethodClass()
        {

        }

        public MethodClass(string a)
        {

        }

        public int IntProperty
        {
            get;
            set;
        }

        public static string StaticStringProperty
        {
            get;
            set;
        }

        public void VoidFunction()
        {

        }

        public Func<Func<bool>> FunctionWithGenericReturnValue()
        {
            return null;
        }

        public void FunctionWithOneParameter(Func<int> a)
        {

        }

        public void FunctionWithTwoParameters(int a, Func<int> b)
        {

        }

        protected IDisposable ProtectedFunction(List<DateTime> args)
        {
            return null;
        }

        public static void StaticFunc()
        {

        }

        public void PublicFuncWithOutParameter(out int val)
        {
            val = 0;
        }

    }
}