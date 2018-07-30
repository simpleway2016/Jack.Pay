using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;

namespace Jack.Pay.Impls.IPaysoon
{
    [PayInterface(PayInterfaceType.IPaysoonWeiXinScanQRCode)]
    class Paysoon_WeixinScanQRCode : Paysoon_WeiXinBarcode
    {
        protected override string ServerUrl => "http://gateway.ipaysoon.com/MSZFPlatform/wxpays/qrpay";
        protected override bool SupportNotify => true;

        protected override string OnBeginPayPostGetResult(string result, Config config, JObject resultDict, PayParameter parameter)
        {
            if (resultDict["resultCode"].ToString() == "0000")
            {
                var rsa = Way.Lib.RSA.CreateRsaFromPrivateKey(config.merchantPrivateKey , Way.Lib.RSAKeyType.PKCS8);
                rsa.KeySize = 1024;
                var detailJson = System.Text.Encoding.UTF8.GetString(Way.Lib.RSA.DecryptFromBase64(rsa, resultDict["detail"].ToString(), RSAEncryptionPadding.Pkcs1));

                var resultDetail = (Newtonsoft.Json.Linq.JArray)Newtonsoft.Json.JsonConvert.DeserializeObject(detailJson);
                return resultDetail[0]["url"].ToString();
            }
            else
            {
                throw new PayServerReportException(resultDict["resultMsg"].ToString());
            }
        }
    }
}
