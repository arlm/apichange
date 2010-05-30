
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BaseLibrary.ApiChanges;

namespace DependantLibV1.MethodUsage
{
    class ClassWhichUsesMethods
    {
        Action func;

        public ClassWhichUsesMethods()
        {
            PublicBaseClass bc = new PublicBaseClass();
            bc.Dispose();
            IDisposable disposable = (IDisposable)bc;
            disposable.Dispose();
            PublicBaseClass.StaticMethod();
            func = PublicBaseClass.StaticMethod;
        }

        public void CallGenericIntFunc(PublicBaseClass cl)
        {
            cl.DoSomeThing((List<int>) null);
        }

        public void CallGenericFloatFunc(PublicBaseClass cl)
        {
            cl.DoSomeThing((List<float>)null);
        }

        public void CallRealGenericFunc()
        {
            var gen = new PublicGenericClass<string>((Func<int>) null);
            gen.GenericFunction<string>("");
        }

        public void CallGenericFuncWithGenericReturnType()
        {
            var gen = new PublicGenericClass<string>((Func<int>)null);
            gen.GenericFunction<int, long>(1, 2);
        }

        public void RegisterToPublicEvent(DerivedFromPublic c1)
        {
            c1.PublicEvent += new Action(c1_PublicEvent);
        }

        public void RegisterToPublicStaticEvent(DerivedFromPublic c1)
        {
            PublicBaseClass.PublicStaticEvent += c1_PublicEvent;
        }

        public void UnRegisterFromPublicStaticEvent(DerivedFromPublic c1)
        {
            PublicBaseClass.PublicStaticEvent -= c1_PublicEvent;
        }

        void c1_PublicEvent()
        {
            throw new NotImplementedException();
        }

        void CallCheckForUpdates()
        {
            PublicBaseClass cl = new PublicBaseClass();
            IDisposable var1 = null;
            IList<IDisposable> var2 = null;
            cl.CheckForUpdates(null, out var1, out var2);
        }

    }
}