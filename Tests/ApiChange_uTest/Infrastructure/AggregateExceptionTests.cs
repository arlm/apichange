
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using NUnit.Framework;
using ApiChange.Infrastructure;

namespace UnitTests.Infrastructure
{
    [TestFixture]
    public class AggregateExceptionTests
    {
        void Func1()
        {
            Func2();
        }

        private void Func2()
        {
            Func3();
        }

        private void Func3()
        {
            throw new NotImplementedException();
        }

        List<Exception> CreateNExceptions(int n)
        {
            List<Exception> ret = new List<Exception>();

            for (int i = 1; i <= n; i++)
            {
                try
                {
                    Func1();
                }
                catch (Exception ex)
                {
                    ret.Add(new Exception(String.Format("Exception {0}", i), ex));
                }
            }

            return ret;

        }

        [Test]
        [Explicit]
        public void Can_Store_And_Print_An_ExceptionList()
        {
            var list = CreateNExceptions(5);
            AggregateException agex = new AggregateException("Got many exceptions", list);
            string str = agex.ToString();

            Console.WriteLine(agex);
            StringAssert.Contains("Func3", str);
            StringAssert.Contains("Exception 4", str);
            StringAssert.Contains("Exception 1", str);
        }


    }

}
