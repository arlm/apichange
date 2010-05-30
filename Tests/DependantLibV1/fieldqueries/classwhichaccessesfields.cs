
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DependantLibV1.FieldQueries
{
    class ClassWhichAccessesFields
    {
        static int a;
        static string b;
        static Func<int> c;

        static ClassWhichAccessesFields()
        {
            DerivedFromPublic p = new DerivedFromPublic();
            a = p.PublicIntField;
            b = p.PublicStringField;
            c = DerivedFromPublic.PublicStaticField; 
        }

        public void PassFieldsAsFunctionArguments(DerivedFromPublic inst)
        {
            inst.PublicIntField.ToString();
            inst.PublicStringField.ToCharArray();
            DerivedFromPublic.PublicStaticField.ToString();
        }

        public void AssignFields(DerivedFromPublic inst)
        {
            inst.PublicIntField = 0;
            inst.PublicStringField = "";
            DerivedFromPublic.PublicStaticField = null;
        }
    }
}