
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BaseLibrary.ApiChanges
{
    public class AddedEventClass
    {
        public event Func<string> PublicStringEvent;
    }
}