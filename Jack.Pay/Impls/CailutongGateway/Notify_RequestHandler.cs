using System;
using System.Collections.Generic;
using System.Text;
using Jack.HttpRequestHandlers;

namespace Jack.Pay.Impls.CailutongGateway
{
    class Notify_RequestHandler : Jack.HttpRequestHandlers.IRequestHandler
    {
        public const string NotifyPageName = "JACK_PAY_Cailutong_Notify";
        public string UrlPageName => NotifyPageName;

        public TaskStatus Handle(IHttpProxy httpProxy)
        {
            using (Log log = new Log("CailutongGateway.Notify_RequestHandler"))
            {
                try
                {
                  

                    string json = httpProxy.ReadRequestBody();
                    log.Log(json);

                    SortedDictionary<string, object> dict = Newtonsoft.Json.JsonConvert.DeserializeObject<SortedDictionary<string, object>>(json);
                    string outTradeNo = (string)dict["outTradeNo"];
                    var config = new Config(PayFactory.GetInterfaceXmlConfig(PayInterfaceType.CailutongBTC, outTradeNo));

                    if (Cailutong_Helper.Sign(dict, config.Secret) != (string)dict["sign"])
                        throw new Exception("校验失败");

                    var status = Convert.ToInt32(dict["status"]);
 
                    if (status == 2)
                    {
                        PayFactory.OnPaySuccessed(outTradeNo, Convert.ToDouble(dict["payedAmount"]), null, json);
                    }

                    httpProxy.ResponseWrite("{\"status\":\"success\"}");
                }
                catch (Exception ex)
                {
                    log.Log(ex.ToString());
                }
            }
            return TaskStatus.Completed;
        }
    }
}
