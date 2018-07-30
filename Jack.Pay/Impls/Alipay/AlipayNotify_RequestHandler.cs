using System;
using System.Collections.Generic;
using System.Text;

#if NET46
#else
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Internal;
#endif
using System.Xml.Linq;
using System.Xml.XPath;
using System.Security.Cryptography;
using System.IO;
using Way.Lib;
using Jack.HttpRequestHandlers;

namespace Jack.Pay.Impls.Alipay
{
    /// <summary>
    /// 处理支付宝通知信息
    /// </summary>
    class AlipayNotify_RequestHandler:IRequestHandler
    {
        public const string NotifyPageName = "JACK_PAY_AlipayNotify_HttpHandler.aspx";

        public string UrlPageName => NotifyPageName;

        public static SortedDictionary<string, string> GetRequestData(KeyPair form)
        {
            SortedDictionary<string, string> result = new SortedDictionary<string, string>();
            foreach (var item in form)
            {
                if (item.Key == "sign" || item.Key == "sign_type" || string.IsNullOrEmpty(item.Value))
                    continue;

                result[item.Key] = item.Value;
            }
            return result;
        }

        public TaskStatus Handle(IHttpProxy httpProxy)
        {
            using (CLog log = new CLog("Alipay Notify",false))
            {
                var data = GetRequestData(httpProxy.Form);

                var dataJson = Newtonsoft.Json.JsonConvert.SerializeObject(data);
                log.Log(dataJson);

                if (data.Count > 0)
                {

                    string out_trade_no = data["out_trade_no"];
                    string sign = httpProxy.Form["sign"];
                    //string sign_type = form["sign_type"];


                    PayFactory.OnLog(out_trade_no, LogEventType.ReceiveNotify, dataJson);

                    var config = new Config(PayFactory.GetInterfaceXmlConfig(PayInterfaceType.AlipayScanQRCode, out_trade_no));

                    var signStr = Helper.GetUrlString(data, false);

                    System.Security.Cryptography.RSA rsacore = Way.Lib.RSA.CreateRsaFromPublicKey(config.alipayPublicKey);

                    var isPass = rsacore.VerifyData(Encoding.GetEncoding("utf-8").GetBytes(signStr), Convert.FromBase64String(sign), HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);


                    if (isPass == false)
                    {
                        log.Log("sign:{0}", sign);
                        log.Log("签名不一致");
                        httpProxy.ResponseWrite( "fail");
                        return TaskStatus.Completed;
                    }

                    //支付宝交易号
                    string trade_no = data["trade_no"];

                    //交易状态
                    //在支付宝的业务通知中，只有交易通知状态为TRADE_SUCCESS或TRADE_FINISHED时，才是买家付款成功。
                    string trade_status = data["trade_status"];

                    double? receipt_amount = null;
                    try
                    {
                        receipt_amount = Convert.ToDouble(data["receipt_amount"]);
                    }
                    catch
                    {

                    }
                    log.Log(trade_status);

                    if (trade_status == "TRADE_SUCCESS")
                    {
                        log.Log("excute OnPaySuccessed");
                        PayFactory.OnPaySuccessed(out_trade_no, receipt_amount, null, dataJson);

                    }
                    
                    httpProxy.ResponseWrite("success");
                    
                }
                else
                {
                    httpProxy.ResponseWrite("无通知参数");
                }
            }

            return  TaskStatus.Completed;
        }


    }

}
