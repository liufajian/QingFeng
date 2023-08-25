//from https://github.com/dromara/hutool

using System;
using System.Text;

namespace QingFeng.SimpleUtils
{
    /// <summary>
    /// 数字转中文类，包括：
    /// 1. 数字转中文大写形式，比如一百二十一
    /// 2. 数字转金额用的大写形式，比如：壹佰贰拾壹
    /// 3. 转金额形式，比如：壹佰贰拾壹整
    /// </summary>
    public class NumberChineseFormatter
    {

        // 中文形式，奇数位置是简体，偶数位置是记账繁体，0共用<br>
        // 使用混合数组提高效率和数组复用
        private static readonly char[] DIGITS = {'零', '一', '壹', '二', '贰', '三', '叁', '四', '肆', '五', '伍',
            '六', '陆', '七', '柒', '八', '捌', '九', '玖'};

        //汉字转阿拉伯数字的
        private static readonly ChineseUnit[] CHINESE_NAME_VALUE = {
                new ChineseUnit(' ', 1, false),
                new ChineseUnit('十', 10, false),
                new ChineseUnit('拾', 10, false),
                new ChineseUnit('百', 100, false),
                new ChineseUnit('佰', 100, false),
                new ChineseUnit('千', 1000, false),
                new ChineseUnit('仟', 1000, false),
                new ChineseUnit('万', 1_0000, true),
                new ChineseUnit('亿', 1_0000_0000, true),
        };

        /// <summary>
        /// 阿拉伯数字转换成中文,小数点后四舍五入保留两位. 使用于整数、小数的转换.
        /// </summary>
        /// <param name="amount">数字</param>
        /// <param name="isUseTraditional">是否使用繁体</param>
        /// <returns></returns>
        public static string Format(double amount, bool isUseTraditional)
        {
            return Format(amount, isUseTraditional, false);
        }

        /// <summary>
        /// 阿拉伯数字转换成中文
        /// 主要是对发票票面金额转换的扩展
        /// 如：-12.32,发票票面转换为：(负数)壹拾贰圆叁角贰分
        /// 而非：负壹拾贰元叁角贰分
        /// 共两点不同：1、(负数) 而非 负；2、圆 而非 元
        /// </summary>
        /// <param name="amount">数字</param>
        /// <param name="isUseTraditional">是否使用繁体</param>
        /// <param name="isMoneyMode">是否金额模式</param>
        /// <param name="negativeName">负号转换名称 如：负、(负数)</param>
        /// <param name="unitName">单位名称 如：元、圆</param>
        /// <returns></returns>
        public static string Format(double amount, bool isUseTraditional, bool isMoneyMode, string negativeName, string unitName)
        {
            if (0 == amount)
            {
                return "零";
            }

            var chineseStr = new StringBuilder();

            // 负数
            if (amount < 0)
            {
                chineseStr.Append(negativeName ?? "负");
                amount = -amount;
            }

            long yuan = (long)Math.Round(amount * 100);
            int fen = (int)(yuan % 10);
            yuan /= 10;
            int jiao = (int)(yuan % 10);
            yuan /= 10;

            // 元
            if (false == isMoneyMode || 0 != yuan)
            {
                // 金额模式下，无需“零元”
                chineseStr.Append(LongToChinese(yuan, isUseTraditional));

                if (isMoneyMode)
                {
                    chineseStr.Append(unitName ?? "元");
                }
            }

            if (0 == jiao && 0 == fen)
            {
                //无小数部分的金额结尾
                if (isMoneyMode)
                {
                    chineseStr.Append("整");
                }
                return chineseStr.ToString();
            }

            // 小数部分
            if (false == isMoneyMode)
            {
                chineseStr.Append("点");
            }

            // 角
            if (0 == yuan && 0 == jiao)
            {
                // 元和角都为0时，只有非金额模式下补“零”
                if (false == isMoneyMode)
                {
                    chineseStr.Append("零");
                }
            }
            else
            {
                chineseStr.Append(NumberToChinese(jiao, isUseTraditional));
                if (isMoneyMode && 0 != jiao)
                {
                    chineseStr.Append("角");
                }
            }

            // 分
            if (0 != fen)
            {
                chineseStr.Append(NumberToChinese(fen, isUseTraditional));
                if (isMoneyMode)
                {
                    chineseStr.Append("分");
                }
            }

            return chineseStr.ToString();
        }

