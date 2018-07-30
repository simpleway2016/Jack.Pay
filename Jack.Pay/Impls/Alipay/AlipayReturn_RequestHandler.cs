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

namespace Jack.Pay.Impls.Alipay
{
    /// <summary>
    /// 处理支付宝通知信息
    /// </summary>
    class AlipayReturn_RequestHandler:IRequestHandler
    {
       
        public const string ReturnPageName = "JACK_PAY_AlipayReturn_HttpHandler.aspx";

        public string UrlPageName => ReturnPageName;

        public TaskStatus Handle(IHttpProxy httpProxy)
        {
            var out_trade_no = httpProxy.QueryString["out_trade_no"];
            var returnurl = httpProxy.QueryString["returnUrl"];         

            if (returnurl.Contains("?") == false)
                returnurl += "?";
            else
                returnurl += "&";

            AlipayBarcode api = new AlipayBarcode();
            var payStatus = api.GetPayState(new PayParameter {
                TradeID = out_trade_no
            });

            httpProxy.Redirect($"{returnurl}tradeId={System.Net.WebUtility.UrlEncode(out_trade_no)}&payStatus=" + payStatus);

            return TaskStatus.Completed;
        }
    }

}
