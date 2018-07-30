using System;
using System.Collections.Generic;
using System.Text;
using Jack.Pay.Impls.Alipay;
using System.Security.Cryptography;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Jack.Pay.Impls
{
    [PayInterface(PayInterfaceType.AlipayScanQRCode)]
    class AlipayScanQRCode : AlipayBarcode
    {
        const string ServerUrl = "https://openapi.alipay.com/gateway.do";

        public override string BeginPay(PayParameter parameter)
        {
            var enableNotify = false;
            var config = new Config(PayFactory.GetInterfaceXmlConfig(PayInterfaceType.AlipayScanQRCode, parameter.TradeID));
            SortedDictionary<string, string> postDict = new SortedDictionary<string, string>();
            postDict["app_id"] = config.appid;
            postDict["method"] = "alipay.trade.precreate";
            postDict["charset"] = "utf-8";
            postDict["sign_type"] = "RSA";
            postDict["timestamp"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            postDict["version"] = "1.0";
            if (string.IsNullOrEmpty(parameter.NotifyDomain) == false)
            {
                enableNotify = true;
                postDict["notify_url"] = $"{parameter.NotifyDomain}/{AlipayNotify_RequestHandler.NotifyPageName}";
            }

            var bizParameters = new SortedDictionary<string, object>
            {
                 {"out_trade_no", parameter.TradeID},
                {"subject", parameter.TradeName},
                {"body", parameter.Description},
                {"total_amount", parameter.Amount.ToString("0.00")},
                {"undiscountable_amount", "0"},
                {"timeout_express", Math.Max(1 , parameter.Timeout/60) + "m"},
                //{"seller_id", config.pid}
            };
            if (parameter.GoodsDetails.Count > 0)
            {
                var goodsDetails = new List<object>();
                foreach (var gooditem in parameter.GoodsDetails)
                {
                    goodsDetails.Add(new
                    {
                        goods_id = gooditem.GoodsId,
                        goods_name = gooditem.GoodsName,
                        quantity = gooditem.Quantity,
                        price = gooditem.Price
                    });
                }
                bizParameters["goods_detail"] = goodsDetails;
            }
            if (!string.IsNullOrEmpty(parameter.StoreId))
            {
                bizParameters["store_id"] = parameter.StoreId;
            }
            postDict["biz_content"] = Newtonsoft.Json.JsonConvert.SerializeObject( bizParameters);

            //获取签名的内容
            var signContent = Helper.GetUrlString(postDict);

            var rsa = Way.Lib.RSA.CreateRsaFromPrivateKey(config.merchantPrivateKey , Way.Lib.RSAKeyType.PKCS1);
            rsa.KeySize = 1024;

            var signatureBytes = rsa.SignData(Encoding.UTF8.GetBytes(signContent), HashAlgorithmName.SHA1 , RSASignaturePadding.Pkcs1);

            // 把密文加入提交的参数列表中
            postDict["sign"] = Convert.ToBase64String(signatureBytes);

            var result = Helper.PostQueryString(ServerUrl, Helper.BuildQuery(postDict), parameter.RequestTimeout);
            PayFactory.OnLog(parameter.TradeID, LogEventType.ReceivePayResult, result);

            // 把json结果转为对象
            var payResult = Newtonsoft.Json.JsonConvert.DeserializeObject<AlipayTradePrecreateResult>(result);

            AlipayBarcode.CheckSign(result , "alipay_trade_precreate_response" , payResult.sign , config);
           

            if (payResult.alipay_trade_precreate_response.code == "10000")
            {
                if (enableNotify == false)
                {
                    new Thread(() =>
                    {
                        CheckPayStateInLoop(parameter);
                    }).Start();
                }
                return payResult.alipay_trade_precreate_response.qr_code;
            }
            else
            {
                throw new Exception(payResult.alipay_trade_precreate_response.sub_msg);
            }
        }
        
       
    }
}
