using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Jack.Pay.Impls.IPaysoon
{
    [PayInterface(PayInterfaceType.IPaysoonWeiXinJSApi)]
    class Paysoon_WeixinJSApi : Paysoon_WeiXinBarcode
    {
        protected override string ServerUrl => "http://gateway.ipaysoon.com/MSZFPlatform/wxpays/webpay";
        protected override string BusinessCode => "WXPAYWEBT1";
        protected override bool SupportNotify => true;

        public override string BeginPay(PayParameter parameter)
        {
            if (string.IsNullOrEmpty(parameter.ReturnUrl))
                throw new Exception("ReturnUrl can not be empty");

            var attrs = this.GetType().GetCustomAttributes(typeof(PayInterfaceAttribute), false);
            //获取当前接口类型
            var myInterfaceType = ((PayInterfaceAttribute)attrs[0]).InterfaceType;
            var config = new Config(PayFactory.GetInterfaceXmlConfig(myInterfaceType, parameter.TradeID));

            Dictionary<string, object> postDict = new Dictionary<string, object>();
            postDict["merchantCode"] = config.merchantCode;
            postDict["operatorCode"] = config.operatorCode;
            postDict["businessCode"] = this.BusinessCode;


            var detail = new Dictionary<string, string>
            {
                {"amount", ((int)(parameter.Amount*100)).ToString()},
                {"businessCode",this.BusinessCode},
                {"merchantGenCode",parameter.TradeID},
                {"operatorCode",config.operatorCode},
                {"purpose" , parameter.Description},
                {"openId" , parameter.AuthCode},
            };

            if (!string.IsNullOrEmpty(parameter.ReturnUrl))
            {
                PaysoonReturn_RequestHandler.ReturnUrlDict[parameter.TradeID] = parameter.ReturnUrl;
                detail["frontCallback"] = $"{parameter.NotifyDomain}/{PaysoonReturn_RequestHandler.ReturnPageName}";
            }
            if (SupportNotify)
            {
                if (string.IsNullOrEmpty(parameter.NotifyDomain) == false)
                {
                    detail["callBackUrl"] = $"{parameter.NotifyDomain}/{Paysoon_RequestHandler.NotifyPageName}";
                }
                else
                {
                    throw new Exception("This interface just run at website");
                }
            }

            var signContent = Newtonsoft.Json.JsonConvert.SerializeObject(new object[] { detail });

            var rsa = Way.Lib.RSA.CreateRsaFromPublicKey(config.platformPublicKey);
            rsa.KeySize = 1024;
            // 把密文加入提交的参数列表中

            postDict["detail"] = Way.Lib.RSA.EncryptToBase64(rsa, Encoding.UTF8.GetBytes(signContent), RSAEncryptionPadding.Pkcs1);


            string json = Newtonsoft.Json.JsonConvert.SerializeObject(postDict);
            string queryStr = $"request={WebUtility.UrlEncode(json)}";
            var result = Helper.PostQueryString(ServerUrl, queryStr, parameter.RequestTimeout);
            PayFactory.OnLog(parameter.TradeID, LogEventType.ReceivePayResult, result);

            var resultDict = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(result);
            return OnBeginPayPostGetResult(result,config, resultDict, parameter);
        }

        protected override string OnBeginPayPostGetResult(string result, Config config, JObject resultDict, PayParameter parameter)
        {
            if (resultDict["resultCode"].ToString() == "0000")
            {
                var rsa = Way.Lib.RSA.CreateRsaFromPrivateKey(config.merchantPrivateKey , Way.Lib.RSAKeyType.PKCS8);
                rsa.KeySize = 1024;
                var detailJson = System.Text.Encoding.UTF8.GetString(Way.Lib.RSA.DecryptFromBase64(rsa, resultDict["detail"].ToString(), RSAEncryptionPadding.Pkcs1));

                var resultDetail = (Newtonsoft.Json.Linq.JArray)Newtonsoft.Json.JsonConvert.DeserializeObject(detailJson);
                var weixinCode = resultDetail[0]["url"].ToString();

                var returnDict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(weixinCode);
                returnDict["ReturnUrl"] = parameter.ReturnUrl;
                returnDict["TradeID"] = parameter.TradeID;
                var jsonStr = Newtonsoft.Json.JsonConvert.SerializeObject(returnDict);

                //先把jsonStr保存成一个临时文件
                string tranid = Guid.NewGuid().ToString("N");
                string tempFile = $"{Helper.GetSaveFilePath()}\\{tranid}.txt";
                System.IO.File.WriteAllText(tempFile, jsonStr, Encoding.UTF8);

                return $"{parameter.NotifyDomain}/{Weixin.WeiXinPayRedirect_RequestHandler.NotifyPageName}?tranId={tranid}";
            }
            else
            {
                throw new PayServerReportException(resultDict["resultMsg"].ToString());
            }
        }
    }
}