        /// <summary>
        /// 阿拉伯数字转换成中文,小数点后四舍五入保留两位. 使用于整数、小数的转换.
        /// </summary>
        /// <param name="amount">数字</param>
        /// <param name="isUseTraditional">是否使用繁体</param>
        /// <param name="isMoneyMode">是否为金额模式</param>
        /// <returns>中文</returns>
        public static string Format(double amount, bool isUseTraditional, bool isMoneyMode)
        {
            return Format(amount, isUseTraditional, isMoneyMode, "负", "元");
        }

        /// <summary>
        /// 阿拉伯数字（支持正负整数）转换成中文
        /// </summary>
        /// <param name="amount">数字</param>
        /// <param name="isUseTraditional">是否使用繁体</param>
        /// <returns>中文</returns>
        public static string Format(long amount, bool isUseTraditional)
        {
            if (0 == amount)
            {
                return "零";
            }

            var chineseStr = new StringBuilder();

            // 负数
            if (amount < 0)
            {
                chineseStr.Append("负");
                amount = -amount;
            }

            chineseStr.Append(LongToChinese(amount, isUseTraditional));
            return chineseStr.ToString();
        }

        //
        //格式化-999~999之间的数字<br>
        //这个方法显示10~19以下的数字时使用"十一"而非"一十一"。
        //
        //@param amount           数字
        //@param isUseTraditional 是否使用繁体
        //@return 中文
        //@since 5.7.17
        //
        public static string FormatThousand(int amount, bool isUseTraditional)
        {
            if (amount > 999 || amount < -999)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "Number support only: (-999 ~ 999)！");
            }

            string chinese = ThousandToChinese(amount, isUseTraditional);

            if (amount < 20 && amount >= 10)
            {
                // "十一"而非"一十一"
                return chinese.Substring(1);
            }

