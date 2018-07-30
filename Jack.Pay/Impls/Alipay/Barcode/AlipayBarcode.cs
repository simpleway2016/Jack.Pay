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
    [PayInterface(PayInterfaceType.AlipayBarcode)]
    class AlipayBarcode : BasePay
    {
        const string ServerUrl = "https://openapi.alipay.com/gateway.do";

        internal static void CheckSign(string body, string bodyName , string sign,Config config)
        {
            //获取结果集里的签名内容
            var signStr = body.Substring(("{\"" + bodyName + "\":").Length);
            signStr = signStr.Substring(0, signStr.IndexOf(",\"sign\":"));

            RSA rsacore = Way.Lib.RSA.CreateRsaFromPublicKey(config.alipayPublicKey);
            //进行签名验证
            var isPass = rsacore.VerifyData(Encoding.GetEncoding("utf-8").GetBytes(signStr), Convert.FromBase64String(sign), HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);
            if (!isPass)
            {
                throw new Exception("服务器返回的结果不能通过签名验证");
            }
        }

        public override string BeginPay(PayParameter parameter)
        {
            var config = new Config(PayFactory.GetInterfaceXmlConfig(PayInterfaceType.AlipayBarcode, parameter.TradeID));
            SortedDictionary<string, string> postDict = new SortedDictionary<string, string>();
            postDict["app_id"] = config.appid;
            postDict["method"] = "alipay.trade.pay";
            postDict["charset"] = "utf-8";
            postDict["sign_type"] = "RSA";
            postDict["timestamp"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            postDict["version"] = "1.0";

            var bizParameters = new SortedDictionary<string, object>
            {
                {"out_trade_no", parameter.TradeID},
                {"subject", parameter.TradeName},
                {"body", parameter.Description},
                {"total_amount", parameter.Amount.ToString("0.00")},
                {"undiscountable_amount", "0"},
                {"scene", "bar_code"},      // 支付场景 条码支付
                {"auth_code", parameter.AuthCode},  //客户出示的二维码
                {"timeout_express", Math.Max(1 , parameter.Timeout/60) + "m"},  // 订单允许的最晚付款时间 为两分钟
                //{"operator_id", "app"},     // 商户操作员编号
                //{"store_id", "NJ_001"},     // 商户门店编号
                //{"terminal_id", "NJ_T_001"},// 商户机具终端编号
                //{"seller_id", config.pid}
            };

            if(parameter.GoodsDetails.Count > 0)
            {
                var goodsDetails = new List<object>();
                foreach( var gooditem in parameter.GoodsDetails )
                {
                    goodsDetails.Add(new {
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

            // 把json结果转为对象
            var payResult = Newtonsoft.Json.JsonConvert.DeserializeObject<AlipayTradePayResult>(result);

            CheckSign(result, "alipay_trade_pay_response", payResult.sign, config);

            // 明确一定是错误的代码
            string[] errorCodes = { "20000", "20001", "40001", "40002", "40003", "40004", "40006" };
            if (payResult.alipay_trade_pay_response.code == "10003")
                throw new PayServerReportException("二维码重复提交");

            if (errorCodes.Contains(payResult.alipay_trade_pay_response.code))
            {
                //明确交易失败了
                throw new PayServerReportException(payResult.alipay_trade_pay_response.sub_msg);
            }
            else if (payResult.alipay_trade_pay_response.code == "10000")
            {
                double? discountAmount = null;
                if( !string.IsNullOrEmpty( payResult.alipay_trade_pay_response.discount_goods_detail ))
                {
                    var discount_details =  Newtonsoft.Json.JsonConvert.DeserializeObject<AlipayTradePayResult.Discount_goods_detail[]>(payResult.alipay_trade_pay_response.discount_goods_detail);
                    discountAmount = discount_details.Sum(m => Convert.ToDouble(m.discount_amount));
                }
                //明确交易成功了
                PayFactory.OnPaySuccessed(parameter.TradeID, payResult.alipay_trade_pay_response.receipt_amount, discountAmount, result);
            }
            else  //到这里，不能确定支付结果
            {
                new Thread(() => {
                    CheckPayStateInLoop(parameter);
                }).Start();
            }

            return null;
        }

       
        public override bool CheckPayState(PayParameter parameter)
        {
            var config = new Config(PayFactory.GetInterfaceXmlConfig(PayInterfaceType.AlipayBarcode, parameter.TradeID));
            SortedDictionary<string, string> postDict = new SortedDictionary<string, string>();
            postDict["app_id"] = config.appid;
            postDict["method"] = "alipay.trade.query";
            postDict["charset"] = "utf-8";
            postDict["sign_type"] = "RSA";
            postDict["timestamp"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            postDict["version"] = "1.0";

            var bizParameters = new SortedDictionary<string, string>
            {
                {"out_trade_no", parameter.TradeID},
            };

            postDict["biz_content"] = Newtonsoft.Json.JsonConvert.SerializeObject(bizParameters);

            //获取签名的内容
            var signContent = Helper.GetUrlString(postDict);

            var rsa = Way.Lib.RSA.CreateRsaFromPrivateKey(config.merchantPrivateKey , Way.Lib.RSAKeyType.PKCS1);
            rsa.KeySize = 1024;

            var signatureBytes = rsa.SignData(Encoding.UTF8.GetBytes(signContent), HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);

            // 把密文加入提交的参数列表中
            postDict["sign"] = Convert.ToBase64String(signatureBytes);

            var result = Helper.PostQueryString(ServerUrl, Helper.BuildQuery(postDict), parameter.RequestTimeout);
            PayFactory.OnLog(parameter.TradeID, LogEventType.ReceivePayResult, result);

            // 把json结果转为对象
            var payResult = Newtonsoft.Json.JsonConvert.DeserializeObject<AlipayTradeQueryResult>(result);

            CheckSign(result, "alipay_trade_query_response", payResult.sign, config);
                      

            if (payResult.alipay_trade_query_response.code == "10000" && (payResult.alipay_trade_query_response.trade_status == "TRADE_SUCCESS" || payResult.alipay_trade_query_response.trade_status == "TRADE_FINISHED"))
            {
                //明确交易成功了
                PayFactory.OnPaySuccessed(parameter.TradeID,payResult.alipay_trade_query_response.receipt_amount, null, result);
                return true;
            }

            return false;
        }

        public string GetPayState(PayParameter parameter)
        {
            var config = new Config(PayFactory.GetInterfaceXmlConfig(PayInterfaceType.AlipayBarcode, parameter.TradeID));
            SortedDictionary<string, string> postDict = new SortedDictionary<string, string>();
            postDict["app_id"] = config.appid;
            postDict["method"] = "alipay.trade.query";
            postDict["charset"] = "utf-8";
            postDict["sign_type"] = "RSA";
            postDict["timestamp"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            postDict["version"] = "1.0";

            var bizParameters = new SortedDictionary<string, string>
            {
                {"out_trade_no", parameter.TradeID},
            };

            postDict["biz_content"] = Newtonsoft.Json.JsonConvert.SerializeObject(bizParameters);

            //获取签名的内容
            var signContent = Helper.GetUrlString(postDict);

            var rsa = Way.Lib.RSA.CreateRsaFromPrivateKey(config.merchantPrivateKey, Way.Lib.RSAKeyType.PKCS1);
            rsa.KeySize = 1024;

            var signatureBytes = rsa.SignData(Encoding.UTF8.GetBytes(signContent), HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);

            // 把密文加入提交的参数列表中
            postDict["sign"] = Convert.ToBase64String(signatureBytes);

            var result = Helper.PostQueryString(ServerUrl, Helper.BuildQuery(postDict), parameter.RequestTimeout);
            PayFactory.OnLog(parameter.TradeID, LogEventType.ReceivePayResult, result);

            // 把json结果转为对象
            var payResult = Newtonsoft.Json.JsonConvert.DeserializeObject<AlipayTradeQueryResult>(result);

            CheckSign(result, "alipay_trade_query_response", payResult.sign, config);


            if (payResult.alipay_trade_query_response.code == "10000" && (payResult.alipay_trade_query_response.trade_status == "TRADE_SUCCESS" || payResult.alipay_trade_query_response.trade_status == "TRADE_FINISHED"))
            {
                //明确交易成功了
                return "success";
            }

            return null;
        }

        public override RefundResult Refund(RefundParameter parameter)
        {
            var config = new Config(PayFactory.GetInterfaceXmlConfig(PayInterfaceType.AlipayBarcode, parameter.TradeID));
            SortedDictionary<string, string> postDict = new SortedDictionary<string, string>();
            postDict["app_id"] = config.appid;
            postDict["method"] = "alipay.trade.refund";
            postDict["charset"] = "utf-8";
            postDict["sign_type"] = "RSA";
            postDict["timestamp"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            postDict["version"] = "1.0";

            var bizParameters = new SortedDictionary<string, string>
            {
                {"out_trade_no", parameter.TradeID},
                {"refund_amount", parameter.Amount.ToString("0.00")},
            };

            postDict["biz_content"] = Newtonsoft.Json.JsonConvert.SerializeObject(bizParameters);

            //获取签名的内容
            var signContent = Helper.GetUrlString(postDict);

            var rsa = Way.Lib.RSA.CreateRsaFromPrivateKey(config.merchantPrivateKey , Way.Lib.RSAKeyType.PKCS1);
            rsa.KeySize = 1024;

            var signatureBytes = rsa.SignData(Encoding.UTF8.GetBytes(signContent), HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);

            // 把密文加入提交的参数列表中
            postDict["sign"] = Convert.ToBase64String(signatureBytes);

            var result = Helper.PostQueryString(ServerUrl, Helper.BuildQuery(postDict), 15);
            // 把json结果转为对象
            var refundResult = Newtonsoft.Json.JsonConvert.DeserializeObject<AlipayTradeRefundResult>(result);

            CheckSign(result, "alipay_trade_refund_response", refundResult.sign, config);

            if (refundResult.alipay_trade_refund_response.code == "10000")
            {
                return new RefundResult() {
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


       
    }
}
