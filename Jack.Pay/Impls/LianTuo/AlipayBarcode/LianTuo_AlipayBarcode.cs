using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Jack.Pay.Impls.LianTuo.AlipayBarcode
{
    [PayInterface(PayInterfaceType.LianTuoAlipayBarcode)]
    class LianTuo_AlipayBarcode : BasePay
    {
#if DEBUG
        public const string URL = "http://newfront.liantuobank.com/NewFront/base/gateway.in";
#else
        public const string URL = "http://newfront.liantuobank.com/NewFront/base/gateway.in";
#endif
        protected virtual string ChannelType => "ALI";
        protected virtual string ServiceType => "front.micropay";
      

        public override string BeginPay(PayParameter parameter)
        {
            var config = PayFactory.GetConfig<Config>(this.GetType(), parameter.TradeID);

            var head = new Dictionary<string, object>();
            head["service"] = ServiceType;

            var body = new Dictionary<string, object>();
            body["merchant_no"] = config.merchant_id;
            body["channel_type"] = ChannelType;
            body["out_trade_no"] = parameter.TradeID;
            body["total_amount"] = parameter.Amount.ToString();
            body["subject"] = parameter.TradeName;
            body["spbill_create_ip"] = "8.8.8.8";
            body["auth_code"] = parameter.AuthCode;

            var strResult = LianTuo_Helper.PostJsonReturnString(config, URL, head, body, parameter.RequestTimeout);
            PayFactory.OnLog(parameter.TradeID, LogEventType.ReceivePayResult, strResult);

            var responseObj = Newtonsoft.Json.JsonConvert.DeserializeObject<ResponseObject>(strResult);

            string serverSign = responseObj.head["sign"].ToString();
            if (LianTuo_Helper.Sign(config.key, responseObj.head, responseObj.body) != serverSign)
                throw new Exception("服务器返回信息签名检验失败");

            if ((string)responseObj.body["is_success"] == "S")
            {
                double? receipt_amount = null;
                try
                {
                    if (responseObj.body["receipt_amount"] != null)
                    {
                        receipt_amount = Convert.ToDouble(responseObj.body["receipt_amount"]);
                    }
                }
                catch
                {

                }
                PayFactory.OnPaySuccessed(parameter.TradeID, receipt_amount, null, strResult);
            }
            else if ((string)responseObj.body["is_success"] == "F")
            {
                throw new Exception((string)responseObj.body["message"]);
            }
            else if ((string)responseObj.body["is_success"] == "P")
            {
                new Thread(() =>
                {
                    CheckPayStateInLoop(parameter);
                }).Start();
            }
            return null;
        }

        public override bool CheckPayState(PayParameter parameter)
        {
            var config = PayFactory.GetConfig<Config>(this.GetType(), parameter.TradeID);

            var head = new Dictionary<string, object>();
            head["service"] = "front.query";

            var body = new Dictionary<string, object>();
            body["out_trade_no"] = parameter.TradeID;

            var strResult = LianTuo_Helper.PostJsonReturnString(config, URL, head, body, parameter.RequestTimeout);

            var responseObj = Newtonsoft.Json.JsonConvert.DeserializeObject<ResponseObject>(strResult);

            string serverSign = responseObj.head["sign"].ToString();
            if (LianTuo_Helper.Sign(config.key, responseObj.head, responseObj.body) != serverSign)
                throw new Exception("服务器返回信息签名检验失败");

            if ((string)responseObj.body["is_success"] == "S")
            {
                if ((string)responseObj.body["trade_status"] == "success")
                {
                    double? receipt_amount = null;
                    try
                    {
                        if (responseObj.body["receipt_amount"] != null)
                        {
                            receipt_amount = Convert.ToDouble(responseObj.body["receipt_amount"]);
                        }
                    }
                    catch
                    {

                    }
                    PayFactory.OnPaySuccessed(parameter.TradeID, receipt_amount, null, strResult);
                    return true;
                }
                else if ((string)responseObj.body["trade_status"] == "fail")
                {
                    PayFactory.OnPayFailed(parameter.TradeID, (string)responseObj.body["trade_error_msg"], strResult);
                    return true;
                }
                else if ((string)responseObj.body["trade_status"] == "closed")
                {
                    throw new Exception("订单已关闭");
                }
                else if ((string)responseObj.body["trade_status"] == "cancel")
                {
                    throw new Exception("订单已取消");
                }
            }
            return false;
        }

        public override RefundResult Refund(RefundParameter parameter)
        {
            var config = PayFactory.GetConfig<Config>(this.GetType(), parameter.TradeID);

            var head = new Dictionary<string, object>();
            head["service"] = "front.refund";

            var body = new Dictionary<string, object>();
            body["merchant_no"] = config.merchant_id;
            body["out_trade_no"] = parameter.TradeID;
            body["out_refund_no"] = Guid.NewGuid().ToString("N");
            body["refund_fee"] = parameter.Amount;
            body["refund_reason"] = parameter.Reason;
            body["spbill_create_ip"] = "8.8.8.8";

            var strResult = LianTuo_Helper.PostJsonReturnString(config, URL, head, body, 8);

            var responseObj = Newtonsoft.Json.JsonConvert.DeserializeObject<ResponseObject>(strResult);

            string serverSign = responseObj.head["sign"].ToString();
            if (LianTuo_Helper.Sign(config.key, responseObj.head, responseObj.body) != serverSign)
                throw new Exception("服务器返回信息签名检验失败");

            if ((string)responseObj.body["is_success"] == "S")
            {
                return new RefundResult {
                    Result = RefundResult.ResultEnum.SUCCESS,
                    ServerMessage = strResult
                };
            }
            else if ((string)responseObj.body["is_success"] == "F")
            {
                return new RefundResult
                {
                    Error = (string)responseObj.body["message"],
                    Result = RefundResult.ResultEnum.FAIL,
                    ServerMessage = strResult
                };
            }
            return CheckRefundStateInLoop(parameter.TradeID, (string)body["out_refund_no"]);
        }

        protected RefundResult CheckRefundStateInLoop(string tradeId, string refundNo)
        {
            DateTime startTime = DateTime.Now;
            RefundResult result = null;
            while (true)
            {
                Thread.Sleep(1000);
                if ((DateTime.Now - startTime).TotalSeconds > 120)
                {
                    throw new Exception("访问联拓服务器超时");
                }

                try
                {                   
                    result = this.CheckRefundState(tradeId, refundNo);
                    if (result != null)
                    {
                        return result;
                    }
                }
                catch (Exception ex)
                {
                    using (Log log = new Log("CheckRefundStateInLoop error"))
                    {
                        log.Log(this.GetType().FullName);
                        log.Log(ex.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// 查询退款状态
        /// </summary>
        /// <param name="tradeId"></param>
        /// <param name="refundNo">退款交易号</param>
        /// <returns></returns>
        public RefundResult CheckRefundState(string tradeId, string refundNo)
        {
            var config = PayFactory.GetConfig<Config>(this.GetType(), tradeId);

            var head = new Dictionary<string, object>();
            head["service"] = "front.refundquery";

            var body = new Dictionary<string, object>();
            body["out_refund_no"] = refundNo;

            var strResult = LianTuo_Helper.PostJsonReturnString(config, URL, head, body, 8);

            var responseObj = Newtonsoft.Json.JsonConvert.DeserializeObject<ResponseObject>(strResult);

            string serverSign = responseObj.head["sign"].ToString();
            if (LianTuo_Helper.Sign(config.key, responseObj.head, responseObj.body) != serverSign)
                throw new Exception("服务器返回信息签名检验失败");

            if ((string)responseObj.body["is_success"] == "S")
            {
                if ((string)responseObj.body["refund_status"] == "success")
                {
                    return new RefundResult
                    {
                        Result = RefundResult.ResultEnum.SUCCESS,
                        ServerMessage = strResult
                    };
                }
                else if ((string)responseObj.body["refund_status"] == "fail")
                {
                    return new RefundResult
                    {
                        Error = (string)responseObj.body["refund_error_msg"],
                        Result = RefundResult.ResultEnum.FAIL,
                        ServerMessage = strResult
                    };
                }
            }
            return null;
        }
    }
}
