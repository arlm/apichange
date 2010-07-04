
using System;
using System.Collections.Generic;
using System.Text;
using Mono.Cecil.Pdb;
using Mono.Cecil.Cil;
using Mono.Cecil;
using System.IO;
using System.Diagnostics;
using ApiChange.Infrastructure;

namespace ApiChange.Api.Introspection
{
    public class PdbInformationReader : IDisposable
    {
        static TypeHashes myType = new TypeHashes(typeof(PdbInformationReader));
        private PdbFactory myPdbFactory = new PdbFactory();
        Dictionary<string, ISymbolReader> myFile2PdbMap = new Dictionary<string, ISymbolReader>(StringComparer.OrdinalIgnoreCase);
        HashSet<string> myFailedPdbs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        string mySymbolServer;
        PdbDownLoader myDownLoader = new PdbDownLoader();


        public PdbInformationReader()
        {

        }

        public PdbInformationReader(string symbolServer)
        {
            mySymbolServer = symbolServer;
        }

        public void ReleasePdbForModule(ModuleDefinition module)
        {
            string fileName = module.Assembly.MainModule.Image.FileInformation.FullName;
            ISymbolReader reader;
            if (myFile2PdbMap.TryGetValue(fileName, out reader))
            {
                reader.Dispose();
                myFile2PdbMap.Remove(fileName);
            }
        }


        public ISymbolReader LoadPdbForModule(ModuleDefinition module)
        {
            using (Tracer t = new Tracer(myType, "LoadPdbForModule"))
            {
                string fileName = module.Assembly.MainModule.Image.FileInformation.FullName;
                t.Info("Module file name: {0}", fileName);
                ISymbolReader reader = null;

                if (!myFile2PdbMap.TryGetValue(fileName, out reader))
                {
                    if (myFailedPdbs.Contains(fileName))
                    {
                        t.Warning("This pdb could not be successfully downloaded");
                        return reader;
                    }

                    for (int i = 0; i < 2; i++)
                    {
                        try
                        {
                            reader = myPdbFactory.CreateReader(module, fileName);
                            myFile2PdbMap[fileName] = reader;
                            break;
                        }
                        catch (Exception ex)
                        {
                            t.Error(Level.L3, ex, "Pdb did not match or it is not present");

                            string pdbFileName = Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(fileName) + ".pdb");
                            try
                            {
                                File.Delete(pdbFileName);
                            }
                            catch (Exception delex)
                            {
                                t.Error(Level.L2, delex, "Could not delete pdb {0}", pdbFileName);
                            }

                            // When we have symbol server we try to make us of it for matches.
                            if (String.IsNullOrEmpty(mySymbolServer))
                            {
                                break;
                            }

                            t.Info("Try to download pdb from symbol server {0}", mySymbolServer);
                            bool bDownloaded = myDownLoader.DownloadPdbs(new FileQuery(fileName),
                                                                    mySymbolServer);
                            t.Info("Did download pdb {0} from symbol server with return code: {1}", fileName, bDownloaded);


                            if (bDownloaded == false || i == 1) // second try did not work out as well
                            {
                                myFailedPdbs.Add(fileName);
                                break;
                            }
                        }
                    }
                }

                return reader;
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            // release pdbs
            foreach(ISymbolReader reader in myFile2PdbMap.Values)
            {
                reader.Dispose();
            }
            myFile2PdbMap.Clear();
        }

        #endregion

        /// <summary>
        /// Try to get the file name where the type is defined from the pdb via walking
        /// through some methods
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public KeyValuePair<string, int> GetFileLine(TypeDefinition type)
        {
            KeyValuePair<string, int> fileLine = new KeyValuePair<string, int>("", 0);

            for (int i = 0; i < type.Methods.Count; i++)
            {
                fileLine = GetFileLine(type.Methods[i].Body);
                if (!String.IsNullOrEmpty(fileLine.Key))
                    break;
            }
            return fileLine;
        }


        public KeyValuePair<string, int> GetFileLine(MethodDefinition method)
        {
            return GetFileLine(method.Body);
        }

        public KeyValuePair<string, int> GetFileLine(MethodBody body)
        {
            if (body != null)
            {
                var symbolReader = LoadPdbForModule(body.Method.DeclaringType.Module);
                if (symbolReader != null)
                {
                    symbolReader.Read(body);

                    foreach (Instruction ins in body.Instructions)
                    {
                        if (ins.SequencePoint != null)
                            return new KeyValuePair<string, int>(PatchDriveLetter(ins.SequencePoint.Document.Url), 0);
                    }
                }
            }

            return new KeyValuePair<string, int>("", 0);
        }

        bool HasValidFileAndLineNumber(Instruction ins)
        {
            bool lret = true;
            if( ins == null )
                lret = false;
            if( lret )
            {
                if( ins.SequencePoint == null )
                    lret = false;
            }

            if( lret )
            {
                if( ins.SequencePoint.StartLine == 0xfeefee )
                    lret = false;
            }

            return lret;
        }

        Instruction GetILInstructionWithLineNumber(Instruction ins, bool bSearchForward)
        {
            Instruction current = ins;
            if (bSearchForward)
            {
                while (current != null && !HasValidFileAndLineNumber(current))
                {
                    current = current.Next;
                }
            }
            else
            {
                while (current != null && !HasValidFileAndLineNumber(current))
                {
                    current = current.Previous;
                }
            }

            return current;
        }

        /// <summary>
        /// Get for a specific IL instruction the matching file and line.
        /// </summary>
        /// <param name="ins"></param>
        /// <param name="method"></param>
        /// <param name="bSearchForward">Search the next il instruction first if set to true for the line number from the pdb. If nothing is found we search backward.</param>
        /// <returns></returns>
        public KeyValuePair<string, int> GetFileLine(Instruction ins, MethodDefinition method, bool bSearchForward)
        {
            using (Tracer t = new Tracer(myType, "GetFileLine"))
            {
                t.Info("Try to get file and line info for {0} {1} forwardSearch {2}", method.DeclaringType.FullName, method.Name, bSearchForward);

                var symReader = LoadPdbForModule(method.DeclaringType.Module);
                if (symReader != null && method.Body != null)
                {
                    symReader.Read(method.Body);
                    Instruction current = ins;

                    if (bSearchForward)
                    {
                        current = GetILInstructionWithLineNumber(ins, true);
                        if (current == null)
                        {
                            current = GetILInstructionWithLineNumber(ins, false);
                        }
                    }
                    else
                    {
                        current = GetILInstructionWithLineNumber(ins, false);
                        if (current == null)
                        {
                            current = GetILInstructionWithLineNumber(ins, true);
                        }
                    }

                    if (current != null)
                    {
                        return new KeyValuePair<string, int>(
                            PatchDriveLetter(current.SequencePoint.Document.Url), current.SequencePoint.StartLine);
                    }
                }
                else
                {
                    t.Info("No symbol reader present or method has no body");
                }

                return new KeyValuePair<string, int>("", 0);
            }
        }   

        string PatchDriveLetter(string url)
        {
            string root = Directory.GetDirectoryRoot(Directory.GetCurrentDirectory());
            StringBuilder sb = new StringBuilder(url);
            sb[0] = root[0];
            return sb.ToString();
        }
    }
}
