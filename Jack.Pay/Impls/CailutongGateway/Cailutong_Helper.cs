using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Jack.Pay.Impls.CailutongGateway
{
    class Cailutong_Helper
    {
        public static string Sign(SortedDictionary<string, object> data, string secret)
        {
            StringBuilder str = new StringBuilder();
            foreach (var item in data)
            {
                if (item.Value == null || item.Key == "sign" || item.Value.ToString().Length == 0)
                    continue;

                if (str.Length > 0)
                    str.Append('&');
                str.Append(item.Key);
                str.Append('=');
                str.Append(item.Value);
            }
            str.Append($"&secret={secret}");
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
