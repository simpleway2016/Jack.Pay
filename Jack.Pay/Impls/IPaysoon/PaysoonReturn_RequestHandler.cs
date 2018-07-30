using System;
using System.Collections.Generic;
using System.Text;

#if NET46
#else
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Internal;
#endif
using System.Xml.Linq;
using System.Xml.XPath;
using System.Security.Cryptography;
using System.IO;
using Way.Lib;
using Jack.HttpRequestHandlers;

namespace Jack.Pay.Impls.IPaysoon
{
    /// <summary>
    /// 处理支付宝通知信息
    /// </summary>
    class PaysoonReturn_RequestHandler:IRequestHandler
    {
        public static System.Collections.Concurrent.ConcurrentDictionary<string, string> ReturnUrlDict = new System.Collections.Concurrent.ConcurrentDictionary<string, string>();
        public const string ReturnPageName = "JACK_PAY_PaysoonReturn_HttpHandler.aspx";

        public string UrlPageName => ReturnPageName;


        public TaskStatus Handle(IHttpProxy httpProxy)
        {
            var out_trade_no = httpProxy.QueryString["out_trade_no"];

            string returnurl;
            ReturnUrlDict.TryRemove(out_trade_no, out returnurl);
            if (returnurl.Contains("?") == false)
                returnurl += "?";
            else
                returnurl += "&";
            httpProxy.Redirect($"{returnurl}tradeId={System.Net.WebUtility.UrlEncode(out_trade_no)}");

            return TaskStatus.Completed;
        }
    }

}
