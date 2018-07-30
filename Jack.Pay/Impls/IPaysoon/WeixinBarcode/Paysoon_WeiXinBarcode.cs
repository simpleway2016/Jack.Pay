using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Jack.Pay.Impls.IPaysoon
{
    [PayInterface(PayInterfaceType.IPaysoonWeiXinBarcode)]
    class Paysoon_WeiXinBarcode : BasePay
    {
        /// <summary>
        /// 下单地址
        /// </summary>
        protected virtual string ServerUrl => "http://gateway.ipaysoon.com/MSZFPlatform/wxpays/micropay";
        protected virtual string BusinessCode => "WXPAYF2FT1";
        /// <summary>
        /// 查询地址
        /// </summary>
        protected virtual string QueryUrl => "http://gateway.ipaysoon.com/MSZFPlatform/wxpay/query";
        /// <summary>
        /// 是否支持页面回调结果
        /// </summary>
        protected virtual bool SupportNotify => false;

        public override string BeginPay(PayParameter parameter)
        {
            var attrs = this.GetType().GetCustomAttributes(typeof(PayInterfaceAttribute), false);
            //获取当前接口类型
            var myInterfaceType = ((PayInterfaceAttribute)attrs[0]).InterfaceType;
            var config = new Config(PayFactory.GetInterfaceXmlConfig(myInterfaceType, parameter.TradeID));

            Dictionary<string, object> postDict = new Dictionary<string, object>();
            postDict["merchantCode"] = config.merchantCode;
            postDict["operatorCode"] = config.operatorCode;
            postDict["businessCode"] = this.BusinessCode;
         

            var detail = new SortedDictionary<string, object>
            {
                {"amount", ((int)(parameter.Amount*100)).ToString()},
                {"businessCode",this.BusinessCode},
                {"merchantGenCode",parameter.TradeID},
                 {"operatorCode",config.operatorCode},
                {"purpose" , parameter.Description},
            };
            if(!string.IsNullOrEmpty(parameter.AuthCode))
            {
                detail["authCode"] = parameter.AuthCode;
            }
            if (parameter.GoodsDetails.Count > 0)
            {
                var goodsDetails = new List<object>();
                foreach (var gooditem in parameter.GoodsDetails)
                {
                    goodsDetails.Add(new
                    {
                        productCode = gooditem.GoodsId,
                        productName = gooditem.GoodsName,
                        productNumber = gooditem.Quantity.ToString(),
                        productPrice = ((int)(gooditem.Price * 100))
                    });
                }
                detail["requestProductList"] = goodsDetails;
            }

            if (SupportNotify)
            {
                if (string.IsNullOrEmpty(parameter.NotifyDomain) == false)
                {
                    detail["callBackUrl"] = $"{parameter.NotifyDomain}/{Paysoon_RequestHandler.NotifyPageName}";
                }
            }

            var signContent = Newtonsoft.Json.JsonConvert.SerializeObject(new object[] { detail });

            var rsa = Way.Lib.RSA.CreateRsaFromPublicKey(config.platformPublicKey);
            rsa.KeySize = 1024;
            // 把密文加入提交的参数列表中

            postDict["detail"] = Way.Lib.RSA.EncryptToBase64(rsa , Encoding.UTF8.GetBytes(signContent) , RSAEncryptionPadding.Pkcs1);

         
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(postDict);
            string queryStr = $"request={WebUtility.UrlEncode(json)}";
            var result = Helper.PostQueryString(ServerUrl, queryStr, parameter.RequestTimeout);
            PayFactory.OnLog(parameter.TradeID, LogEventType.ReceivePayResult, result);
            
            var resultDict = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(result);
            return OnBeginPayPostGetResult(result,config, resultDict , parameter);
        }
        /// <summary>
        /// 处理服务器返回的结果
        /// </summary>
        /// <param name="result">服务器返回的内容</param>
        /// <param name="resultDict"></param>
        /// <param name="parameter">支付参数</param>
        /// <returns></returns>
        protected virtual string OnBeginPayPostGetResult(string result ,Config config, Newtonsoft.Json.Linq.JObject resultDict,PayParameter parameter)
        {
            if (resultDict["resultCode"].ToString() == "0000")
            {
                var rsa = Way.Lib.RSA.CreateRsaFromPrivateKey(config.merchantPrivateKey , Way.Lib.RSAKeyType.PKCS8);
                rsa.KeySize = 1024;
                var detailJson = System.Text.Encoding.UTF8.GetString( Way.Lib.RSA.DecryptFromBase64(rsa, resultDict["detail"].ToString(), RSAEncryptionPadding.Pkcs1));

                var resultDetail = (Newtonsoft.Json.Linq.JArray)Newtonsoft.Json.JsonConvert.DeserializeObject(detailJson);
                if (resultDetail[0]["statusId"].ToString() == "14")
                {
                    //var charge = Convert.ToDouble(resultDetail[0]["charge"].ToString()) / 100.0;
                    var amount = Convert.ToDouble(resultDetail[0]["amount"].ToString()) / 100.0;
                    var receiptAmount = Convert.ToDouble(resultDetail[0]["receiptAmount"].ToString()) / 100.0;
                    PayFactory.OnPaySuccessed(parameter.TradeID, receiptAmount, null, result);
                }
            }
            else
            {
                throw new PayServerReportException(resultDict["resultMsg"].ToString());
            }
            return null;
        }

        public override bool CheckPayState(PayParameter parameter)
        {
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
                {"merchantGenCode",parameter.TradeID},
                 {"operatorCode",config.operatorCode},
            };

            postDict["detail"] = new object[] { detail };


            string json = Newtonsoft.Json.JsonConvert.SerializeObject(postDict);
            string queryStr = $"request={WebUtility.UrlEncode(json)}";
            var result = Helper.PostQueryString(QueryUrl, queryStr, parameter.RequestTimeout);

            var resultDict = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(result);
            if (resultDict["resultCode"].ToString() == "0000")
            {
                var resultDetail = (Newtonsoft.Json.Linq.JArray)resultDict["detail"];
                if (resultDetail[0]["statusId"].ToString() == "14")
                {
                    var charge = Convert.ToDouble(resultDetail[0]["charge"].ToString()) / 100.0;
                    var amount = Convert.ToDouble(resultDetail[0]["charge"].ToString()) / 100.0;
                    PayFactory.OnPaySuccessed(parameter.TradeID, amount - charge, null, result);
                    return true;
                }
                else if (resultDetail[0]["statusId"].ToString() == "3")
                {
                    PayFactory.OnPayFailed(parameter.TradeID, "订单已打回", result);
                    return true;
                }
                else if (resultDetail[0]["statusId"].ToString() == "12")
                {
                    PayFactory.OnPayFailed(parameter.TradeID, "通道提交失败", result);
                    return true;
                }
                else if (resultDetail[0]["statusId"].ToString() == "15")
                {
                    PayFactory.OnPayFailed(parameter.TradeID, "处理失败", result);
                    return true;
                }
            }
            return false;
        }

        public override RefundResult Refund(RefundParameter parameter)
        {
            throw new NotImplementedException();
        }
    }
}
