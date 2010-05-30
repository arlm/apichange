
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using BaseLibrary.ApiChanges;

namespace DependantLibV1.WhoUsesTypeInSignature
{
    class ClassWithUsingStatement
    {
        PublicBaseClass m_Base;

        void FunctionWithUsingStatement()
        {
            using(var stream = new FileStream("", FileMode.Append))
            {
                Console.WriteLine("Hello");
            }
        }

        void UsingDisposeableStruct()
        {
            using (AsyncFlowControl flow = new AsyncFlowControl())
            {

            }
        }

        void WriteToField()
        {
            m_Base.PublicIntField = 1;
        }

        void ReadFromField()
        {
            int val = m_Base.PublicIntField;
        }

        void CallFunctionFromBaseClass()
        {
            m_Base.DoSomeThing((List<int>)null);
        }
    }
}
