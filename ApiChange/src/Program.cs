
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using ApiChange.Api.Scripting;
using ApiChange.Infrastructure;

namespace ApiChange
{
    internal class Program
    {
        static TypeHashes myType = new TypeHashes(typeof(Program));

        private static void Main(string[] args)
        {
            new Program().Execute(args);
        }

        private void Execute(string[] args)
        {
            using (Tracer t = new Tracer(myType, "Execute"))
            {
                try
                {
                    Stopwatch w = Stopwatch.StartNew();
                    AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(this.CurrentDomain_UnhandledException);
                    CommandData cmdArgs = new CommandParser().Parse(args);
                    using (var command = cmdArgs.GetCommand())
                    {
                        command.Execute();
                    }
                    t.Info("ApiChange Tool did run {0}", w.Elapsed);
                }
                catch (Exception ex)
                {
                    this.PrintException(ex);
                }
            }
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            this.PrintException((Exception)e.ExceptionObject);
        }

        private void PrintException(Exception ex)
        {
            Tracer.Error(Level.L1, myType, "PrintException", "Got Exception: {0}", ex);
            Console.WriteLine("Error {0}: {1}", ex.GetType().FullName, ex.Message);
        }
    }
}
