using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Jack.Pay.Impls.LianTuo
{
    class LianTuo_Helper
    {
        internal static string PostJsonReturnString(Config config, string url, IDictionary<string, object> head, IDictionary<string, object> body, int timeout )
        {
            head["version"] = "1.0";
            head["partner_id"] = config.partner_id;
            head["core_merchant_no"] = config.core_merchant_no;
            head["input_charset"] = "UTF-8";
            head["sign_type"] = "MD5";
            head["sign"] = Sign(config.key, head,body);

            var postParam = new Dictionary<string, object> {
                { "head",head },
                { "body",body }
            };

            return Helper.PostQueryString(url, "requestJson=" +System.Web.HttpUtility.UrlEncode( Newtonsoft.Json.JsonConvert.SerializeObject(postParam)), timeout);
        }
     
        internal static string Sign(string key, IDictionary<string, object> head,IDictionary<string,object> body)
        {
            StringBuilder str = new StringBuilder();
            var param = new SortedDictionary<string, object>();
            foreach (var item in head)
            {
                if ( item.Value == null || string.IsNullOrEmpty(item.Value.ToString()))
                    continue;
                param.Add(item.Key, item.Value);
            }
            foreach (var item in body)
            {
                if (item.Value == null || string.IsNullOrEmpty(item.Value.ToString()))
                    continue;
                param.Add(item.Key, item.Value);
            }
            foreach (var item in param)
            {
                if (item.Key == "sign" || item.Key == "sign_type")
                    continue;

                if (str.Length > 0)
                    str.Append('&');
                str.Append(item.Key);
                str.Append('=');
                str.Append(item.Value);
            }
            str.Append(key);

            using (MD5 md5Hash = MD5.Create())
            {
                var bs = md5Hash.ComputeHash(Encoding.GetEncoding(param["input_charset"].ToString()).GetBytes(str.ToString()));
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
