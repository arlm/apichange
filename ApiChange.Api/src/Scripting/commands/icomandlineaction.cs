
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiChange.Api.Scripting
{
    public interface ICommandLineAction : IDisposable
    {
        void Execute();
    }
}
