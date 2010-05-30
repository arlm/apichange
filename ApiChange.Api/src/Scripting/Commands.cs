
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiChange.Api.Scripting
{
    public enum Commands
    {
        None,
        CorFlags,
        Diff,
        MethodUsage,
        WhoImplementsInterface,
        WhoUsesType,
        WhoUsesEvent,
        WhoUsesField,
        WhoUsesStringConstant,
        WhoReferences,
        DownloadPdbs,
        ShowRebuildTargets,
        ShowStrongName,
        ShowReferences,
        GetFileInfo
    }
}
