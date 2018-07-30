using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Jack.Pay.Impls.Bboqi
{
    public class Bboqi_Helper
    {
        internal const string Url = "http://api.bboqi.com/openapi/shopconfig/v1";
        /// <summary>
        /// 注册店铺
        /// </summary>
        /// <param name="ourshopid">我们自己的门店编号</param>
        public static string RegisterShop(string ourshopid)
        {
            var config = new Config(PayFactory.GetInterfaceXmlConfig( PayInterfaceType.Bboqi, null));
            SortedDictionary<string, string> dict = new SortedDictionary<string, string>();
            dict["appid"] = config.AppId;
            dict["nonce_str"] = Guid.NewGuid().ToString("N");
            dict["timestamp"] = Helper.ConvertDateTimeInt(DateTime.Now).ToString();
            dict["shop_code"] = ourshopid;
            dict["sign"] = Bboqi_Helper.Sign(config, dict);
            dict = PostJson(config , Url, dict, 8);
            var bs = Convert.FromBase64String(dict["shopconfig"]);
            return System.Text.Encoding.UTF8.GetString(bs);
        }
        internal static SortedDictionary<string, string> PostJson(Config config, string url , SortedDictionary<string,string> dict , int timeout)
        {
            string result = PostJsonReturnString(config,url , dict,timeout);
            var resultJson = (SortedDictionary<string, string>)Newtonsoft.Json.JsonConvert.DeserializeObject(result, typeof(SortedDictionary<string, string>));
            if (resultJson["result_code"] == "FAIL")
                throw new Exception(resultJson["return_msgs"]);
            string serverSign = resultJson["sign"];
            if (Bboqi_Helper.Sign(config, resultJson) != serverSign)
                throw new Exception("服务器返回信息签名检验失败");
            return resultJson;
        }
        internal static string PostJsonReturnString(Config config, string url, SortedDictionary<string, string> dict, int timeout)
        {
            dict["appid"] = config.AppId;
            dict["nonce_str"] = Guid.NewGuid().ToString("N");
            dict["timestamp"] = Helper.ConvertDateTimeInt(DateTime.Now).ToString();
            if (dict.ContainsKey("shop_code") == false)
            {
                dict["shop_code"] = config.ShopId;
            }
            dict["sign"] = Bboqi_Helper.Sign(config, dict);
            string result = Helper.PostJsonString(url, Newtonsoft.Json.JsonConvert.SerializeObject(dict), timeout);
            return result;
        }
        internal static string Sign(Config config , SortedDictionary<string,string> param)
        {
            StringBuilder str = new StringBuilder();
            foreach( var item in param )
            {
                if (item.Key == "sign")
                    continue;
                if (str.Length > 0)
                    str.Append('&');
                str.Append(item.Key);
                str.Append('=');
                str.Append(item.Value);
            }
            str.Append(config.Key);
            using (MD5 md5Hash = MD5.Create())
            {
                var bs = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(str.ToString()));
                var sb = new StringBuilder();
                foreach (byte b in bs)
                {
                    sb.Append(b.ToString("x2"));
                }
                //所有字符转为小写
                return sb.ToString().ToLower();
            }
        }
    }
}
