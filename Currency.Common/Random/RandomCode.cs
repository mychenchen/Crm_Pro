using System;
using System.Text;

namespace Currency.Common
{
    /// <summary>
    /// 生成随机数
    /// </summary>
    public class RandomCode
    {
        #region 生成验证码 
        /// <summary>
        /// 验证码生成的取值范围
        /// </summary>
        private static string[] verifycodeRange = { "1","2","3","4","5","6","7","8","9",
                                                    "a","b","c","d","e","f","g",
                                                    "h",    "j","k",    "m","n",
                                                        "p","q",    "r","s","t",
                                                    "u","v","w",    "x","y"
                                                  };

        /// <summary>
        /// 生成验证码所使用的随机数发生器
        /// </summary>
        private static Random verifycodeRandom = new Random();

        /// <summary>
        /// 产生验证码
        /// </summary>
        /// <param name="len">长度</param>
        /// <param name="OnlyNum">是否仅为数字</param>
        /// <returns>string</returns>
        public static string CreateAuthStr(int len, bool OnlyNum)
        {
            int number;
            StringBuilder checkCode = new StringBuilder();

            for (int i = 0; i < len; i++)
            {
                if (!OnlyNum)
                    number = verifycodeRandom.Next(0, verifycodeRange.Length);
                else
                    number = verifycodeRandom.Next(0, 10);

                checkCode.Append(verifycodeRange[number]);
            }
            return checkCode.ToString();
        }

        #endregion

        /// <summary>
        /// 获取最新记录编码,并将编码格式成0001这种格式
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static string GetMaxCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return "1000000";
            }
            string tempCode = "";
            for (int i = 0; i < (4 - code.Length); i++)
            {
                tempCode += "0";
            }
            return tempCode + code;
        }


        /// <summary>
        /// 生成随机数字
        /// </summary>
        /// <param name="Length">生成长度</param>
        /// <param name="Sleep">是否要在生成前将当前线程阻止以避免重复</param>
        /// <returns></returns>
        public static string Number(int Length, bool Sleep)
        {
            if (Sleep)
                System.Threading.Thread.Sleep(3);
            string result = "";
            System.Random random = new Random();
            for (int i = 0; i < Length; i++)
            {
                result += random.Next(10).ToString();
            }
            return result;
        }

        /// <summary>
        /// 用户编号随机数字(7位)
        /// </summary>
        /// <returns></returns>
        public static string NumberForUserNum(string strNo)
        {
            strNo = (Convert.ToUInt32(strNo) +1).ToString();
            return strNo;
        }


        /// <summary>
        /// 订单编号
        /// </summary>
        /// <returns></returns>
        public static string NumberForOrderNo()
        {
            return Number(7, true);
        }

    }
}
