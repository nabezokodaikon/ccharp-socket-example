using CommonLib.Command;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonLibTest.Command
{
    [TestFixture]
    class ExampleCommandTest
    {
        [Test]
        public void InterconversionTest()
        {
            var src = new ExampleCommand("あいうえお");
            var bin = src.ToBinary();
            var dest = new ExampleCommand(bin);
            Assert.AreEqual(src.Contents, dest.Contents);
        }
    }
}
