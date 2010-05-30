
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BaseLibrary.Extensions
{
    class ClassWithGenericMethodArgs
    {
        public List<DateTime> GetImageText(int rectangle_in, bool onlyVisible_in)
        {
            return null;
        }

        public List<ClassWithGenericMethodArgs> SomeMethod(List<List<ClassWithGenericMethodArgs>> a, Dictionary<string, DateTime> b)
        {
            return null;
        }

        public void MethodWithRefAndOutParameters(ref string refStr, out int outInt)
        {
            outInt = 0;
        }
 
   }
}