            return chinese;
        }

        /// <summary>
        /// 数字字符转中文，非数字字符原样返回
        /// </summary>
        /// <param name="c">数字字符</param>
        /// <param name="isUseTraditional">是否繁体</param>
        /// <returns>中文字符</returns>
        public static string NumberCharToChinese(char c, bool isUseTraditional)
        {
            if (c < '0' || c > '9')
            {
                return c.ToString();
            }
            return NumberToChinese(c - '0', isUseTraditional).ToString();
        }

        /// <summary>
        /// 中文大写数字金额转换为数字，返回结果以元为单位的BigDecimal类型数字,如：
        /// “陆万柒仟伍佰伍拾陆元叁角贰分”返回“67556.32”
        /// “叁角贰分”返回“0.32”
        /// </summary>
        /// <param name="chineseMoneyAmount">中文大写数字金额</param>
        /// <returns>返回结果以元为单位的BigDecimal类型数字</returns>
        public static decimal? ChineseMoneyToNumber(string chineseMoneyAmount)
        {
            if (string.IsNullOrEmpty(chineseMoneyAmount))
            {
                return null;
            }

            //元、角、分
            int y = 0, j = 0, f = 0;

            var span = chineseMoneyAmount.AsSpan();

            // 先找到单位为元的数字
            int yi = span.IndexOf("元");
            if (yi == -1)
            {
                yi = span.IndexOf("圆");
            }
            if (yi > 0)
            {
                y = ChineseToNumber(span.Slice(0, yi));
            }

            // 再找到单位为角的数字
            span = span.Slice(yi + 1);
            int ji = span.IndexOf("角");
            if (ji > 0)
            {
                j = ChineseToNumber(span.Slice(0, ji));
            }

            // 再找到单位为分的数字
            span = span.Slice(ji + 1);
            int fi = span.IndexOf("分");
            if (fi > 0)
            {
                f = ChineseToNumber(span.Slice(0, fi));
            }

            return (f / 100m) + (y * 100 + j);
        }

        /// <summary>
        /// 阿拉伯数字整数部分转换成中文，只支持正数
        /// </summary>
        /// <param name="amount">数字</param>
        /// <param name="isUseTraditional">是否使用繁体</param>
        /// <returns>中文</returns>
        private static string LongToChinese(long amount, bool isUseTraditional)
        {
            if (0 == amount)
            {
                return "零";
            }

            //将数字以万为单位分为多份
            int[] parts = new int[4];
            for (int i = 0; amount != 0; i++)
            {
                parts[i] = (int)(amount % 10000);
                amount = amount / 10000;
            }

            StringBuilder chineseStr = new StringBuilder();
            int partValue;
            string partChinese;

            // 千
            partValue = parts[0];
            if (partValue > 0)
            {
                partChinese = ThousandToChinese(partValue, isUseTraditional);
                chineseStr.Insert(0, partChinese);

                if (partValue < 1000)
                {
                    // 和万位之间空0，则补零，如一万零三百
                    AddPreZero(chineseStr);
                }
            }

            // 万
            partValue = parts[1];
            if (partValue > 0)
            {
                if ((partValue % 10 == 0 && parts[0] > 0))
                {
                    // 如果"万"的个位是0，则补零，如十万零八千
                    AddPreZero(chineseStr);
                }
                partChinese = ThousandToChinese(partValue, isUseTraditional);
                chineseStr.Insert(0, partChinese + "万");

                if (partValue < 1000)
                {
                    // 和亿位之间空0，则补零，如一亿零三百万
                    AddPreZero(chineseStr);
                }
            }
            else
            {
                AddPreZero(chineseStr);
            }

            // 亿
            partValue = parts[2];
            if (partValue > 0)
            {
                if ((partValue % 10 == 0 && parts[1] > 0))
                {
                    // 如果"万"的个位是0，则补零，如十万零八千
                    AddPreZero(chineseStr);
                }

                partChinese = ThousandToChinese(partValue, isUseTraditional);
                chineseStr.Insert(0, partChinese + "亿");

                if (partValue < 1000)
                {
                    // 和万亿位之间空0，则补零，如一万亿零三百亿
                    AddPreZero(chineseStr);
                }
            }
            else
            {
                AddPreZero(chineseStr);
            }

            // 万亿
            partValue = parts[3];
            if (partValue > 0)
            {
                if (parts[2] == 0)
                {
                    chineseStr.Insert(0, "亿");
                }
                partChinese = ThousandToChinese(partValue, isUseTraditional);
                chineseStr.Insert(0, partChinese + "万");
            }

            if (chineseStr.Length > 0 && '零' == chineseStr[0])
            {
                chineseStr.Remove(0, 1);
            }

            return chineseStr.ToString();
        }

        /// <summary>
        /// 把一个 0~9999 之间的整数转换为汉字的字符串，如果是 0 则返回 ""
        /// </summary>
        /// <param name="amountPart">数字部分</param>
        /// <param name="isUseTraditional">是否使用繁体单位</param>
        /// <returns>转换后的汉字</returns>
        private static string ThousandToChinese(int amountPart, bool isUseTraditional)
        {
            if (amountPart == 0)
            {
                // issue#I4R92H@Gitee
                return DIGITS[0].ToString();
            }

            int temp = amountPart;

            StringBuilder chineseStr = new StringBuilder();
            bool lastIsZero = true; // 在从低位往高位循环时，记录上一位数字是不是 0
            for (int i = 0; temp > 0; i++)
            {
                int digit = temp % 10;
                if (digit == 0)
                { // 取到的数字为 0
                    if (false == lastIsZero)
                    {
                        // 前一个数字不是 0，则在当前汉字串前加“零”字;
                        chineseStr.Insert(0, "零");
                    }
                    lastIsZero = true;
                }
                else
                { // 取到的数字不是 0
                    chineseStr.Insert(0, NumberToChinese(digit, isUseTraditional) + GetUnitName(i, isUseTraditional));
                    lastIsZero = false;
                }
                temp /= 10;
            }
            return chineseStr.ToString();
        }

        /// <summary>
        /// 把中文转换为数字 如 二百二十 220，一百一十二 -》 112，一千零一十二 -》 1012
        /// </summary>
        /// <param name="chinese">中文字符</param>
        /// <returns>数字</returns>
        /// <exception cref="InvalidCastException"></exception>
        public static int ChineseToNumber(string chinese)
        {
            return ChineseToNumber(chinese.AsSpan());
        }

        /// <summary>
        /// 把中文转换为数字 如 二百二十 220，一百一十二 -》 112，一千零一十二 -》 1012
        /// </summary>
        /// <param name="chinese">中文字符</param>
        /// <returns>数字</returns>
        /// <exception cref="InvalidCastException"></exception>
        public static int ChineseToNumber(ReadOnlySpan<char> chinese)
        {
            int length = chinese.Length;
            int result = 0;

            // 节总和
            int section = 0;
            int number = 0;
            ChineseUnit unit = null;
            char c;
            for (int i = 0; i < length; i++)
            {
                c = chinese[i];
                int num = ChineseToNumber(c);
                if (num >= 0)
                {
                    if (num == 0)
                    {
                        // 遇到零时节结束，权位失效，比如两万二零一十
                        if (number > 0 && null != unit)
                        {
                            section += number * (unit.Value / 10);
                        }
                        unit = null;
                    }
                    else if (number > 0)
                    {
                        // 多个数字同时出现，报错
                        throw new InvalidCastException($"Bad number '{chinese[i - 1]}{c}' at: {i}");
                    }
                    // 普通数字
                    number = num;
                }
                else
                {
                    unit = ChineseToUnit(c);
                    if (null == unit)
                    {
                        // 出现非法字符
                        throw new InvalidCastException($"Unknown unit '{c}' at: {i}");
                    }

                    //单位
                    if (unit.SecUnit)
                    {
                        // 节单位，按照节求和
                        section = (section + number) * unit.Value;
                        result += section;
                        section = 0;
                    }
                    else
                    {
                        // 非节单位，和单位前的单数字组合为值
                        int unitNumber = number;
                        if (0 == number && 0 == i)
                        {
                            // issue#1726，对于单位开头的数组，默认赋予1
                            // 十二 -> 一十二
                            // 百二 -> 一百二
                            unitNumber = 1;
                        }
                        section += (unitNumber * unit.Value);
                    }
                    number = 0;
                }
            }

            if (number > 0 && null != unit)
            {
                number *= unit.Value / 10;
            }

            return result + section + number;
        }

        /// <summary>
        /// 查找对应的权对象
        /// </summary>
        /// <param name="chinese">中文权位名</param>
        /// <returns>权对象</returns>
        private static ChineseUnit ChineseToUnit(char chinese)
        {
            foreach (ChineseUnit chineseNameValue in CHINESE_NAME_VALUE)
            {
                if (chineseNameValue.Name == chinese)
                {
                    return chineseNameValue;
                }
            }
            return null;
        }

        /// <summary>
        /// 将汉字单个数字转换为int类型数字
        /// </summary>
        /// <param name="chinese">汉字数字，支持简体和繁体</param>
        /// <returns>数字，-1表示未找到</returns>
        private static int ChineseToNumber(char chinese)
        {
            if ('两' == chinese)
            {
                // 口语纠正
                chinese = '二';
            }
            int i = Array.IndexOf(DIGITS, chinese);
            if (i > 0)
            {
                return (i + 1) / 2;
            }
            return i;
        }

        /// <summary>
        /// 单个数字转汉字
        /// </summary>
        /// <param name="number">数字</param>
        /// <param name="isUseTraditional">是否使用繁体</param>
        /// <returns>汉字</returns>
        private static char NumberToChinese(int number, bool isUseTraditional)
        {
            if (0 == number)
            {
                return DIGITS[0];
            }
            return DIGITS[number * 2 - (isUseTraditional ? 0 : 1)];
        }

        /// <summary>
        /// 获取对应级别的单位
        /// </summary>
        /// <param name="index">级别，0表示各位，1表示十位，2表示百位，以此类推</param>
        /// <param name="isUseTraditional">是否使用繁体</param>
        /// <returns>单位</returns>
        private static string GetUnitName(int index, bool isUseTraditional)
        {
            return 0 == index ? string.Empty : CHINESE_NAME_VALUE[index * 2 - (isUseTraditional ? 0 : 1)].Name.ToString();
        }

        private static void AddPreZero(StringBuilder chineseStr)
        {
            if (chineseStr.Length > 0)
            {
                if ('零' != chineseStr[0])
                {
                    chineseStr.Insert(0, '零');
                }
            }
        }

        private class ChineseUnit
        {
            /// <summary>
            /// 中文权名称
            /// </summary>
            public char Name { get; }

            /// <summary>
            /// 10的倍数值
            /// </summary>
            public int Value { get; }

            /// <summary>
            /// 是否为节权位，它不是与之相邻的数字的倍数，而是整个小节的倍数。
            /// 例如二十三万，万是节权位，与三无关，而和二十三关联
            /// </summary>
            public bool SecUnit { get; }

            public ChineseUnit(char name, int value, bool secUnit)
            {
                Name = name;
                Value = value;
                SecUnit = secUnit;
            }
        }
    }
}
