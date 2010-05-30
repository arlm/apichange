
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BaseLibrary.TypeDiff
{
    public class SimpleFieldClass
    {
        public int m_IntMember;
        public string m_StringMember;
        public Func<int> m_FuncIntMember;
        public readonly int ReadOnlyField;
        protected static readonly int StaticReadOnlyField;

    }
}