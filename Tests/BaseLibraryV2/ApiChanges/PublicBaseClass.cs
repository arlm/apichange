
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BaseLibrary.ApiChanges
{
    public class PublicBaseClass : IDisposable
    {
        #region IDisposable Members

        public void DoSomeThing(List<long> l)
        {

        }


        public void Dispose()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}