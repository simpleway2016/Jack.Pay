using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Jack.Pay.Impls.CailutongGateway.BTC
{
    [PayInterface(PayInterfaceType.CailutongBTC)]
    class CailutongBTC : BasePay
    {
        public const string Url = "http://47.75.159.73:9880/api/pay";
        public const string QueryUrl = "http://47.75.159.73:9880/api/query";

        /// <summary>
        /// 获取Config对象
        /// </summary>
        /// <param name="tradeId"></param>
        /// <returns></returns>
        internal Config getConfig(string tradeId)
        {
            return PayFactory.GetConfig<Config>(this.GetType(), tradeId);
        }

        public override string BeginPay(PayParameter parameter)
        {
            var config = getConfig(parameter.TradeID);

            SortedDictionary<string, object> postDict = new SortedDictionary<string, object>();
            postDict["account"] = config.Account;
            postDict["outTradeNo"] = parameter.TradeID;
            postDict["amount"] = parameter.Amount;
            if (string.IsNullOrEmpty(parameter.NotifyDomain) == false)
            {
                postDict["notifyUrl"] = $"{parameter.NotifyDomain}/{Notify_RequestHandler.NotifyPageName}";
            }
            postDict["currency"] = "BTC";
            postDict["sign"] = Cailutong_Helper.Sign(postDict, config.Secret);
            var result = Helper.PostJsonString(Url, Newtonsoft.Json.JsonConvert.SerializeObject(postDict), 8000);
            var resultDict = Newtonsoft.Json.JsonConvert.DeserializeObject<SortedDictionary<string,object>>(result);

            if(Cailutong_Helper.Sign(resultDict, config.Secret) != (string)resultDict["sign"])
            {
                throw new Exception("服务器返回的数据校验失败");
            }

            if((string)resultDict["status"] == "error")
            {
                throw new Exception((string)resultDict["errMsg"]);
            }

            return $"bitcoin:{resultDict["targetAddress"]}";
        }

        public override bool CheckPayState(PayParameter parameter)
        {
            var config = getConfig(parameter.TradeID);

            SortedDictionary<string, object> postDict = new SortedDictionary<string, object>();
            postDict["outTradeNo"] = parameter.TradeID;
            postDict["sign"] = Cailutong_Helper.Sign(postDict, config.Secret);

            var result = Helper.PostJsonString(QueryUrl, Newtonsoft.Json.JsonConvert.SerializeObject(postDict), 8000);
            var resultDict = Newtonsoft.Json.JsonConvert.DeserializeObject<SortedDictionary<string, object>>(result);

            if (Cailutong_Helper.Sign(resultDict, config.Secret) != (string)resultDict["sign"])
            {
                throw new Exception("服务器返回的数据校验失败");
            }

            var status = Convert.ToInt32( resultDict["status"]);
            if (status <= 1)
                return false;//未支付 或者支付金额不足
            if (status == 999)
                throw new Exception("交易已无效");

            if(status == 2)
            {
                PayFactory.OnPaySuccessed(parameter.TradeID, Convert.ToDouble(resultDict["payedAmount"]), null, result);
                return true;
            }

            return false;
        }

        public override RefundResult Refund(RefundParameter parameter)
        {
            throw new NotImplementedException();
        }
    }
}
