

using System;
namespace ApiChange.Api.Introspection
{
    interface ISymChkExecutor
    {
        bool DownLoadPdb(string fullBinaryName, string symbolServer, string downloadDir);
        System.Collections.Generic.List<string> FailedPdbs { get; set; }
        int SucceededPdbCount { get; }
    }
}
