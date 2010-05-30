
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BaseLibrary.ApiChanges
{
    public class PublicDerivedClass1 : PublicBaseClass, IEnumerable<bool>
    {
        #region IEnumerable<int> Members

        public IEnumerator<int> GetEnumerator()
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

        #region IEnumerable<bool> Members

        IEnumerator<bool> IEnumerable<bool>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}