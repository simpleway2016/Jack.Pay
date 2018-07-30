
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
    [PayInterface(PayInterfaceType.WeiXinScanQRCode)]
    class WeiXinScanQRCode : BasePay
    {
        protected const string RefundUrl = "https://api.mch.weixin.qq.com/secapi/pay/refund";
        protected const string QueryUrl = "https://api.mch.weixin.qq.com/pay/orderquery";
        protected const string ServerUrl = "https://api.mch.weixin.qq.com/pay/unifiedorder";
        public override bool CheckPayState(PayParameter parameter)
        {
            var config = new Config(PayFactory.GetInterfaceXmlConfig(PayInterfaceType.WeiXinScanQRCode, parameter.TradeID));
            SortedDictionary<string, string> postDict = new SortedDictionary<string, string>();
            postDict["appid"] = config.AppID;
            postDict["mch_id"] = config.MchID;
            postDict["out_trade_no"] = parameter.TradeID;
            postDict["nonce_str"] = Guid.NewGuid().ToString().Replace("-", "");//随机字符串
            postDict["sign_type"] = "MD5";
            postDict["sign"] = Helper.GetMd5Hash(postDict, config.Key);

            var xml = ToXml(postDict);

            var result = Helper.PostXml(QueryUrl, xml, parameter.RequestTimeout);
            XDocument xmldoc = XDocument.Parse(result);
            CheckSign(xmldoc, config);

            var return_code = xmldoc.Root.XPathSelectElement("return_code").Value;
            var return_msg = xmldoc.Root.XPathSelectElement("return_msg").Value;
            if (return_code == "SUCCESS" && return_msg == "OK")
            {
                if (xmldoc.Root.XPathSelectElement("err_code_des") != null)
                    throw new PayServerReportException(xmldoc.Root.XPathSelectElement("err_code_des").Value);

                var trade_state = xmldoc.Root.XPathSelectElement("trade_state").Value;
                if(trade_state == "SUCCESS")
                {
                    PayFactory.OnPaySuccessed(parameter.TradeID,null, null, result);
                    return true;
                }
                else
                {
                    if(trade_state == "PAYERROR" || trade_state == "REVOKED")
                    {
                        var trade_state_desc = xmldoc.Root.XPathSelectElement("trade_state_desc").Value;
                        PayFactory.OnPayFailed(parameter.TradeID, trade_state_desc , result);
                        return true;
                    }
                }
            }
            return false;
        }

        public override RefundResult Refund(RefundParameter parameter)
        {
            var config = new Config(PayFactory.GetInterfaceXmlConfig(PayInterfaceType.WeiXinScanQRCode, parameter.TradeID));
            SortedDictionary<string, string> postDict = new SortedDictionary<string, string>();
            postDict["appid"] = config.AppID;
            postDict["mch_id"] = config.MchID;
            postDict["nonce_str"] = Guid.NewGuid().ToString().Replace("-", "");//随机字符串
            postDict["out_trade_no"] = parameter.TradeID;
            postDict["out_refund_no"] = Guid.NewGuid().ToString("N");//商户系统内部的退款单号，商户系统内部唯一，只能是数字、大小写字母_-|*@ ，同一退款单号多次请求只退一笔。

            postDict["total_fee"] = ((int)(parameter.TotalAmount * 100)).ToString();//单位：分
            postDict["refund_fee"] = ((int)(parameter.Amount * 100)).ToString();//单位：分
            postDict["sign_type"] = "MD5";
            postDict["sign"] = Helper.GetMd5Hash(postDict, config.Key);

            var xml = ToXml(postDict);

            var result = Helper.PostXml(RefundUrl, xml, 15 , config.SSLCERT_PATH , config.SSLCERT_PASSWORD);

            XDocument xmldoc = XDocument.Parse(result);
            CheckSign(xmldoc, config);

            var return_code = xmldoc.Root.XPathSelectElement("return_code").Value;
            var return_msg = xmldoc.Root.XPathSelectElement("return_msg").Value;
            if (return_code == "SUCCESS" && return_msg == "OK")
            {
                return new RefundResult()
                {
                    Result = RefundResult.ResultEnum.SUCCESS,
                    ServerMessage = result,
                };
            }
            return new RefundResult()
            {
                Result = RefundResult.ResultEnum.FAIL,
                ServerMessage = result,
            };
        }

        internal static void CheckSign(XDocument xmldoc, Config config)
        {
            if (xmldoc.Root.XPathSelectElement("return_msg").Value != "OK" && xmldoc.Root.XPathSelectElement("err_code_des") != null)
                throw new PayServerReportException(xmldoc.Root.XPathSelectElement("err_code_des").Value);

            if (xmldoc.Root.XPathSelectElement("sign") == null)
            {
                throw new PayServerReportException(xmldoc.Root.XPathSelectElement("return_msg").Value);
            }

            SortedDictionary<string, string> xmlDict = new SortedDictionary<string, string>();
            var nodes = xmldoc.Root.Elements();
            foreach (var element in nodes)
            {
                if (element.Name.LocalName != "sign")
                {
                    xmlDict[element.Name.LocalName] = element.Value;
                }
            }
            //签名校验
            var computeSign = Helper.GetMd5Hash(xmlDict, config.Key);

            if (xmldoc.Root.XPathSelectElement("sign").Value != computeSign)
            {
                throw new Exception("服务器返回的结果不能通过签名验证");
            }
        }

        public override string BeginPay(PayParameter parameter)
        {
            bool enableNotify = false;

            var config = new Config( PayFactory.GetInterfaceXmlConfig(PayInterfaceType.WeiXinScanQRCode, parameter.TradeID));
            SortedDictionary<string, string> postDict = new SortedDictionary<string, string>();
            postDict["appid"] = config.AppID;
            postDict["mch_id"] = config.MchID;
            postDict["nonce_str"] = Guid.NewGuid().ToString().Replace("-", "");//随机字符串
            postDict["body"] = parameter.Description;//交易描述
            postDict["out_trade_no"] = parameter.TradeID;
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

            postDict["trade_type"] = "NATIVE";
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
                if(enableNotify == false)
                {
                    new Thread(()=> {
                        CheckPayStateInLoop(parameter);
                    }).Start();
                }
                return xmldoc.Root.XPathSelectElement("code_url").Value;
            }
            return null;
        }

      
        public static string ToXml(SortedDictionary<string, string> parameters)
        {
            //数据为空时不能转化为xml格式
            if (0 == parameters.Count)
            {

                throw new Exception("WxPayData数据为空!");
            }

            string xml = "<xml>";
            foreach (KeyValuePair<string, string> pair in parameters)
            {
                //字段值不能为null，会影响后续流程
                if (pair.Value == null)
                {
                    throw new Exception("WxPayData内部含有值为null的字段!");
                }

                if (pair.Value.GetType() == typeof(int))
                {
                    xml += "<" + pair.Key + ">" + pair.Value + "</" + pair.Key + ">";
                }
                else if (pair.Value.GetType() == typeof(string))
                {
                    xml += "<" + pair.Key + ">" + "<![CDATA[" + pair.Value + "]]></" + pair.Key + ">";
                }
                else//除了string和int类型不能含有其他数据类型
                {
                    throw new Exception("WxPayData字段数据类型错误!");
                }
            }
            xml += "</xml>";
            return xml;
        }
    }
}
