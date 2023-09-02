namespace QingFeng.CoreUtils.ChineseNumber
{
    [TestClass]
    public class An2CnConverterTest
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
                Assert.ThrowsException<An2CnException>(() => An2CnConverter.ConvertToRmbFormat(n), msg);
            }

            var nullOrWhitespaces = new[] { "  ", " ", "\t", "", null, "\t\t" };
            foreach (var n in nullOrWhitespaces)
            {
                Assert.AreEqual(An2CnConverter.DirectConvert(n, An2CnOutMode.upper), n);
                Assert.AreEqual(An2CnConverter.ConvertToCnFormat(n, An2CnOutMode.upper), n);
                Assert.AreEqual(An2CnConverter.ConvertToRmbFormat(n), n);
            }

            Assert.AreEqual(An2CnConverter.DirectConvert(double.NaN, An2CnOutMode.upper), "NaN");
        }

        [TestMethod]
        public void TestConvertToRmbFormat()
        {
            foreach ((decimal d, string cs) in An2CnConverterTestData.GetRmbFmtTestData())
            {
                Assert.AreEqual(cs, An2CnConverter.ConvertToRmbFormat(d));
                if (d != 0)
                {
                    Assert.AreEqual("(负数)" + cs, An2CnConverter.ConvertToRmbFormat(-d, "(负数)"));
                }
            }
        }

        [TestMethod]
        public void TestConvertToRmbFormat2()
        {
            Assert.AreEqual("壹元整", An2CnConverter.ConvertToRmbFormat(1.001m));
        }

        [TestMethod]
        public void TestDirectConvert()
        {
            Assert.AreEqual(An2CnConverter.DirectConvert(0d, An2CnOutMode.lower), "零");
            Assert.AreEqual(An2CnConverter.DirectConvert("0", An2CnOutMode.lower), "零");
            Assert.AreEqual(An2CnConverter.DirectConvert("-0", An2CnOutMode.lower), "负零");
            Assert.AreEqual(An2CnConverter.DirectConvert("-0", An2CnOutMode.lower, "负数"), "负数零");

            Assert.AreEqual(An2CnConverter.DirectConvert(1d, An2CnOutMode.lower), "一");
            Assert.AreEqual(An2CnConverter.DirectConvert("1", An2CnOutMode.lower), "一");
            Assert.AreEqual(An2CnConverter.DirectConvert("-1", An2CnOutMode.lower, "负"), "负一");
            Assert.AreEqual(An2CnConverter.DirectConvert("-1", An2CnOutMode.lower, "负数"), "负数一");

            Assert.AreEqual(An2CnConverter.DirectConvert(12.783214569d, An2CnOutMode.lower), "一二点七八三二一四五六九");

            Assert.AreEqual(An2CnConverter.DirectConvert(98712354560.783d, An2CnOutMode.lower), "九八七一二三五四五六零点七八三");
            Assert.AreEqual(An2CnConverter.DirectConvert("98712354560.783", An2CnOutMode.lower), "九八七一二三五四五六零点七八三");
            Assert.AreEqual(An2CnConverter.DirectConvert("-98712354560.783", An2CnOutMode.lower, "负"), "负九八七一二三五四五六零点七八三");
            Assert.AreEqual(An2CnConverter.DirectConvert("-98712354560.783", An2CnOutMode.lower, "负负数"), "负负数九八七一二三五四五六零点七八三");

            Assert.AreEqual(An2CnConverter.DirectConvert(98712354560.783m, An2CnOutMode.upper), "玖捌柒壹贰叁伍肆伍陆零点柒捌叁");
            Assert.AreEqual(An2CnConverter.DirectConvert("98712354560.783", An2CnOutMode.upper), "玖捌柒壹贰叁伍肆伍陆零点柒捌叁");
            Assert.AreEqual(An2CnConverter.DirectConvert("-98712354560.783", An2CnOutMode.upper, "负"), "负玖捌柒壹贰叁伍肆伍陆零点柒捌叁");
            Assert.AreEqual(An2CnConverter.DirectConvert("-98712354560.783", An2CnOutMode.upper, "负负数"), "负负数玖捌柒壹贰叁伍肆伍陆零点柒捌叁");
        }

        [TestMethod]
        public void TestConvertToCnFormat()
        {
            foreach ((decimal d, string s) in An2CnConverterTestData.GetCnFmtTestData())
            {
                var t1 = An2CnConverter.ConvertToCnFormat(d, An2CnOutMode.upper);
                Assert.AreEqual(t1, s);
            }
        }
    }
}
