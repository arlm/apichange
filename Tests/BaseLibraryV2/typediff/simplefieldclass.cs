
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BaseLibrary.TypeDiff
{
    public class SimpleFieldClass
    {
        public string m_IntMember;                       // Changed
        private string m_StringMember;                   // Changed
        public Func<string> m_FuncIntMember;             // Changed
        public Func<string> m_AddedFuncStringMember;     // Added
        public DateTime m_AddedSomeOhterThing;           // Added
        public const int ReadOnlyField = 5;              // Changed
        public static readonly int StaticReadOnlyField;  // Ch  anged 
    }
}