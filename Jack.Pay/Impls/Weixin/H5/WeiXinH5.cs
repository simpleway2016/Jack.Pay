
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
using Microsoft.AspNetCore.Http;
#endif

namespace Jack.Pay.Impls.Weixin
{
    [PayInterface(PayInterfaceType.WeiXinH5)]
    class WeiXinH5 : WeiXinScanQRCode
    {
        protected const string ServerUrl = "https://api.mch.weixin.qq.com/pay/unifiedorder";

        public override string BeginPay(PayParameter parameter)
        {
            if(string.IsNullOrEmpty(parameter.AuthCode))
            {
                throw new Exception("请把PayParameter.AuthCode设置为客户端ip");
            }
            var config = new Config( PayFactory.GetInterfaceXmlConfig(PayInterfaceType.WeiXinH5, parameter.TradeID));
            SortedDictionary<string, string> postDict = new SortedDictionary<string, string>();
            postDict["appid"] = config.AppID;
            postDict["mch_id"] = config.MchID;
            postDict["nonce_str"] = Guid.NewGuid().ToString().Replace("-", "");//随机字符串
            postDict["body"] = parameter.Description;//交易描述
            postDict["out_trade_no"] = parameter.TradeID;
            postDict["total_fee"] = ((int)(parameter.Amount * 100)).ToString();//单位：分
            postDict["spbill_create_ip"] = parameter.AuthCode;//终端ip
            if (string.IsNullOrEmpty(parameter.NotifyDomain))
            {
                postDict["notify_url"] = "http://paysdk.weixin.qq.com/example/ResultNotifyPage.aspx";
            }
            else
            {
                postDict["notify_url"] = $"{parameter.NotifyDomain}/{WeiXinNotify_RequestHandler.NotifyPageName}";
            }

            postDict["trade_type"] = "MWEB";
            postDict["sign_type"] = "MD5";
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
            postDict["scene_info"] = Newtonsoft.Json.JsonConvert.SerializeObject(new { h5_info = new { type="Wap", wap_url= $"{PayFactory.CurrentDomainUrl}/", wap_name="H5Pay" } });
            postDict["sign"] = Helper.GetMd5Hash(postDict, config.Key);

            var xml = ToXml(postDict);

            var result = Helper.PostXml(ServerUrl, xml, parameter.RequestTimeout);
            PayFactory.OnLog(parameter.TradeID, LogEventType.ReceivePayResult, result);

            XDocument xmldoc = XDocument.Parse(result);

            CheckSign(xmldoc, config);

            var return_code = xmldoc.Root.XPathSelectElement("return_code").Value;
            var return_msg = xmldoc.Root.XPathSelectElement("return_msg").Value;
            if (return_code == "FAIL")
                throw new Exception(return_msg);
            else if(return_code == "SUCCESS" && return_msg == "OK")
            {
                return xmldoc.Root.XPathSelectElement("mweb_url").Value;
            }
            return null;
        }

    }
}
