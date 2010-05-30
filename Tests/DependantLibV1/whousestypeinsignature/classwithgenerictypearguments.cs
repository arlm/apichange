
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DependantLibV1.WhoUsesTypeInSignature
{
    class ClassWithGenericTypeArguments : IEnumerable<DateTime>
    {
        public Func<int,T> GenericMethod<T, V>(T arg1, V arg2)
        {
            return (Func<int,T>)null;
        }

        #region IEnumerable<DateTime> Members

        public IEnumerator<DateTime> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}