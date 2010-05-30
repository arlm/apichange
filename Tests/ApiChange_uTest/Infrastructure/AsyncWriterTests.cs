
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ApiChange.Api.Scripting;
using System.Reflection;
using System.Threading;
using ApiChange.Infrastructure;

namespace UnitTests.Infrastructure
{
    [TestFixture]
    public class AsyncWriterTests
    {
        [Test]
        public void Exeption_In_Async_Thread_Is_Marshalled_Back()
        {
            AsyncWriter<string> writer = new AsyncWriter<string>((str) =>
            {
                throw new InvalidOperationException("Test exception");
            });

            Assert.Throws<TargetInvocationException>(() =>
                {
                    int i = 0;
                    while (true)
                    {
                        writer.Write(i.ToString());
                        i++;
                        Thread.Sleep(10);
                    }
                });

            Assert.Throws<TargetInvocationException>(() => writer.Dispose());
        }

        [Test]
        public void Dispose_Does_Not_Throw_When_ExceptionProcessing_InProgress()
        {
            Exception ex1 = null;
            Exception ex2 = null;
            try
            {
                using (AsyncWriter<string> writer = new AsyncWriter<string>((str) =>
                {
                    throw new InvalidOperationException("Test exception");
                }))
                {
                    try
                    {
                        while (true)
                            writer.Write(null);
                    }
                    catch (Exception ex)
                    {
                        ex1 = ex;
                        throw;
                    }
                    finally
                    {
                        Assert.IsTrue(ExceptionHelper.InException);
                    }
                }
            }
            catch (Exception exx2)
            {
                ex2 = exx2;
            }

            Assert.IsNotNull(ex1);
            Assert.IsNotNull(ex2);
            Assert.IsTrue(Object.ReferenceEquals(ex1, ex2));
        }
    }
}
