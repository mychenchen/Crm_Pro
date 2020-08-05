using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
namespace Currency.Common
{
    /// <summary>
    /// 工具类
    /// </summary>
    public class Utils
    {
        /// <summary>
        /// 生成Guid
        /// </summary>
        /// <returns></returns>
        public static string CreateGuid()
        {
            System.Threading.Thread.Sleep(3);
            string str = Guid.NewGuid().ToString("N");
            return str;
        }


        private static Regex RegexBr = new Regex(@"(\r\n)", RegexOptions.IgnoreCase);

        public static Regex RegexFont = new Regex(@"<font color=" + "\".*?\"" + @">([\s\S]+?)</font>", Utils.GetRegexCompiledOptions());

        /// <summary>
        /// 得到正则编译参数设置
        /// </summary>
        /// <returns>参数设置</returns>
        public static RegexOptions GetRegexCompiledOptions()
        {
            return RegexOptions.None;
        }

        public static bool IsCompriseStr(string str, string stringarray, string strsplit)
        {
            if (string.IsNullOrWhiteSpace(stringarray))
                return false;

            str = str.ToLower();
            string[] stringArray = Utils.SplitString(stringarray.ToLower(), strsplit);
            for (int i = 0; i < stringArray.Length; i++)
            {
                if (str.IndexOf(stringArray[i]) > -1)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 判断指定字符串在指定字符串数组中的位置
        /// </summary>
        /// <param name="strSearch">字符串</param>
        /// <param name="stringArray">字符串数组</param>
        /// <param name="caseInsensetive">是否不区分大小写, true为不区分, false为区分</param>
        /// <returns>字符串在指定字符串数组中的位置, 如不存在则返回-1</returns>
        public static int GetInArrayID(string strSearch, string[] stringArray, bool caseInsensetive)
        {
            for (int i = 0; i < stringArray.Length; i++)
            {
                if (caseInsensetive)
                {
                    if (strSearch.ToLower() == stringArray[i].ToLower())
                        return i;
                }
                else if (strSearch == stringArray[i])
                    return i;
            }
            return -1;
        }


        /// <summary>
        /// 判断指定字符串在指定字符串数组中的位置
        /// </summary>
        /// <param name="strSearch">字符串</param>
        /// <param name="stringArray">字符串数组</param>
        /// <returns>字符串在指定字符串数组中的位置, 如不存在则返回-1</returns>		
        public static int GetInArrayID(string strSearch, string[] stringArray)
        {
            return GetInArrayID(strSearch, stringArray, true);
        }

        /// <summary>
        /// 判断指定字符串是否属于指定字符串数组中的一个元素
        /// </summary>
        /// <param name="strSearch">字符串</param>
        /// <param name="stringArray">字符串数组</param>
        /// <param name="caseInsensetive">是否不区分大小写, true为不区分, false为区分</param>
        /// <returns>判断结果</returns>
        public static bool InArray(string strSearch, string[] stringArray, bool caseInsensetive)
        {
            return GetInArrayID(strSearch, stringArray, caseInsensetive) >= 0;
        }

        /// <summary>
        /// 判断指定字符串是否属于指定字符串数组中的一个元素
        /// </summary>
        /// <param name="str">字符串</param>
        /// <param name="stringarray">字符串数组</param>
        /// <returns>判断结果</returns>
        public static bool InArray(string str, string[] stringarray)
        {
            return InArray(str, stringarray, false);
        }

        /// <summary>
        /// 判断指定字符串是否属于指定字符串数组中的一个元素
        /// </summary>
        /// <param name="str">字符串</param>
        /// <param name="stringarray">内部以逗号分割单词的字符串</param>
        /// <returns>判断结果</returns>
        public static bool InArray(string str, string stringarray)
        {
            return InArray(str, SplitString(stringarray, ","), false);
        }

        /// <summary>
        /// 判断指定字符串是否属于指定字符串数组中的一个元素
        /// </summary>
        /// <param name="str">字符串</param>
        /// <param name="stringarray">内部以逗号分割单词的字符串</param>
        /// <param name="strsplit">分割字符串</param>
        /// <returns>判断结果</returns>
        public static bool InArray(string str, string stringarray, string strsplit)
        {
            return InArray(str, SplitString(stringarray, strsplit), false);
        }

        /// <summary>
        /// 判断指定字符串是否属于指定字符串数组中的一个元素
        /// </summary>
        /// <param name="str">字符串</param>
        /// <param name="stringarray">内部以逗号分割单词的字符串</param>
        /// <param name="strsplit">分割字符串</param>
        /// <param name="caseInsensetive">是否不区分大小写, true为不区分, false为区分</param>
        /// <returns>判断结果</returns>
        public static bool InArray(string str, string stringarray, string strsplit, bool caseInsensetive)
        {
            return InArray(str, SplitString(stringarray, strsplit), caseInsensetive);
        }

        /// <summary>
        /// 返回文件是否存在
        /// </summary>
        /// <param name="filename">文件名</param>
        /// <returns>是否存在</returns>
        public static bool FileExists(string filename)
        {
            return System.IO.File.Exists(filename);
        }


        /// <summary>
        /// 判断文件名是否为浏览器可以直接显示的图片文件名
        /// </summary>
        /// <param name="filename">文件名</param>
        /// <returns>是否可以直接显示</returns>
        public static bool IsImgFilename(string filename)
        {
            filename = filename.Trim();
            if (filename.EndsWith(".") || filename.IndexOf(".") == -1)
                return false;

            string extname = filename.Substring(filename.LastIndexOf(".") + 1).ToLower();
            return (extname == "jpg" || extname == "jpeg" || extname == "png" || extname == "bmp" || extname == "gif");
        }


        /// <summary>
        /// int型转换为string型
        /// </summary>
        /// <returns>转换后的string类型结果</returns>
        public static string IntToStr(int intValue)
        {
            return Convert.ToString(intValue);
        }


        /// <summary>
        /// 自定义的替换字符串函数
        /// </summary>
        public static string ReplaceString(string SourceString, string SearchString, string ReplaceString, bool IsCaseInsensetive)
        {
            return Regex.Replace(SourceString, Regex.Escape(SearchString), ReplaceString, IsCaseInsensetive ? RegexOptions.IgnoreCase : RegexOptions.None);
        }


        /// <summary>
        /// 检测是否是正确的Url
        /// </summary>
        /// <param name="strUrl">要验证的Url</param>
        /// <returns>判断结果</returns>
        public static bool IsURL(string strUrl)
        {
            return Regex.IsMatch(strUrl, @"^(http|https)\://([a-zA-Z0-9\.\-]+(\:[a-zA-Z0-9\.&%\$\-]+)*@)*((25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9])\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[0-9])|localhost|([a-zA-Z0-9\-]+\.)*[a-zA-Z0-9\-]+\.(com|edu|gov|int|mil|net|org|biz|arpa|info|name|pro|aero|coop|museum|[a-zA-Z]{1,10}))(\:[0-9]+)*(/($|[a-zA-Z0-9\.\,\?\'\\\+&%\$#\=~_\-]+))*$");
        }

        /// <summary>
        /// 检测是否有Sql危险字符
        /// </summary>
        /// <param name="str">要判断字符串</param>
        /// <returns>判断结果</returns>
        public static bool IsSafeSqlString(string str)
        {
            return !Regex.IsMatch(str, @"[;|,|\/|\(|\)|\[|\]|\}|\{|%|@|\*|!|\']");
        }

        /// <summary>
        /// 清理字符串
        /// </summary>
        public static string CleanInput(string strIn)
        {
            return Regex.Replace(strIn.Trim(), @"[^\w\.@-]", "");
        }



        /// <summary>
        /// 分割字符串
        /// </summary>
        public static string[] SplitString(string strContent, string strSplit)
        {
            if (!string.IsNullOrWhiteSpace(strContent))
            {
                if (strContent.IndexOf(strSplit) < 0)
                {
                    return new string[] { strContent };
                }
                return Regex.Split(strContent, Regex.Escape(strSplit), RegexOptions.IgnoreCase);
            }
            else
            {
                return new string[0] { };
            }
        }

        /// <summary>
        /// 分割字符串
        /// </summary>
        /// <returns>数组(string[])</returns>
        public static string[] SplitString(string strContent, string strSplit, int count)
        {
            string[] result = new string[count];
            string[] splited = SplitString(strContent, strSplit);

            for (int i = 0; i < count; i++)
            {
                if (i < splited.Length)
                    result[i] = splited[i];
                else
                    result[i] = string.Empty;
            }

            return result;
        }

        /// <summary>
        /// 返回 URL 字符串的编码结果
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns>编码结果</returns>
        public static string UrlEncode(string str)
        {
            return HttpUtility.UrlEncode(str);
        }

        /// <summary>
        /// 返回 URL 字符串的编码结果
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns>解码结果</returns>
        public static string UrlDecode(string str)
        {
            return HttpUtility.UrlDecode(str);
        }

        #region 对象序列化
        /// <summary>
        /// 对象序列化
        /// </summary>
        /// <param name="obj">任意对象</param>
        /// <returns>字符串</returns>
        public static string SerializeObject(object obj)
        {
            IFormatter formatter = new BinaryFormatter();
            string result = string.Empty;
            using (MemoryStream stream = new MemoryStream())
            {
                formatter.Serialize(stream, obj);
                byte[] byt = new byte[stream.Length];
                byt = stream.ToArray();
                //result = Encoding.UTF8.GetString(byt, 0, byt.Length); //用其他编码会有异常：SerializationException: 二进制流“0”不包含有效的 BinaryHeader。
                result = Convert.ToBase64String(byt);
                stream.Flush();
            }
            return result;
        }
        #endregion

        #region 反序列化
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns>对象</returns>
        public static object DeserializeObject(string str)
        {
            IFormatter formatter = new BinaryFormatter();
            //byte[] byt = Encoding.UTF8.GetBytes(str); //用其他编码会有异常：SerializationException: 二进制流“0”不包含有效的 BinaryHeader。
            byte[] byt = Convert.FromBase64String(str);
            object obj = null;
            using (Stream stream = new MemoryStream(byt, 0, byt.Length))
            {
                obj = formatter.Deserialize(stream);
            }
            return obj;
        }
        #endregion


        #region 比较时间差

        /// <summary>
        /// 比较两日期,返回相差分天数
        /// </summary>
        /// <param name="date1">开始</param>
        /// <param name="date2">结束</param>
        /// <returns></returns>
        public static double TimeDaysContrast(DateTime date1, DateTime date2)
        {
            TimeSpan timeSpan = date2 - date1;
            return timeSpan.TotalDays;
        }

        /// <summary>
        /// 比较两日期,返回相差小时
        /// </summary>
        /// <param name="date1">开始</param>
        /// <param name="date2">结束</param>
        /// <returns></returns>
        public static double TimeHoursContrast(DateTime date1, DateTime date2)
        {
            TimeSpan timeSpan = date2 - date1;
            return timeSpan.TotalHours;
        }

        /// <summary>
        /// 比较两日期,返回相差分钟
        /// </summary>
        /// <param name="date1">开始</param>
        /// <param name="date2">结束</param>
        /// <returns></returns>
        public static double TimeMinutesContrast(DateTime date1, DateTime date2)
        {
            TimeSpan timeSpan = date2 - date1;
            return timeSpan.TotalMinutes;
        }

        /// <summary>
        /// 比较两日期,返回相差秒数
        /// </summary>
        /// <param name="date1">开始</param>
        /// <param name="date2">结束</param>
        /// <returns></returns>
        public static double TimeSecondsContrast(DateTime date1, DateTime date2)
        {
            TimeSpan timeSpan = date2 - date1;
            return timeSpan.TotalSeconds;
        }

        #endregion

        /// <summary>
        /// 三种加密方式
        /// </summary>
        /// <param name="pwd"></param>
        /// <param name="passwordSalt">加盐</param>
        /// <param name="format"></param>
        /// <param name="resPwd"></param>
        public static string SetPassword(string pwd, string passwordSalt, PasswordFormatEnum format)
        {
            string resPwd = "";
            string generatorPass = string.Empty;
            if (format == PasswordFormatEnum.MD5)
            {
                //generatorPass = PasswordFormatEnum.MD5.ToString();
                resPwd = pwd.ToMD5().ToLower();
            }
            else if (format == PasswordFormatEnum.TWOMD5)
            {
                //generatorPass = PasswordFormatEnum.ToMD5.ToString();
                resPwd = pwd.ToMD5().ToLower().ToMD5();
            }
            else if (format == PasswordFormatEnum.MD5SALT)
            {
                //generatorPass = PasswordFormatEnum.MD5SALT.ToString();
                resPwd = string.Concat(pwd.ToMD5().ToLower(), passwordSalt).ToLower().ToMD5();
            }
            return resPwd;
        }

        /// <summary>
        /// 加密方式枚举
        /// </summary>
        public enum PasswordFormatEnum
        {
            MD5,
            TWOMD5,
            MD5SALT,
        }
    }
}
