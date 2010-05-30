
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BaseLibrary.StringConstQuery;

namespace DependantLibV1.WhoUsesStringConstants
{
    class UsingStringConstants
    {
        const string ConstCompoundString = "Other string" + ClassDefiningStringConstants.A;


        readonly string ReadonlyCompoundString = "else string" + ClassDefiningStringConstants.A;

        void CompareAgainstString(string input)
        {
            if (input == ClassDefiningStringConstants.A)
            {

            }
        }

        void CreateCompoundString(string str)
        {
            str += ClassDefiningStringConstants.A;
        }

        
    }
}