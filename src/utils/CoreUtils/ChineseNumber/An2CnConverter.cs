using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace QingFeng.CoreUtils.ChineseNumber
{
    /// <summary>
    /// 阿拉伯数字转换成中文数字
    /// </summary>
    public static class An2CnConverter
    {
        static readonly char[] NUMBER_LOW_AN2CN;
        static readonly char[] NUMBER_UP_AN2CN;

        static readonly char[] UNIT_LOW_ORDER_AN2CN;
        static readonly char[] UNIT_UP_ORDER_AN2CN;

        static An2CnConverter()
        {
            NUMBER_LOW_AN2CN = new[] { '零', '一', '二', '三', '四', '五', '六', '七', '八', '九' };
            NUMBER_UP_AN2CN = new[] { '零', '壹', '贰', '叁', '肆', '伍', '陆', '柒', '捌', '玖', };
            UNIT_LOW_ORDER_AN2CN = new[] { ' ', '十', '百', '千', '万', '十', '百', '千', '亿', '十', '百', '千', '万', '十', '百', '千' };
            UNIT_UP_ORDER_AN2CN = new[] { ' ', '拾', '佰', '仟', '万', '拾', '佰', '仟', '亿', '拾', '佰', '仟', '万', '拾', '佰', '仟' };
        }

        #region----DirectConvert----

        /// <summary>
        /// 直接转换为中文数字
        /// </summary>
        /// <param name="inputs">要转换的输入文本</param>
        /// <param name="outMode">输出模式</param>
        /// <param name="negativeText">负号文字 如：负、(负数)</param>
        /// <returns></returns>
        private static string InnerDirectConvert(ReadOnlySpan<char> inputs, An2CnOutMode outMode, string negativeText = "负")
        {
            var len = inputs.Length;

            if (len < 1)
            {
                return string.Empty;
            }

            var numeral_list = outMode == An2CnOutMode.upper ? NUMBER_UP_AN2CN : NUMBER_LOW_AN2CN;

            char[] chs = null;

            int cIndex = 0;

            // 判断正负
            if (inputs[0] == '-')
            {
                inputs = inputs.Slice(1);

                if (!string.IsNullOrEmpty(negativeText))
                {
                    cIndex = negativeText.Length;
                    chs = new char[inputs.Length + cIndex];
                    negativeText.CopyTo(0, chs, 0, cIndex);
                }
            }

            chs ??= new char[inputs.Length];

            for (var i = 0; i < inputs.Length; i++)
            {
                chs[cIndex++] = inputs[i] == '.' ? '点' : numeral_list[inputs[i] - '0'];
            }

            return new string(chs);
        }

        /// <summary>
        /// 直接转换为中文数字
        /// </summary>
        /// <param name="d">数字</param>
        /// <param name="outMode">输出模式</param>
        /// <param name="negativeText">负号文字 如：负、(负数)</param>
        /// <returns></returns>
        public static string DirectConvert(double d, An2CnOutMode outMode, string negativeText = "负")
        {
            return double.IsNaN(d) ? "NaN" : InnerDirectConvert(NumberToString(d).AsSpan(), outMode, negativeText);
        }

        /// <summary>
        /// 直接转换为中文数字
        /// </summary>
        /// <param name="d">数字</param>
        /// <param name="outMode">输出模式</param>
        /// <param name="negativeText">负号文字 如：负、(负数)</param>
        /// <returns></returns>
        public static string DirectConvert(decimal d, An2CnOutMode outMode, string negativeText = "负")
        {
            return InnerDirectConvert(d.ToString().AsSpan(), outMode, negativeText);
        }

        /// <summary>
        /// 直接转换为中文数字
        /// </summary>
        /// <param name="s">数字字符串</param>
        /// <param name="outMode">输出模式</param>
        /// <param name="negativeText">负号文字 如：负、(负数)</param>
        /// <returns></returns>
        public static string DirectConvert(string s, An2CnOutMode outMode, string negativeText = "负")
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return s;
            }

            var ss = s.AsSpan().Trim();

            CheckInputIsValid(ss);

            return InnerDirectConvert(ss, outMode, negativeText);
        }

        #endregion

        #region----ConvertToCnFormat----

        /// <summary>
        /// 阿拉伯数字转换成中文数字格式
        /// </summary>
        /// <param name="inputs">输入的数字文本</param>
        /// <param name="outMode">输出模式</param>
        /// <param name="negativeText">负号文字 如：负、(负数)</param>
        /// <returns></returns>
        /// <exception cref="An2CnException"></exception>
        private static string InnerConvertToCnFormat(ReadOnlySpan<char> inputs, An2CnOutMode outMode, string negativeText = "负")
        {
            var sb = new StringBuilder(30);

            // 判断正负
            if (inputs[0] == '-')
            {
                inputs = inputs.Slice(1);

                sb.Append(negativeText);
            }

            // 切割整数部分和小数部分

            var dotIndex = inputs.IndexOf('.');

            if (dotIndex < 0)
            {
                return IntegerConvert(inputs, outMode, sb).ToString();
            }

            IntegerConvert(inputs.Slice(0, dotIndex), outMode, sb);

            DecimalConvert(inputs.Slice(dotIndex + 1), outMode, sb);

            return sb.ToString();
        }

        /// <summary>
        /// 阿拉伯数字转换成中文数字格式
        /// </summary>
        /// <param name="d">数字</param>
        /// <param name="outMode">输出模式</param>
        /// <param name="negativeText">负号文字 如：负、(负数)</param>
        /// <returns></returns>
        /// <exception cref="An2CnException"></exception>
        public static string ConvertToCnNumbers(double d, An2CnOutMode outMode, string negativeText = "负")
        {
            return double.IsNaN(d) ? "NaN" : InnerConvertToCnFormat(NumberToString(d).AsSpan(), outMode, negativeText);
        }

        /// <summary>
        /// 阿拉伯数字转换成中文数字格式
        /// </summary>
        /// <param name="d">数字</param>
        /// <param name="outMode">输出模式</param>
        /// <param name="negativeText">负号文字 如：负、(负数)</param>
        /// <returns></returns>
        /// <exception cref="An2CnException"></exception>
        public static string ConvertToCnNumbers(decimal d, An2CnOutMode outMode, string negativeText = "负")
        {
            return InnerConvertToCnFormat(d.ToString().AsSpan(), outMode, negativeText);
        }

        /// <summary>
        /// 阿拉伯数字转换成中文数字格式
        /// </summary>
        /// <param name="s">输入的数字文本</param>
        /// <param name="outMode">输出模式</param>
        /// <param name="negativeText">负号文字 如：负、(负数)</param>
        /// <returns></returns>
        /// <exception cref="An2CnException"></exception>
        public static string ConvertToCnFormat(string s, An2CnOutMode outMode, string negativeText = "负")
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return s;
            }

            var ss = s.AsSpan().Trim();

            CheckInputIsValid(ss);

            return InnerConvertToCnFormat(ss, outMode, negativeText);
        }

        #endregion

        #region----ConvertToRmbFmt----

        private static string InnerConvertToRmbFormat(ReadOnlySpan<char> inputs, string negativeText)
        {
            // 判断正负
            var sign = "";
            if (inputs[0] == '-')
            {
                sign = negativeText;
                inputs = inputs.Slice(1);
            }

            var sb = new StringBuilder(30);

            // 切割整数部分和小数部分
            var dotIndex = inputs.IndexOf(".");

            if (dotIndex < 0)
            {
                sb.Append(sign);

                return IntegerConvert(inputs, An2CnOutMode.upper, sb).Append("元整").ToString();
            }

            //整数部分
            var len_int = IntegerConvert(inputs.Slice(0, dotIndex), An2CnOutMode.upper, sb).Length;

            //小数部分
            var decPart = inputs.Slice(dotIndex + 1);
            if (decPart.Length > 3)
            {
                decPart = decPart.Slice(0, 3);
            }
            var len_dec = DecimalConvert(decPart, An2CnOutMode.upper, sb).Length - len_int;

            if (len_dec == 1)
            {
                throw new An2CnException("异常输出：" + decPart.ToString());
            }

            if (len_dec == 2)
            {
                sb.Append('零');
            }

            if (sb[^1] == '零')
            {
                if (sb[^2] == '零') //5.00->伍元整
                {
                    return sb.Remove(len_int, 3).Insert(0, sign).Append("元整").ToString();
                }
                sb[^1] = '角';
            }
            else
            {
                if (sb[^2] != '零')
                {
                    sb.Insert(len_int + 2, '角');
                }

                sb.Append('分');
            }

            //0.57->五角七分
            if (len_int == 1 && sb[0] == '零')
            {
                sb.Remove(0, 2);
            }
            else
            {
                sb[len_int] = '元';
            }

            return sb.Insert(0, sign).ToString();
        }

        /// <summary>
        /// 阿拉伯数字转换成中文人民币格式
        /// </summary>
        /// <param name="d">数字</param>
        /// <param name="outMode">输出模式</param>
        /// <param name="negativeText">负号文字 如：负、(负数)</param>
        /// <returns></returns>
        public static string ConvertToRmbFormat(decimal d, string negativeText = "负")
        {
            return InnerConvertToRmbFormat(d.ToString().AsSpan(), negativeText);
        }

        /// <summary>
        /// 阿拉伯数字转换成中文人民币格式
        /// </summary>
        /// <param name="s">数字文本</param>
        /// <param name="outMode">输出模式</param>
        /// <param name="negativeText">负号文字 如：负、(负数)</param>
        /// <returns></returns>
        public static string ConvertToRmbFormat(string s, string negativeText = "负")
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return s;
            }

            var ss = s.AsSpan().Trim();

            CheckInputIsValid(ss);

            return InnerConvertToRmbFormat(ss, negativeText);
        }

        #endregion

        #region----Inner Methods----

        /// <summary>
        /// double值转成文本，对科学计数法特别处理
        /// </summary>
        private static string NumberToString(double num)
        {
            var str = num.ToString();
            var index = str.IndexOf('e');

            if (index < 0)
            {
                return str;
            }

            var s1 = str.Substring(0, index);

            return str[index + 1] == '-'
                ? "0." + new string('0', int.Parse(str.Substring(index + 2)) - 1) + s1
                : s1 + new string('0', int.Parse(str.Substring(index + 1)) - 1);
        }

        /// <summary>
        /// 检查输入值有效性
        /// </summary>
        private static void CheckInputIsValid(ReadOnlySpan<char> inputs)
        {
            var ss = inputs;

            if (inputs[0] == '-')
            {
                ss = inputs.Slice(1);
            }

            if (ss.Length == 0 || ss[^1] == '.')
            {
                throw new An2CnException("Invalid Number:" + inputs.ToString());
            }

            var invalid = 0;

            foreach (var ch in ss)
            {
                if (ch < '0' || ch > '9')
                {
                    if (ch == '.')
                    {
                        invalid += 1;
                    }
                    else
                    {
                        invalid = 2;
                    }

                    if (invalid > 1)
                    {
                        throw new An2CnException("Invalid Number:" + inputs.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// 整数部分转换
        /// </summary>
        private static StringBuilder IntegerConvert(ReadOnlySpan<char> inputs, An2CnOutMode mode, StringBuilder sb)
        {
            char[] numeral_list, unit_list;

            if (mode == An2CnOutMode.lower)
            {
                numeral_list = NUMBER_LOW_AN2CN;
                unit_list = UNIT_LOW_ORDER_AN2CN;
            }
            else if (mode == An2CnOutMode.upper)
            {
                numeral_list = NUMBER_UP_AN2CN;
                unit_list = UNIT_UP_ORDER_AN2CN;
            }
            else
            {
                throw new An2CnException("error mode: " + mode);
            }

            if (inputs.Length < 1)
            {
                return sb.Append('零');
            }

            var len_integer = inputs.Length;

            if (len_integer > unit_list.Length)
            {
                throw new An2CnException($"超出数据范围，最长支持 {unit_list.Length} 位");
            }

            for (var i = 0; i < len_integer; i++)
            {
                if (i == len_integer - 1)
                {
                    if (inputs[i] != '0')
                    {
                        sb.Append(numeral_list[inputs[i] - '0']);
                    }
                }
                else if (inputs[i] != '0')
                {
                    sb.Append(numeral_list[inputs[i] - '0']).Append(unit_list[len_integer - i - 1]);
                }
                else
                {
                    if ((len_integer - i - 1) % 4 == 0)
                    {
                        var unit = unit_list[len_integer - i - 1];

                        if (sb[^1] == '零')
                        {
                            sb[^1] = unit;
                        }
                        else
                        {
                            sb.Append(unit);
                        }

                        if (unit == '万' && sb[^2] == '亿')
                        {
                            sb.Remove(sb.Length - 1, 1);
                        }
                    }

                    if (i > 0 && sb[^1] != '零')
                    {
                        sb.Append('零');
                    }
                }
            }

            if (sb[^1] == '零')
            {
                sb.Remove(sb.Length - 1, 1);
            }

            // 解决「一十几」问题
            if (sb.Length > 1 && sb[0] == '一' && sb[1] == '十')
            {
                sb.Remove(0, 1);
            }

            return sb;
        }

        /// <summary>
        /// 小数部分转换
        /// </summary>
        private static StringBuilder DecimalConvert(ReadOnlySpan<char> inputs, An2CnOutMode mode, StringBuilder sb)
        {
            var len = inputs.Length;

            if (len < 1)
            {
                return sb;
            }

            sb.Append('点');

            char[] numeral_list;

            if (mode == An2CnOutMode.lower)
            {
                numeral_list = NUMBER_LOW_AN2CN;
            }
            else if (mode == An2CnOutMode.upper)
            {
                numeral_list = NUMBER_UP_AN2CN;
            }
            else
            {
                throw new An2CnException("error mode: " + mode);
            }

            for (var i = 0; i < len; i++)
            {
                sb.Append(numeral_list[inputs[i] - '0']);
            }

            return sb;
        }

        #endregion
    }

    /// <summary>
    /// 阿拉伯数字转中文数字输出模式
    /// </summary>
    public enum An2CnOutMode
    {
        /// <summary>
        /// 中文小写数字
        /// </summary>
        lower,

        /// <summary>
        /// 中文大写数字
        /// </summary>
        upper
    }

    /// <summary>
    /// 阿拉伯数字转换成中文异常类
    /// </summary>
    public class An2CnException : Exception
    {
        public An2CnException(string message) : base(message)
        {

        }
    }
}