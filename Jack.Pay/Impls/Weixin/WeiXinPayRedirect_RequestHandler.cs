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


namespace Jack.Pay.Impls.Weixin
{

    class WeiXinPayRedirect_RequestHandler : IRequestHandler
    {
        public const string NotifyPageName = "JACK_PAY_WeiXinPayRedirect_HttpHandler";
        public string UrlPageName => NotifyPageName;

        public TaskStatus Handle(IHttpProxy httpHandler)
        {
            var tranId = httpHandler.QueryString["tranId"];

            //读取临时文件，还原PayParameter参数
            string tempFile = $"{Helper.GetSaveFilePath()}\\{tranId}.txt";
            var dict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(System.IO.File.ReadAllText(tempFile, Encoding.UTF8));
            var returnUrl = dict["ReturnUrl"];
            var tradeID = dict["TradeID"];

            //移除不要的key
            dict.Remove("ReturnUrl");
            dict.Remove("TradeID");

            if (returnUrl.Contains("?"))
                returnUrl += "&";
            else
                returnUrl += "?";

            returnUrl += $"tradeId={tradeID}";

            var jsonStr = Newtonsoft.Json.JsonConvert.SerializeObject(dict);
            var html = Helper.ReadContentFromResourceStream("Jack.Pay.Impls.WeiXinPayPage.html");
            html = html.Replace("<%=payContent%>", jsonStr).Replace("<%=payReturnUrl%>", returnUrl);
            using (Log log = new Log("发起微信JsApi支付"))
            {
                log.LogJson(dict);
                log.Log(html);
            }
                httpHandler.ResponseWrite(html);

            return TaskStatus.Completed;
        }
    }

}
