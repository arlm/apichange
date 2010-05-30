
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiChange.Infrastructure;

namespace UnitTests
{
    class TraceReset : IDisposable
    {
        string myOldTraceCfg;
        public TraceReset(string newTraceCfg)
        {
            myOldTraceCfg = TracerConfig.Reset(newTraceCfg);
        }
        #region IDisposable Members

        public void Dispose()
        {
            TracerConfig.Reset(myOldTraceCfg);
        }

        #endregion
    }
}
