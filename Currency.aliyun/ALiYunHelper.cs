using Aliyun.Acs.Core;
using Aliyun.Acs.Core.Exceptions;
using Aliyun.Acs.Core.Profile;
using Aliyun.Acs.Dysmsapi.Model.V20170525;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Currency.aliyun
{
    /// <summary>
    /// 阿里云帮助类
    /// </summary>
    public static class ALiYunHelper
    {
        //产品名称:云通信短信API产品,开发者无需替换
        const string product = "Dysmsapi";
        //产品域名,开发者无需替换
        const string domain = "dysmsapi.aliyuncs.com";

        // TODO 此处需要替换成开发者自己的AK(在阿里云访问控制台寻找)
        const string accessKeyId = "yourAccessKeyId";
        const string accessKeySecret = "yourAccessKeySecret";

        #region 单个发送(支持在一次请求中向多个不同的手机号码发送同样内容的短信)

        /// <summary>
        /// demo
        /// </summary>
        /// <returns></returns>
        public static SendSmsResponse demo_sendSms()
        {
            IClientProfile profile = DefaultProfile.GetProfile("cn-hangzhou", accessKeyId, accessKeySecret);
            DefaultProfile.AddEndpoint("cn-hangzhou", "cn-hangzhou", product, domain);
            IAcsClient acsClient = new DefaultAcsClient(profile);
            SendSmsRequest request = new SendSmsRequest();
            SendSmsResponse response = null;
            try
            {

                //必填:待发送手机号。支持以逗号分隔的形式进行批量调用，批量上限为1000个手机号码,批量调用相对于单条调用及时性稍有延迟,验证码类型的短信推荐使用单条调用的方式
                request.PhoneNumbers = "15000000000";
                //必填:短信签名-可在短信控制台中找到
                request.SignName = "云通信";
                //必填:短信模板-可在短信控制台中找到
                request.TemplateCode = "SMS_1000000";
                //可选:模板中的变量替换JSON串,如模板内容为"亲爱的${name},您的验证码为${code}"时,此处的值为
                request.TemplateParam = "{\"customer\":\"123\"}";
                //可选:outId为提供给业务方扩展字段,最终在短信回执消息中将此值带回给调用者
                request.OutId = "yourOutId";
                //请求失败这里会抛ClientException异常
                response = acsClient.GetAcsResponse(request);

            }
            catch (ServerException e)
            {
                response.Code = e.ErrorCode;
                response.Message = e.ErrorMessage;
            }
            catch (ClientException e)
            {
                response.Code = e.ErrorCode;
                response.Message = e.ErrorMessage;
            }
            return response;

        }

        /// <summary>
        /// 发送短信
        /// </summary>
        /// <param name="tels">待发送手机号
        /// 支持对多个手机号码发送短信，手机号码之间以英文逗号（,）分隔。上限为1000个手机号码
        /// </param>
        /// <param name="signName">短信签名-可在短信控制台中找到</param>
        /// <param name="temCode">短信模板-可在短信控制台中找到</param>
        /// <param name="temParam">
        /// 可选:模板中的变量替换JSON串,如模板内容为"亲爱的${name},您的验证码为${code}"时,此处的值为
        /// {\"name\":\"张三\",\"code\":\"123\"}
        /// </param>
        /// <param name="outId">可选:为提供给业务方扩展字段,最终在短信回执消息中将此值带回给调用者</param>
        /// <returns></returns>
        public static SendSmsResponse Send_Sms(string tels, string signName, string temCode, string temParam = "", string outId = "")
        {
            IClientProfile profile = DefaultProfile.GetProfile("cn-hangzhou", accessKeyId, accessKeySecret);
            DefaultProfile.AddEndpoint("cn-hangzhou", "cn-hangzhou", product, domain);
            IAcsClient acsClient = new DefaultAcsClient(profile);
            SendSmsRequest request = new SendSmsRequest();
            SendSmsResponse response = null;
            try
            {
                //必填:待发送手机号。支持以逗号分隔的形式进行批量调用，批量上限为1000个手机号码,
                //批量调用相对于单条调用及时性稍有延迟,验证码类型的短信推荐使用单条调用的方式
                request.PhoneNumbers = tels;
                //必填:短信签名-可在短信控制台中找到
                request.SignName = signName;
                //必填:短信模板-可在短信控制台中找到
                request.TemplateCode = temCode;
                //可选:模板中的变量替换JSON串,如模板内容为"亲爱的${name},您的验证码为${code}"时,此处的值为
                request.TemplateParam = temParam;
                //可选:outId为提供给业务方扩展字段,最终在短信回执消息中将此值带回给调用者
                request.OutId = outId;
                //请求失败这里会抛ClientException异常
                response = acsClient.GetAcsResponse(request);

            }
            catch (ServerException e)
            {
                response.Code = e.ErrorCode;
                response.Message = e.ErrorMessage;
            }
            catch (ClientException e)
            {
                response.Code = e.ErrorCode;
                response.Message = e.ErrorMessage;
            }
            return response;

        }

        #endregion

        #region 批量发送(支持在一次请求中分别向多个不同的手机号码发送不同签名的短信)

        /// <summary>
        /// demo
        /// </summary>
        /// <returns></returns>
        public static SendBatchSmsResponse demo_sendBatchSms()
        {
            IClientProfile profile = DefaultProfile.GetProfile("cn-hangzhou", accessKeyId, accessKeySecret);
            DefaultProfile.AddEndpoint("cn-hangzhou", "cn-hangzhou", product, domain);

            IAcsClient acsClient = new DefaultAcsClient(profile);
            SendBatchSmsRequest request = new SendBatchSmsRequest();
            //request.Protocol = ProtocolType.HTTPS;
            //request.TimeoutInMilliSeconds = 1;

            SendBatchSmsResponse response = null;
            try
            {

                //必填:待发送手机号。支持JSON格式的批量调用，批量上限为100个手机号码,批量调用相对于单条调用及时性稍有延迟,验证码类型的短信推荐使用单条调用的方式
                request.PhoneNumberJson = "[\"1500000000\",\"1500000001\"]";
                //必填:短信签名-支持不同的号码发送不同的短信签名
                request.SignNameJson = "[\"云通信\",\"云通信\"]";
                //必填:短信模板-可在短信控制台中找到
                request.TemplateCode = "SMS_1000000";
                //必填:模板中的变量替换JSON串,如模板内容为"亲爱的${name},您的验证码为${code}"时,此处的值为
                //友情提示:如果JSON中需要带换行符,请参照标准的JSON协议对换行符的要求,比如短信内容中包含\r\n的情况在JSON中需要表示成\\r\\n,否则会导致JSON在服务端解析失败
                request.TemplateParamJson = "[{\"name\":\"Tom\", \"code\":\"123\"},{\"name\":\"Jack\", \"code\":\"456\"}]";
                //可选-上行短信扩展码(扩展码字段控制在7位或以下，无特殊需求用户请忽略此字段)
                //request.SmsUpExtendCodeJson = "[\"90997\",\"90998\"]";

                //请求失败这里会抛ClientException异常
                response = acsClient.GetAcsResponse(request);

            }
            catch (ServerException e)
            {
                response.Code = e.ErrorCode;
                response.Message = e.ErrorMessage;
            }
            catch (ClientException e)
            {
                response.Code = e.ErrorCode;
                response.Message = e.ErrorMessage;
            }
            return response;

        }

        /// <summary>
        /// 发送短信
        /// </summary>
        /// <param name="telAttr">待发送手机号
        /// </param>
        /// <param name="signAttr">短信签名-可在短信控制台中找到</param>
        /// <param name="temCode">短信模板-可在短信控制台中找到</param>
        /// <param name="temParamAttr">
        /// 可选:模板中的变量替换JSON串,如模板内容为"亲爱的${name},您的验证码为${code}"时,此处的值为
        /// {\"name\":\"张三\",\"code\":\"123\"}
        /// </param>
        /// <returns></returns>
        public static SendBatchSmsResponse Send_Batch_Sms(List<string> telAttr, List<string> signAttr, string temCode, List<string> temParamAttr)
        {
            IClientProfile profile = DefaultProfile.GetProfile("cn-hangzhou", accessKeyId, accessKeySecret);
            DefaultProfile.AddEndpoint("cn-hangzhou", "cn-hangzhou", product, domain);

            IAcsClient acsClient = new DefaultAcsClient(profile);
            SendBatchSmsRequest request = new SendBatchSmsRequest();
            //request.Protocol = ProtocolType.HTTPS;
            //request.TimeoutInMilliSeconds = 1;

            SendBatchSmsResponse response = null;
            try
            {
                //必填:待发送手机号。支持JSON格式的批量调用，批量上限为100个手机号码,批量调用相对于单条调用及时性稍有延迟,验证码类型的短信推荐使用单条调用的方式
                request.PhoneNumberJson = JsonConvert.SerializeObject(telAttr);
                //必填:短信签名-支持不同的号码发送不同的短信签名
                request.SignNameJson = JsonConvert.SerializeObject(signAttr);
                //必填:短信模板-可在短信控制台中找到
                request.TemplateCode = temCode;
                //必填:模板中的变量替换JSON串,如模板内容为"亲爱的${name},您的验证码为${code}"时,此处的值为
                //友情提示:如果JSON中需要带换行符,请参照标准的JSON协议对换行符的要求,比如短信内容中包含\r\n的情况在JSON中需要表示成\\r\\n,否则会导致JSON在服务端解析失败

                request.TemplateParamJson = $"[{string.Join(",", temParamAttr)}]";
                //可选-上行短信扩展码(扩展码字段控制在7位或以下，无特殊需求用户请忽略此字段)
                //request.SmsUpExtendCodeJson = "[\"90997\",\"90998\"]";

                //请求失败这里会抛ClientException异常
                response = acsClient.GetAcsResponse(request);

            }
            catch (ServerException e)
            {
                response.Code = e.ErrorCode;
                response.Message = e.ErrorMessage;
            }
            catch (ClientException e)
            {
                response.Code = e.ErrorCode;
                response.Message = e.ErrorMessage;
            }
            return response;

        }

        #endregion

    }
}
