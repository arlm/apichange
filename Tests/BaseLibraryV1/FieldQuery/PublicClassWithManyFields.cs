
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BaseLibrary.FieldQuery
{
    public class PublicClassWithManyFields
    {
        private int privateIntField;
        private List<string> privateListField1;
        private List<string> privateListField2;
        private const int privateConstant = 9;

        protected int protectedIntField;
        protected static readonly int protectedstaticreadonlyField = 878;
        protected const int protectedstaticconstField = 879;
        protected internal int protectedInternalField;
        protected internal static int protectedInternalStaticField;

        internal readonly int internalReadOnyInt = 10;

        public static readonly int publicReadOnlyStaticint = 100;
        public static DateTime publicStaticDateTimeField;
        public const int publicConstStaticInt = 999;
        protected internal const string protectedInternalConstString = "Test Value";
    }
}
