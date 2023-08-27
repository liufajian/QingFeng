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
        public void MyTestMethod()
        {
            var tests = new[] { (1,"壹元整"), (10, "壹拾元整") , (100, "壹佰元整") , (1000, "壹仟元整") , (10000, "壹万元整") 
                , (100000, "壹拾万元整") ,(1000000,"壹佰万元整"),(10000000,"壹仟万元整"),(1_0000_0000,"壹亿元整")
                ,(10_0000_0000,"壹拾亿元整"),(100_0000_0000,"壹佰亿元整"),(1000_0000_0000,"壹仟亿元整")
                ,(10000_0000_0000,"壹万亿元整"),(10_0000_0000_0000,"壹拾万亿元整"),(100_0000_0000_0000,"壹佰万亿元整")};

            foreach((double d,string cs) in tests)
            {
                Assert.AreEqual(cs, An2Cn.Convert(d, An2Cn.OutMode.rmb));
            }
            
            Assert.AreEqual("贰拾肆万陆仟零肆拾元整", An2Cn.Convert(246040, An2Cn.OutMode.rmb));
            Assert.AreEqual("叁拾捌万零捌佰贰拾捌元捌角", An2Cn.Convert(380828.8, An2Cn.OutMode.rmb));
        }
    }
}
