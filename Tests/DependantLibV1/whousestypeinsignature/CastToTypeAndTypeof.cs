
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DependantLibV1.WhoUsesTypeInSignature
{
    class CastToTypeAndTypeof
    {
        object Convert(object type)
        {
            return (IDisposable)type;
        }

        void InitService(Type t)
        {

        }

        void UseService()
        {
            InitService(typeof(IDisposable));
        }
    }

}
