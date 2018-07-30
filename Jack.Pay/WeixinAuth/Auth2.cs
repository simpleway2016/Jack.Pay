using System;
using System.Collections.Generic;
using System.Text;

namespace Jack.Pay.WeixinAuth
{
    /// <summary>
    /// 微信auth2.0验证
    /// </summary>
    public class Auth2
    {
        /// <summary>
        /// 应用授权作用域
        /// </summary>
        public enum Scope
        {
            /// <summary>
            /// 静默授权，可获取成员的基础信息
            /// </summary>
            snsapi_base = 0,
            /// <summary>
            /// 静默授权，可获取成员的详细信息，但不包含手机、邮箱
            /// </summary>
            snsapi_userinfo = 1,
            /// <summary>
            /// 手动授权，可获取成员的详细信息，包含手机、邮箱
            /// </summary>
            snsapi_privateinfo = 2
        }

        /// <summary>
        /// 获取openid
        /// 请先登录微信公众号管理后台，设置“网页授权域名”为returnUrl相同的域名 如：www.test.com
        /// </summary>
        /// <param name="appId">开发者ID(AppID)</param>
        /// <param name="secret">公众号开发者密码(AppSecret)</param>
        /// <param name="scope">应用授权作用域</param>
        /// <param name="returnUrl">获取到openid后的返回地址，返回形式为http://returnUrl?openid=**&access_token=***，如果有错误，则http://returnUrl?err=**</param>
        /// <returns>返回url，用于跳转</returns>
        public static string GetOpenId(string appId,string secret, Scope scope,string returnUrl)
        {
            var uri = new Uri(returnUrl);
            var redirect_uri = $"{uri.Scheme}://{uri.Host}{((uri.Port == 0 || uri.Port == 80) ? "" : ":" + uri.Port)}/{WeixinAuth_RequestHandler.PageName}";

            var tranid = Guid.NewGuid().ToString("N");
            var tempFile = Helper.GetSaveFilePath() + "\\" + tranid;
            System.IO.File.WriteAllText(tempFile, Newtonsoft.Json.JsonConvert.SerializeObject( new {
                appId = appId,
                secret = secret,
                returnUrl = returnUrl
            }), Encoding.UTF8);

            redirect_uri = System.Web.HttpUtility.UrlEncode(redirect_uri);

            var url = $"https://open.weixin.qq.com/connect/oauth2/authorize?appid={appId}&redirect_uri={redirect_uri}&response_type=code&scope={scope}&state={tranid}#wechat_redirect";
            return url;
        }

        /// <summary>
        /// 通过openid获取用户信息
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="openid"></param>
        /// <returns></returns>
        public static string GetUserInfo(string access_token,string openid)
        {
            var url = $"https://api.weixin.qq.com/sns/userinfo?access_token={System.Web.HttpUtility.UrlEncode(access_token, Encoding.UTF8)}&openid={System.Web.HttpUtility.UrlEncode(openid, Encoding.UTF8)}";
            return Helper.GetWebContent(url, 8);
        }
    }
}
