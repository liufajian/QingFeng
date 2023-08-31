using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QingFeng.CoreUtils.ChineseNumber
{
    [TestClass]
    public class An2CnTest
    {
        [TestMethod]
        public void TestStringInputs()
        {
            var invalidNumbers = new[] { "-", ".", "-.", "-1.", "-1.5.", "-1.5.6", "1.", "1.5.6" };
            foreach (var n in invalidNumbers)
            {
                var msg = "failed to assert number check:" + n;
                Assert.ThrowsException<An2CnException>(() => An2CnConverter.DirectConvert(n, An2CnOutMode.lower), msg);
                Assert.ThrowsException<An2CnException>(() => An2CnConverter.ConvertToCnFormat(n, An2CnOutMode.lower), msg);
                Assert.ThrowsException<An2CnException>(() => An2CnConverter.ConvertToRmbFormat(n, An2CnOutMode.lower), msg);
            }

            var nullOrWhitespaces = new[] { "  ", " ", "\t", "", null, "\t\t" };
            foreach (var n in nullOrWhitespaces)
            {
                Assert.AreEqual(An2CnConverter.DirectConvert(n, An2CnOutMode.upper), n);
                Assert.AreEqual(An2CnConverter.ConvertToCnFormat(n, An2CnOutMode.upper), n);
                Assert.AreEqual(An2CnConverter.ConvertToRmbFormat(n, An2CnOutMode.upper), n);
            }
        }

        [TestMethod]
        public void TestDirectConvert()
        {

        }
    }
}
