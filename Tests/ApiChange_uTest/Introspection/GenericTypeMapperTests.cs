
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ApiChange.Api.Introspection;

namespace UnitTests.Introspection
{
    [TestFixture]
    public class GenericTypeMapperTests
    {
        [Test]
        public void Can_Expand_GenericTypeArgumentNames()
        {
            string expanded = GenericTypeMapper.ConvertClrTypeNames("Func<int,int,int,bool>");
            Assert.AreEqual("Func`4<System.Int32,System.Int32,System.Int32,System.Boolean>", expanded);
        }

        [Test]
        public void Can_Expand_Nested_GenericArgumentNames()
        {
            var expanded = GenericTypeMapper.ConvertClrTypeNames("Func< Func<Func<int,int>,bool> >");
            Assert.AreEqual("Func`1<Func`2<Func`2<System.Int32,System.Int32>,System.Boolean>>", expanded);
        }

        [Test]
        public void Can_Transform_Nested_Type_Arguments()
        {
            var transformed = GenericTypeMapper.TransformGenericTypeNames("Func< Func<Func<int,int>,bool> >",
                (type) => "*" + type);

            Assert.AreEqual("*Func`1<*Func`2<*Func`2<*System.Int32,*System.Int32>,*System.Boolean>>", transformed);
        }
    }
}