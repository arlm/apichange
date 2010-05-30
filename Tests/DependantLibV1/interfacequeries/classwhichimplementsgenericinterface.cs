
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DependantLibV1.InterfaceQueries
{
    class ClassWhichImplementsGenericInterface : IComparable<string>
    {
        #region IComparable<string> Members

        public int CompareTo(string other)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}