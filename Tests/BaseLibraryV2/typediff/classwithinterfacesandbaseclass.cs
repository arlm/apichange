
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BaseLibrary.TypeDiff
{
    class ClassWithInterfacesAndBaseClass : ResolveEventArgs, 
    //   IEnumerable<string>,  // Removed interface
        IEnumerable<long>, // Changed from int to long
        IDisposable        // unchanged 
    {
        public ClassWithInterfacesAndBaseClass()
            : base("")
        {
        }
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


        #region IDisposable Members

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerable<long> Members

        IEnumerator<long> IEnumerable<long>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}