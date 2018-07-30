using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Jack.Pay.Impls.Weixin
{
    /// <summary>
    /// 企业付款
    /// </summary>
    [PayInterface( PayInterfaceType.WeiXinTransfers)]
    class WeixinTransfers : IPay
    {
        
        const string QueryUrl = "https://api.mch.weixin.qq.com/mmpaymkttransfers/gettransferinfo";
        const string ServerUrl = "https://api.mch.weixin.qq.com/mmpaymkttransfers/promotion/transfers";
        public bool CheckPayState(PayParameter parameter)
        {
            var config = new Config(PayFactory.GetInterfaceXmlConfig(PayInterfaceType.WeiXinScanQRCode, parameter.TradeID));
            SortedDictionary<string, string> postDict = new SortedDictionary<string, string>();
            postDict["appid"] = config.AppID;
            postDict["mch_id"] = config.MchID;
            postDict["partner_trade_no"] = parameter.TradeID;
            postDict["nonce_str"] = Guid.NewGuid().ToString().Replace("-", "");//随机字符串
            postDict["sign"] = Helper.GetMd5Hash(postDict, config.Key);

            var xml = WeiXinScanQRCode.ToXml(postDict);

            var result = Helper.PostXml(QueryUrl, xml, parameter.RequestTimeout, config.SSLCERT_PATH, config.SSLCERT_PASSWORD);
            

            XDocument xmldoc = XDocument.Parse(result);
            //这里不用验证sign
            
            var return_code = xmldoc.Root.XPathSelectElement("return_code").Value;
            if (return_code == "SUCCESS")
            {
                if (xmldoc.Root.XPathSelectElement("err_code_des") != null)
                    throw new PayServerReportException(xmldoc.Root.XPathSelectElement("err_code_des").Value);

                var result_code = xmldoc.Root.XPathSelectElement("result_code").Value;
                if (result_code == "SUCCESS")
                {
                    PayFactory.OnPaySuccessed(parameter.TradeID,null, null, result);
                    return true;
                }
            }
            return false;
        }
        

        public RefundResult Refund(RefundParameter parameter)
        {
            throw new NotImplementedException();
        }


        public string BeginPay(PayParameter parameter)
        {
            if(string.IsNullOrEmpty(parameter.Description))
            {
                throw new Exception("PayParameter.Description can not be empty");
            }
            var config = new Config(PayFactory.GetInterfaceXmlConfig(PayInterfaceType.WeiXinTransfers, parameter.TradeID));
            SortedDictionary<string, string> postDict = new SortedDictionary<string, string>();
            postDict["mch_appid"] = config.AppID;
            postDict["mchid"] = config.MchID;
            postDict["partner_trade_no"] = parameter.TradeID;
            postDict["nonce_str"] = Guid.NewGuid().ToString().Replace("-", "");//随机字符串
            postDict["openid"] = parameter.AuthCode;
            postDict["check_name"] = "NO_CHECK";
            postDict["amount"] = ((int)(parameter.Amount * 100)).ToString();//单位：分
            postDict["desc"] = parameter.Description;
            postDict["spbill_create_ip"] = "8.8.8.8";
            postDict["sign"] = Helper.GetMd5Hash(postDict, config.Key);

            var xml = WeiXinScanQRCode.ToXml(postDict);
            
            var result = Helper.PostXml(ServerUrl, xml, parameter.RequestTimeout, config.SSLCERT_PATH, config.SSLCERT_PASSWORD);
            PayFactory.OnLog(parameter.TradeID, LogEventType.ReceivePayResult, result);

            XDocument xmldoc = XDocument.Parse(result);
            //这里不用验证sign

            if (xmldoc.Root.XPathSelectElement("return_msg").Value != "OK" && xmldoc.Root.XPathSelectElement("err_code_des") != null)
            {
                throw new PayServerReportException(xmldoc.Root.XPathSelectElement("err_code_des").Value);
            }

            var return_code = xmldoc.Root.XPathSelectElement("return_code").Value;
            if (return_code == "SUCCESS" && xmldoc.Root.XPathSelectElement("result_code").Value == "SUCCESS")
            {
                PayFactory.OnPaySuccessed(parameter.TradeID,null, null, result);
            }
            return null;
        }
    }
}
