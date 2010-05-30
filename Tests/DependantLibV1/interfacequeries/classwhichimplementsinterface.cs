
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BaseLibrary.ApiChanges;

namespace DependantLibV1.InterfaceQueries
{
    class ClassWhichImplementsInterface : OnlyV1Interface, IDisposable,  IComparable
    {
        #region IDisposable Members

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        #endregion


        #region IComparable Members

        public int CompareTo(object obj)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}