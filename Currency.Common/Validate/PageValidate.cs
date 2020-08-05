using System.Text.RegularExpressions;

namespace Currency.Common
{
    /// <summary>
    /// 验证字符串是否合法
    /// </summary>
    public class PageValidate
    {
        private static Regex RegNumber = new Regex("^[0-9]+$");
        private static Regex RegNumberSign = new Regex("^[+-]?[0-9]+$");
        private static Regex RegDecimal = new Regex("^[0-9]+[.]?[0-9]+$");
        private static Regex RegDecimalSign = new Regex("^[+-]?[0-9]+[.]?[0-9]+$"); //等价于^[+-]?\d+[.]?\d+$
        private static Regex RegEmail = new Regex(@"^[\w\.]+([-]\w+)*@[A-Za-z0-9-_]+[\.][A-Za-z0-9-_]");//w 英文字母或数字的字符串，和 [a-zA-Z0-9] 语法一样 
        private static Regex RegCHZN = new Regex("[\u4e00-\u9fa5]");

        #region 数字字符串检查

        public enum CheckType
        { None, Int, SignInt, Float, SignFloat, Chinese, Mail }
        /// <summary>
        /// 检测字符串类型
        /// </summary>
        /// <param name="inputString">输入字符串</param>
        /// <param name="checktype">0:不检测| 1:数字| 2:符号数字| 3: 浮点数| 4:符号浮点| 5: 中文?| 6:邮件?</param>
        /// <returns></returns>
        public static bool CheckString(string inputString, int checktype)
        {

            bool _return = false;
            switch (checktype)
            {
                case 0:
                    _return = true;
                    break;
                case 1:
                    _return = IsNumeric(inputString);
                    break;
                case 2:
                    _return = IsNumberSign(inputString);
                    break;
                case 3:
                    _return = IsDecimal(inputString);
                    break;
                case 4:
                    _return = IsDecimalSign(inputString);
                    break;
                case 5:
                    _return = IsHasCHZN(inputString);
                    break;
                case 6:
                    _return = IsEmail(inputString);
                    break;
                default:
                    _return = false;
                    break;
            }
            return _return;
        }


        /// <summary>
        /// 判断对象是否为Int32类型的数字
        /// </summary>
        /// <param name="Expression"></param>
        /// <returns></returns>
        public static bool IsNumeric(object expression)
        {
            if (expression != null)
                return IsNumeric(expression.ToString());

            return false;
        }

        /// <summary>
        /// 判断对象是否为Int32类型的数字
        /// </summary>
        /// <param name="Expression"></param>
        /// <returns></returns>
        public static bool IsNumeric(string expression)
        {
            bool flag = false;
            if (string.IsNullOrWhiteSpace(expression))
                return flag;

            string str = expression;
            if (str.Length > 0 && str.Length <= 11 && RegNumber.IsMatch(str))
            {
                if ((str.Length < 10) || (str.Length == 10 && str[0] == '1') || (str.Length == 11 && str[0] == '-' && str[1] == '1'))
                    flag = true;
            }
            return flag;
        }

        /// <summary>
        /// 是否数字字符串 可带正负号
        /// </summary>
        /// <param name="inputString">输入字符串</param>
        /// <returns></returns>
        public static bool IsNumberSign(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return false;

            return RegNumberSign.IsMatch(expression);
        }

        /// <summary>
        /// 是否为Decimal类型
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static bool IsDecimal(object expression)
        {
            if (expression != null)
                return RegDecimal.IsMatch(expression.ToString());

            return false;
        }

        /// <summary>
        /// 是否为Double类型 可带正负号
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static bool IsDecimalSign(object expression)
        {
            if (expression != null)
                return RegDecimalSign.IsMatch(expression.ToString());//等价于^[+-]?\d+[.]?\d+$

            return false;
        }

        /// <summary>
        /// 判断给定的字符串数组(strNumber)中的数据是不是都为数值型
        /// </summary>
        /// <param name="strNumber">要确认的字符串数组</param>
        /// <returns>是则返加true 不是则返回 false</returns>
        public static bool IsNumericArray(string[] strNumber)
        {
            if (strNumber == null)
                return false;

            if (strNumber.Length < 1)
                return false;

            foreach (string id in strNumber)
            {
                if (!IsNumeric(id))
                    return false;
            }
            return true;
        }

        #endregion

        #region 中文检测 邮箱检测

        /// <summary>
        /// 检测是否有中文字符
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns></returns>
        public static bool IsHasCHZN(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return false;

            return RegCHZN.IsMatch(expression);
        }


        /// <summary>
        /// 是否是电子邮件地址
        /// </summary>
        /// <param name="inputString">输入字符串</param>
        /// <returns></returns>
        public static bool IsEmail(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return false;

            return RegEmail.IsMatch(expression);
        }

        #endregion

        #region 页面HTML格式化
        public static string GetHtml(string sDetail)
        {
            Regex r;
            Match m;
            #region 处理空格
            sDetail = sDetail.Replace(" ", "");
            #endregion
            #region 处理单引号
            sDetail = sDetail.Replace("'", "’");
            #endregion
            #region 处理双引号
            sDetail = sDetail.Replace("\"", "''");
            #endregion
            #region html标记符
            sDetail = sDetail.Replace("<", "<");
            sDetail = sDetail.Replace(">", ">");

            #endregion
            #region 处理换行
            //处理换行，在每个新行的前面添加两个全角空格
            r = new Regex(@"(\r\n(( )|　)+)(?<正文>\S+)", RegexOptions.IgnoreCase);
            for (m = r.Match(sDetail); m.Success; m = m.NextMatch())
            {
                sDetail = sDetail.Replace(m.Groups[0].ToString(), "<BR>　　" + m.Groups["正文"].ToString());
            }
            //处理换行，在每个新行的前面添加两个全角空格
            sDetail = sDetail.Replace("\r\n", "<BR>");
            #endregion

            return sDetail;
        }
        #endregion
    }
}
