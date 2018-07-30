using System;
using System.Collections.Generic;
using System.Text;
using Jack.HttpRequestHandlers;

namespace Jack.Pay.WeixinAuth
{
    class WeixinAuth_RequestHandler : IRequestHandler
    {
        public const string PageName = "JACK_PAY_Weixin_Auth.aspx";
        public string UrlPageName => PageName;

        public TaskStatus Handle(IHttpProxy httpProxy)
        {
          
            var code = httpProxy.QueryString["code"];
            if (code != null)
            {
                var tranid = httpProxy.QueryString["state"];
                var filepath = Helper.GetSaveFilePath() + "\\" + tranid;
                var dict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string,string>>(System.IO.File.ReadAllText(filepath, System.Text.Encoding.UTF8));
                try
                {
                    System.IO.File.Delete(filepath);
                }
                catch { }
                string appId = dict["appId"];
                string secret = dict["secret"];
                string returnUrl = dict["returnUrl"];
                if (returnUrl.Contains("?"))
                    returnUrl += "&";
                else
                    returnUrl += "?";

                try
                {
                    var url = $"https://api.weixin.qq.com/sns/oauth2/access_token?appid={System.Web.HttpUtility.UrlEncode(appId, Encoding.UTF8)}&secret={System.Web.HttpUtility.UrlEncode(secret, Encoding.UTF8)}&code={System.Web.HttpUtility.UrlEncode(code, Encoding.UTF8)}&grant_type=authorization_code";
                    var content = Helper.GetWebContent(url, 8);
                    var tokenObj = Newtonsoft.Json.JsonConvert.DeserializeObject<weixin_token>(content);
                    if (tokenObj.errmsg != null)
                    {
                        returnUrl += $"err={System.Web.HttpUtility.UrlEncode(tokenObj.errmsg, Encoding.UTF8)}";
                    }
                    else
                    {
                        returnUrl += $"openid={System.Web.HttpUtility.UrlEncode(tokenObj.openid, Encoding.UTF8)}&access_token={System.Web.HttpUtility.UrlEncode(tokenObj.access_token, Encoding.UTF8)}";
                    }
                }
                catch(Exception ex)
                {
                    var err = ex;
                    while(err.InnerException != null)
                    {
                        err = err.InnerException;
                    }
                    returnUrl += $"err={System.Web.HttpUtility.UrlEncode(err.Message, Encoding.UTF8)}";
                }
                httpProxy.Redirect(returnUrl);
            }
            return TaskStatus.Completed;
        }
    }

    class weixin_token
    {
        public string access_token { get; set; }

        public int? expires_in { get; set; }

        public string refresh_token { get; set; }

        public string openid { get; set; }

        public string scope { get; set; }
        public string errmsg { get; set; }

    }
    class weixin_userinfo
    {
        public string openid { get; set; }

        public string nickname { get; set; }

        public int? sex { get; set; }

        public string language { get; set; }

        public string city { get; set; }

        public string province { get; set; }

        public string country { get; set; }

        public string headimgurl { get; set; }

        public object privilege { get; set; }

    }

}
