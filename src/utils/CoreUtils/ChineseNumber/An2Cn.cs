using System;
using System.Text;

namespace QingFeng.CoreUtils.ChineseNumber
{
    public static class An2Cn
    {
        static readonly char[] NUMBER_LOW_AN2CN;
        static readonly char[] NUMBER_UP_AN2CN;

        static readonly char[] UNIT_LOW_ORDER_AN2CN;
        static readonly char[] UNIT_UP_ORDER_AN2CN;

        static An2Cn()
        {
            NUMBER_LOW_AN2CN = new[] { '零', '一', '二', '三', '四', '五', '六', '七', '八', '九' };
            NUMBER_UP_AN2CN = new[] { '零', '壹', '贰', '叁', '肆', '伍', '陆', '柒', '捌', '玖', };
            UNIT_LOW_ORDER_AN2CN = new[] { ' ', '十', '百', '千', '万', '十', '百', '千', '亿', '十', '百', '千', '万', '十', '百', '千' };
            UNIT_UP_ORDER_AN2CN = new[] { ' ', '拾', '佰', '仟', '万', '拾', '佰', '仟', '亿', '拾', '佰', '仟', '万', '拾', '佰', '仟' };
        }

        public enum OutMode : byte { low, up, direct, rmb }

        public static string Convert(double d, OutMode mode)
        {
            if (double.IsNaN(d))
            {
                return "NaN";
            }

            var inputs = NumberToString(d).AsSpan();

            // 判断正负
            var sign = "";
            if (inputs[0] == '-')
            {
                sign = "负";

                inputs = inputs.Slice(1);
            }

            if (mode == OutMode.direct)
            {
                return sign + DirectConvert(inputs);
            }

            // 切割整数部分和小数部分
            var dotIndex = inputs.IndexOf(".");
            var sb = new StringBuilder(30);

            if (dotIndex < 0)
            {
                sb.Append(sign);

                if (mode == OutMode.rmb)
                {
                    return IntegerConvert(inputs, OutMode.up, sb).Append("元整").ToString();
                }
                else
                {
                    return IntegerConvert(inputs, mode, sb).ToString();
                }
            }

            //包含小数的输入
            if (mode != OutMode.rmb)
            {
                sb.Append(sign);
                IntegerConvert(inputs.Slice(0, dotIndex), mode, sb);
                DecimalConvert(inputs.Slice(dotIndex + 1), mode, sb);
                return sb.ToString();
            }

            //rmb
            var len_int = IntegerConvert(inputs.Slice(0, dotIndex), OutMode.up, sb).Length;
            var len_dec = DecimalConvert(inputs.Slice(dotIndex + 1), OutMode.up, sb).Length - len_int;

            if (len_dec == 0)
            {
                return sb.Append("元整").ToString();
            }

            if (len_dec == 1)
            {
                throw new Exception("异常输出：" + inputs.Slice(dotIndex + 1).ToString());
            }

            if (len_dec > 3)
            {
                sb.Remove(len_int + 3, len_dec - 3);
            }
            else if (len_dec == 2)
            {
                sb.Append('零');
            }

            if (sb[^1] == '零')
            {
                if (sb[^2] == '零')
                {
                    return sb.Remove(len_int, 3).Append("元整").ToString();
                }
                sb[^1] = '角';
            }
            else
            {
                sb.Append('分');

                if (sb[^2] != '零')
                {
                    sb.Insert(len_int + 2, '角');
                }
            }

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

        private static string DirectConvert(ReadOnlySpan<char> inputs)
        {
            var chs = new char[inputs.Length];

            for (var i = 0; i < inputs.Length; i++)
            {
                chs[i] = inputs[i] == '.' ? '点' : NUMBER_LOW_AN2CN[inputs[i] - '0'];
            }

            return new string(chs);
        }

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

        private static StringBuilder IntegerConvert(ReadOnlySpan<char> inputs, OutMode mode, StringBuilder sb)
        {
            char[] numeral_list, unit_list;

            if (mode == OutMode.low)
            {
                numeral_list = NUMBER_LOW_AN2CN;
                unit_list = UNIT_LOW_ORDER_AN2CN;
            }
            else if (mode == OutMode.up)
            {
                numeral_list = NUMBER_UP_AN2CN;
                unit_list = UNIT_UP_ORDER_AN2CN;
            }
            else
            {
                throw new Exception("error mode: " + mode);
            }

            // 去除前面的 0，比如 007 => 7

            inputs = inputs.TrimStart('0');

            if (inputs.Length < 1)
            {
                return sb.Append('零');
            }

            var len_integer = inputs.Length;

            if (len_integer > unit_list.Length)
            {
                throw new Exception($"超出数据范围，最长支持 {unit_list.Length} 位");
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

        private static StringBuilder DecimalConvert(ReadOnlySpan<char> inputs, OutMode mode, StringBuilder sb)
        {
            var len = inputs.Length;

            if (len < 1)
            {
                return sb;
            }

            if (len > 16)
            {
                Console.WriteLine($"注意：小数部分长度为 {len} ，将自动截取前 16 位有效精度！");

                inputs = inputs.Slice(0, 16);
            }

            sb.Append('点');

            char[] numeral_list;

            if (mode == OutMode.low)
            {
                numeral_list = NUMBER_LOW_AN2CN;
            }
            else if (mode == OutMode.up)
            {
                numeral_list = NUMBER_UP_AN2CN;
            }
            else
            {
                throw new Exception("error mode: " + mode);
            }

            for (var i = 0; i < len; i++)
            {
                sb.Append(numeral_list[inputs[i] - '0']);
            }

            return sb;
        }
    }
}