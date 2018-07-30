using System;
using System.Collections.Generic;
using System.Text;
using Jack.HttpRequestHandlers;

namespace Jack.Pay.Impls.LianTuo.WeixinJsApi
{
    /// <summary>
    /// 公众号支付的回调处理
    /// </summary>
    class PayResult_RequestHandler : Jack.HttpRequestHandlers.IRequestHandler
    {
        public const string NotifyPageName = "Jack.Pay.LianTuo.WXJSApi.Result";
        public string UrlPageName => NotifyPageName;

        public TaskStatus Handle(IHttpProxy httpProxy)
        {
            try
            {
                var requestJson = httpProxy.Form["requestJson"];
                if (!string.IsNullOrEmpty(requestJson))
                {
                  
                    var responseObj = Newtonsoft.Json.JsonConvert.DeserializeObject<ResponseObject>(requestJson);
                    var tradeId = responseObj.body["out_trade_no"].ToString();
                    var config = PayFactory.GetConfig<Config>(typeof(LianTuo_WeixinJsApi), tradeId);

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
                        PayFactory.OnPaySuccessed(tradeId, receipt_amount, null, requestJson);
                    }
                    else if ((string)responseObj.body["is_success"] == "F")
                    {
                        PayFactory.OnPayFailed(tradeId, (string)responseObj.body["message"], requestJson);
                    }
                }
                httpProxy.ResponseWrite("success");
            }
            catch(Exception ex)
            {
                using (Log log = new Log("Jack.Pay.LianTuo.WXJSApi.Result Error", false))
                {
                    log.Log(ex.ToString());
                    log.LogJson(httpProxy.Form);
                }
            }
            return TaskStatus.Completed;
        }      

    }
}
