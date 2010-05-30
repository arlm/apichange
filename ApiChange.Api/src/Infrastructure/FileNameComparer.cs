
using System;
using System.Collections.Generic;
using System.IO;

namespace ApiChange.Infrastructure
{
    class FileNameComparer : IEqualityComparer<string>
    {
        #region IEqualityComparer<string> Members

        public bool Equals(string x, string y)
        {
            return String.Compare(Path.GetFileName(x), Path.GetFileName(y), StringComparison.OrdinalIgnoreCase) == 0;
        }

        public int GetHashCode(string obj)
        {
            return Path.GetFileName(obj).ToLower().GetHashCode();
        }

        #endregion
    }
}
