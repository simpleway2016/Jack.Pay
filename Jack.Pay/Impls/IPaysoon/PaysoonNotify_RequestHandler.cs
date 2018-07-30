using System;
using System.Collections.Generic;
using System.Text;


using System.Xml.Linq;
using System.Xml.XPath;
using System.Security.Cryptography;
using Way.Lib;
using Jack.HttpRequestHandlers;
#if NET46
#else
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Internal;
#endif


namespace Jack.Pay.Impls.IPaysoon
{
    /// <summary>
    /// 处理微信通知信息
    /// </summary>
    class Paysoon_RequestHandler:IRequestHandler
    {
        public const string NotifyPageName = "JACK_PAY_IPaysoonNotify_HttpHandler.aspx";

        public string UrlPageName => NotifyPageName;


        public TaskStatus Handle(IHttpProxy httpProxy)
        {
            var json = httpProxy.Form["request"];
            try
            {
                using (CLog log = new CLog("IPaysoonNotify Notify"))
                {
                    log.Log(json);
                    var result = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                    if (result["resultCode"].ToString() == "0000" && result["statusId"].ToString() == "14")
                    {
                        var tradid = result["merchantGenCode"].ToString();
                        var charge = Convert.ToDouble(result["charge"].ToString()) / 100.0;
                        var amount = Convert.ToDouble(result["amount"].ToString()) / 100.0;
                        log.Log("tradid:{0} charge:{1} amount:{2}", tradid, charge, amount);
                        log.Log("excute OnPaySuccessed");
                        PayFactory.OnPaySuccessed(tradid, amount - charge, null, json);
                    }
                    httpProxy.ResponseWrite( "SUCCESS");
                }
            }
            catch (Exception ex)
            {
                using (CLog log = new CLog("IPaysoon Notify error "))
                {
                    log.Log(ex.ToString());
                }
            }
            return TaskStatus.Completed;
        }
    }
    
}
