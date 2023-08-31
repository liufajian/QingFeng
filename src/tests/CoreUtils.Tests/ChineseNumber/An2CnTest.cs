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
                Assert.ThrowsException<An2CnException>(() => An2CnConverter.ConvertToRmbFormat(n), msg);
            }

            var nullOrWhitespaces = new[] { "  ", " ", "\t", "", null, "\t\t" };
            foreach (var n in nullOrWhitespaces)
            {
                Assert.AreEqual(An2CnConverter.DirectConvert(n, An2CnOutMode.upper), n);
                Assert.AreEqual(An2CnConverter.ConvertToCnFormat(n, An2CnOutMode.upper), n);
                Assert.AreEqual(An2CnConverter.ConvertToRmbFormat(n), n);
            }

            Assert.AreEqual(An2CnConverter.DirectConvert(double.NaN, An2CnOutMode.upper), "Nan");
        }

        [TestMethod]
        public void TestConvertToRmbFormat()
        {
            Assert.AreEqual("壹万壹仟零壹元整", An2CnConverter.ConvertToRmbFormat(11001));

            var tests = new[] {(0,"零元整"), (1,"壹元整"), (10, "壹拾元整") , (100, "壹佰元整") , (1000, "壹仟元整") , (10000, "壹万元整")
                , (100000, "壹拾万元整") ,(1000000,"壹佰万元整"),(10000000,"壹仟万元整"),(1_0000_0000,"壹亿元整")
                ,(10_0000_0000,"壹拾亿元整"),(100_0000_0000,"壹佰亿元整"),(1000_0000_0000,"壹仟亿元整")
                ,(10000_0000_0000,"壹万亿元整"),(10_0000_0000_0000,"壹拾万亿元整"),(100_0000_0000_0000,"壹佰万亿元整")};

            foreach ((double d, string cs) in tests)
            {
                Assert.AreEqual(cs, An2CnConverter.ConvertToRmbFormat((decimal)d));
            }

            var tests2 = new (decimal, string)[]{(0m,"零元整"),(1,"壹元整"),(10,"壹拾元整"),(110,"壹佰壹拾元整"),(1000,"壹仟元整"),(1001,"壹仟零壹元整")
                ,(1011,"壹仟零壹拾壹元整"),(1101,"壹仟壹佰零壹元整"),(1234,"壹仟贰佰叁拾肆元整"),(100000,"壹拾万元整"),(100001,"壹拾万零壹元整")
                ,(100101,"壹拾万零壹佰零壹元整"),(110100,"壹拾壹万零壹佰元整"),(123456,"壹拾贰万叁仟肆佰伍拾陆元整"),(10000000,"壹仟万元整")
                ,(10100100,"壹仟零壹拾万零壹佰元整"),(11010000,"壹仟壹佰零壹万元整"),(11010100,"壹仟壹佰零壹万零壹佰元整")
                ,(11010101,"壹仟壹佰零壹万零壹佰零壹元整"),(12345678,"壹仟贰佰叁拾肆万伍仟陆佰柒拾捌元整"),(1000000000,"壹拾亿元整")
                ,(1000000001,"壹拾亿零壹元整"),(1000001000,"壹拾亿零壹仟元整"),(1000001001,"壹拾亿零壹仟零壹元整")
                ,(1000011001,"壹拾亿零壹万壹仟零壹元整"),(1001001001,"壹拾亿零壹佰万零壹仟零壹元整"),(1001011001,"壹拾亿零壹佰零壹万壹仟零壹元整")
                ,(1011011001,"壹拾亿零壹仟壹佰零壹万壹仟零壹元整"),(100111011001,"壹仟零壹亿壹仟壹佰零壹万壹仟零壹元整"),(101011011001,"壹仟零壹拾亿零壹仟壹佰零壹万壹仟零壹元整")
                ,(111111111111,"壹仟壹佰壹拾壹亿壹仟壹佰壹拾壹万壹仟壹佰壹拾壹元整"),(123456789012,"壹仟贰佰叁拾肆亿伍仟陆佰柒拾捌万玖仟零壹拾贰元整")
                ,(1234567890,"壹拾贰亿叁仟肆佰伍拾陆万柒仟捌佰玖拾元整"),(1.11m,"壹元壹角壹分")
                ,(1.10m,"壹元壹角"),(1.01m,"壹元零壹分"),(1.00m,"壹元整"),(1.001m,"壹元整"),(1.155m,"壹元壹角伍分"),(0.55m,"伍角伍分")};

            foreach ((decimal d, string cs) in tests2)
            {
                Assert.AreEqual(cs, An2CnConverter.ConvertToRmbFormat(d));
            }
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
    }
}
