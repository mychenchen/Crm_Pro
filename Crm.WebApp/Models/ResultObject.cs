using System.ComponentModel;

namespace Crm.WebApp.Models
{
    public class ResultObject
    {
        /// <summary>
        /// 结果类型 0错误 1正常
        /// </summary>
        public int code { get; set; }

        /// <summary>
        /// 结果说明
        /// </summary>
        public string message { get; set; }

        /// <summary>
        /// 返回时间
        /// </summary>
        public string resultTime { get; set; }

        /// <summary>
        /// 结果类型 0错误 1正常
        /// </summary>
        public object data { get; set; }
    }

    /// <summary>
    /// 错误信息枚举
    /// </summary>
    public enum ErrorCode : int
    {
        /// <summary>
        /// 失败
        /// </summary>
        [Description("失败")]
        Error = 0,

        /// <summary>
        /// 成功
        /// </summary>
        [Description("成功")]
        Success = 1,

        /// <summary>
        /// 未授权的请求,请联系管理员
        /// </summary>
        [Description("未授权的请求,请联系管理员")]
        NoAuthorize = 401,
        /// <summary>
        /// 数据不存在
        /// </summary>
        [Description("数据不存在")]
        NoExist = 404,
        /// <summary>
        /// 用户存在
        /// </summary>
        [Description("用户已存在")]
        UserExist = 402,
        /// <summary>
        /// 用户不存在或未注册
        /// </summary>
        [Description("用户不存在或未注册")]
        UserNotExist = 403,
        /// <summary>
        /// 数据为空
        /// </summary>
        [Description("数据为空")]
        DataEmpty = 405,
        /// <summary>
        /// 未授权的操作
        /// </summary>
        [Description("未授权的操作,请联系管理员")]
        NoActionAuthorize = 406,
        /// <summary>
        /// 服务器错误
        /// </summary>
        [Description("服务器错误,请稍后重试")]
        ServerError = 500,
        /// <summary>
        /// 无法连接服务器
        /// </summary>
        [Description("网络错误,请稍后重试")]
        NetworkError = 501,
        /// <summary>
        /// 参数为空
        /// </summary>
        [Description("参数为空,请检查后重试")]
        ArgumentEmpty = 10001,
        /// <summary>
        /// 参数索引溢出
        /// </summary>
        [Description("参数索引溢出,请检查后重试")]
        ArgumentOutOfIndex = 10002,
        /// <summary>
        /// 参数类型错误
        /// </summary>
        [Description("参数类型异常,请检查后重试")]
        ArgumentTypeError = 10003,
        /// <summary>
        /// 参数超出最大长度
        /// </summary>
        [Description("参数超出最大长度,请检查后重试")]
        ArgumentSizeOver = 10004,
        /// <summary>
        /// 参数值小于最大长度
        /// </summary>
        [Description("参数小于最小长度,请检查后重试")]
        ArgumentSizeMin = 10005,
        /// <summary>
        /// 发送短信失败
        /// </summary>
        [Description("发送短信失败,请联系管理员")]
        SmsFail = 11,
        /// <summary>
        /// 验证码错误
        /// </summary>
        [Description("验证码错误或已失效")]
        ValicodeError = 12,
    }
}
