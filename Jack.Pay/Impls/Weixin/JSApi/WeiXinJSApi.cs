using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
#if NET46
#else
using System.Net.Http;
#endif

namespace Jack.Pay.Impls.Weixin
{
    [PayInterface(PayInterfaceType.WeiXinJSApi)]
    class WeiXinJSApi : WeiXinScanQRCode
    {
        public override string BeginPay(PayParameter parameter)
        {
            if (string.IsNullOrEmpty(parameter.AuthCode))
                throw new Exception("PayParameter.AuthCode（wechat openid）can not be empty");
            if (string.IsNullOrEmpty(parameter.ReturnUrl))
                throw new Exception("ReturnUrl can not be empty");

            bool enableNotify = false;

            var config = new Config( PayFactory.GetInterfaceXmlConfig(PayInterfaceType.WeiXinJSApi, parameter.TradeID));
            SortedDictionary<string, string> postDict = new SortedDictionary<string, string>();
            postDict["appid"] = config.AppID;
            postDict["mch_id"] = config.MchID;
            postDict["nonce_str"] = Guid.NewGuid().ToString().Replace("-", "");//随机字符串
            postDict["sign_type"] = "MD5";
            postDict["body"] = parameter.Description;//商品描述
            postDict["out_trade_no"] = parameter.TradeID;
            postDict["openid"] = parameter.AuthCode;
            postDict["total_fee"] = ((int)(parameter.Amount * 100)).ToString();//单位：分
            postDict["spbill_create_ip"] = "8.8.8.8";//终端ip
            if (string.IsNullOrEmpty(parameter.NotifyDomain))
            {

                postDict["notify_url"] = "http://paysdk.weixin.qq.com/example/ResultNotifyPage.aspx";
            }
            else
            {
                enableNotify = true; 
                postDict["notify_url"] = $"{parameter.NotifyDomain}/{WeiXinNotify_RequestHandler.NotifyPageName}";
            }

            postDict["time_start"] = DateTime.Now.ToString("yyyyMMddHHmmss");//交易起始时间
            if (parameter.ExpireTime != null)
            {
                postDict["time_expire"] = parameter.ExpireTime.Value.ToString("yyyyMMddHHmmss");//交易结束时间
            }
            else
            {
                //默认十分钟
                postDict["time_expire"] = DateTime.Now.AddMinutes(10).ToString("yyyyMMddHHmmss");//交易结束时间
            }

            postDict["trade_type"] = "JSAPI";
           
            postDict["sign"] = Helper.GetMd5Hash(postDict, config.Key);

            var xml = ToXml(postDict);

            var result = Helper.PostXml(ServerUrl, xml, parameter.RequestTimeout);
            PayFactory.OnLog(parameter.TradeID, LogEventType.ReceivePayResult, result);

            XDocument xmldoc = XDocument.Parse(result);        

            var return_code = xmldoc.Root.XPathSelectElement("return_code").Value;
            var return_msg = xmldoc.Root.XPathSelectElement("return_msg").Value;
            if (return_code == "FAIL")
                throw new PayServerReportException(return_msg);
            else if(return_code == "SUCCESS" && return_msg == "OK")
            {
                WeiXinScanQRCode.CheckSign(xmldoc, config);

                if (enableNotify == false)
                {
                    new Thread(()=> {
                        CheckPayStateInLoop(parameter);
                    }).Start();
                }

                var prepayid = xmldoc.Root.XPathSelectElement("prepay_id").Value;
                SortedDictionary<string, string> returnDict = new SortedDictionary<string, string>();
                returnDict["appId"] = config.AppID;
                returnDict["timeStamp"] = GenerateTimeStamp();
                returnDict["nonceStr"] = Guid.NewGuid().ToString("N");
                returnDict["package"] = $"prepay_id={prepayid}";
                returnDict["signType"] = "MD5";
                returnDict["paySign"] = Helper.GetMd5Hash(returnDict, config.Key);

                returnDict["ReturnUrl"] = parameter.ReturnUrl;
                returnDict["TradeID"] = parameter.TradeID;
                var jsonStr = Newtonsoft.Json.JsonConvert.SerializeObject(returnDict);


                //先把jsonStr保存成一个临时文件
                string tranid = Guid.NewGuid().ToString("N");
                string tempFile = $"{Helper.GetSaveFilePath()}\\{tranid}.txt";
                System.IO.File.WriteAllText(tempFile, jsonStr, Encoding.UTF8);

                return $"{parameter.NotifyDomain}/{WeiXinPayRedirect_RequestHandler.NotifyPageName}?tranId={tranid}";
            }
            return null;
        }

        static string GenerateTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString();
        }

    }
}
