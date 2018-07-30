
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
    [PayInterface(PayInterfaceType.WeiXinBarcode)]
    class WeiXinBarcode : WeiXinScanQRCode
    {
        const string QueryUrl = "https://api.mch.weixin.qq.com/pay/orderquery";
        const string ServerUrl = "https://api.mch.weixin.qq.com/pay/micropay";


        public override string BeginPay(PayParameter parameter)
        {
            var config = new Config( PayFactory.GetInterfaceXmlConfig(PayInterfaceType.WeiXinBarcode, parameter.TradeID));
            SortedDictionary<string, string> postDict = new SortedDictionary<string, string>();
            postDict["appid"] = config.AppID;
            postDict["mch_id"] = config.MchID;
            postDict["nonce_str"] = Guid.NewGuid().ToString().Replace("-", "");//随机字符串
            postDict["body"] = parameter.Description;//商品描述
            postDict["out_trade_no"] = parameter.TradeID;
            postDict["total_fee"] = ((int)(parameter.Amount * 100)).ToString();//单位：分
            postDict["spbill_create_ip"] = "8.8.8.8";//终端ip
            postDict["auth_code"] = parameter.AuthCode;            
            postDict["sign_type"] = "MD5";
            postDict["sign"] = Helper.GetMd5Hash(postDict, config.Key);

            var xml = ToXml(postDict);

            var result = Helper.PostXml(ServerUrl, xml, parameter.RequestTimeout);
            PayFactory.OnLog(parameter.TradeID, LogEventType.ReceivePayResult,  result);

            XDocument xmldoc = XDocument.Parse(result);

            WeiXinScanQRCode.CheckSign(xmldoc, config);

            var return_code = xmldoc.Root.XPathSelectElement("return_code").Value;
            var return_msg = xmldoc.Root.XPathSelectElement("return_msg").Value;

            if (return_code == "FAIL")
                throw new Exception(return_msg);
            else if(return_code == "SUCCESS" && return_msg == "OK")
            {
                if(xmldoc.Root.XPathSelectElement("result_code").Value == "SUCCESS")
                {
                    //确定付款成功
                    PayFactory.OnPaySuccessed(parameter.TradeID,null, null, result);
                }
                else if (xmldoc.Root.XPathSelectElement("err_code").Value != "USERPAYING" &&
                    xmldoc.Root.XPathSelectElement("result_code").Value == "FAIL" && xmldoc.Root.XPathSelectElement("err_code_des") != null)
                {
                    throw new PayServerReportException(xmldoc.Root.XPathSelectElement("err_code_des").Value);
                }
                else 
                {
                    new Thread(()=> {
                        CheckPayStateInLoop(parameter);
                    }).Start();
                }
            }
            return null;
        }

       
    }
}
