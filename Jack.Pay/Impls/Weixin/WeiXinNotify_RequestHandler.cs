using System;
using System.Collections.Generic;
using System.Text;

using System.Xml.Linq;
using System.Xml.XPath;
using System.Security.Cryptography;
using Way.Lib;
using Jack.HttpRequestHandlers;
#if NET46
#else
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Internal;
#endif


namespace Jack.Pay.Impls.Weixin
{
    /// <summary>
    /// 处理微信通知信息
    /// </summary>
    class WeiXinNotify_RequestHandler:IRequestHandler
    {
        public const string NotifyPageName = "JACK_PAY_WeiXinNotify_HttpHandler.aspx";

        public string UrlPageName => NotifyPageName;

       
        public TaskStatus Handle(IHttpProxy httpHandler)
        {          
            try
            {
                var xml = httpHandler.ReadRequestBody();
                using (CLog log = new CLog("WeiXinNotify Notify",false))
                {
                    log.Log("xml:{0}", xml);

                    XDocument xmldoc = XDocument.Parse(xml);
                    SortedDictionary<string, string> xmlDict = new SortedDictionary<string, string>();
                    var nodes = xmldoc.Root.Elements();
                    foreach (var element in nodes)
                    {
                        if (element.Name.LocalName != "sign")
                        {
                            xmlDict[element.Name.LocalName] = element.Value;
                        }
                    }

                    var return_code = xmlDict["return_code"];
                    var result_code = xmlDict["result_code"];
                    var out_trade_no = xmlDict["out_trade_no"];

                    PayFactory.OnLog(out_trade_no, LogEventType.ReceiveNotify, xml);

                    var config = new Config(PayFactory.GetInterfaceXmlConfig(PayInterfaceType.WeiXinScanQRCode, out_trade_no));

                    log.Log("签名校验");
                    var sign = xmldoc.Root.XPathSelectElement("sign").Value;

                    var computeSign = Helper.GetMd5Hash(xmlDict, config.Key);

                    if (sign != computeSign)
                    {
                        log.Log("正确签名：{0}", computeSign);
                        log.Log("签名校验不通过");
                        throw new Exception("签名校验不通过");
                    }

                    if (result_code == "SUCCESS" && return_code == "SUCCESS")
                    {
                        log.Log("excute OnPaySuccessed");
                        PayFactory.OnPaySuccessed(out_trade_no, null, null, xml);

                        WxPayData data = new WxPayData();
                        data.SetValue("return_code", "SUCCESS");
                        data.SetValue("return_msg", "OK");
                        data.SetValue("appid", config.AppID);
                        data.SetValue("mch_id", config.MchID);
                        data.SetValue("result_code", "SUCCESS");
                        data.SetValue("err_code_des", "OK");
                        data.SetValue("sign", data.MakeSign(config));

                        var writebackXml = data.ToXml();
                        log.Log("write to weixin:{0}", writebackXml);

                        httpHandler.ResponseWrite(writebackXml);
                    }
                }
            }
            catch (Exception ex)
            {
                using (CLog log = new CLog("WeiXin Notify error "))
                {
                    log.Log(ex.ToString());

                    WxPayData res = new WxPayData();
                    res.SetValue("return_code", "FAIL");
                    res.SetValue("return_msg", ex.Message);

                    var writebackXml = res.ToXml();
                    log.Log("write to weixin:{0}", writebackXml);

                    httpHandler.ResponseWrite( writebackXml);
                }
            }
            return TaskStatus.Completed;
        }
    }

}
