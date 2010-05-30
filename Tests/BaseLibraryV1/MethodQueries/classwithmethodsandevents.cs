
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BaseLibrary.MethodQueries
{
    class ClassWithMethodsAndEvents
    {
        public event Func<int> Event1;
        public event Action Event2;

        public int IntProperty
        {
            get;
            set;
        }

        public void PublicMethod()
        {
        }
    }
}