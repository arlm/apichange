
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BaseLibrary.TypeDiff
{
    class ClassWithInterfacesAndBaseClass : EventArgs, 
        IEnumerable<string>, 
        IEnumerable<int>, 
        IDisposable
    {
        #region IEnumerable<string> Members

        public IEnumerator<string> GetEnumerator()
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

        #region IEnumerable<int> Members

        IEnumerator<int> IEnumerable<int>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}