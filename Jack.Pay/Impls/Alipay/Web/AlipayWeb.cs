using System;
using System.Collections.Generic;
using System.Text;
using Jack.Pay.Impls.Alipay;
using System.Security.Cryptography;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Jack.Pay.Impls
{
    [PayInterface(PayInterfaceType.AlipayWeb)]
    class AlipayWeb : AlipayBarcode
    {
        const string ServerUrl = "https://openapi.alipay.com/gateway.do";
        protected virtual string Method => "alipay.trade.page.pay";
        public override string BeginPay(PayParameter parameter)
        {
            var config = new Config(PayFactory.GetInterfaceXmlConfig(PayInterfaceType.AlipayWeb, parameter.TradeID));
            SortedDictionary<string, string> postDict = new SortedDictionary<string, string>();
            postDict["app_id"] = config.appid;
            postDict["method"] = Method;
            postDict["charset"] = "utf-8";
            postDict["sign_type"] = "RSA";
            postDict["timestamp"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            postDict["version"] = "1.0";

            if (string.IsNullOrEmpty(parameter.NotifyDomain) == false)
            {
                if (!string.IsNullOrEmpty(parameter.ReturnUrl))
                {
                    postDict["return_url"] = $"{parameter.NotifyDomain}/{AlipayReturn_RequestHandler.ReturnPageName}?returnUrl={System.Web.HttpUtility.UrlEncode(parameter.ReturnUrl)}";
                }
                postDict["notify_url"] = $"{parameter.NotifyDomain}/{AlipayNotify_RequestHandler.NotifyPageName}";
            }

            var bizParameters = new SortedDictionary<string, string>
            {
                 {"out_trade_no", parameter.TradeID},
                {"subject", parameter.TradeName},
                {"body", parameter.Description},
                {"total_amount", parameter.Amount.ToString("0.00")},
                {"undiscountable_amount", "0"},
                {"timeout_express", Math.Max(1 , parameter.Timeout/60) + "m"},
                { "product_code", "FAST_INSTANT_TRADE_PAY"},
            };

            postDict["biz_content"] = Newtonsoft.Json.JsonConvert.SerializeObject( bizParameters);

            //获取签名的内容
            var signContent = Helper.GetUrlString(postDict);

            var rsa = Way.Lib.RSA.CreateRsaFromPrivateKey(config.merchantPrivateKey , Way.Lib.RSAKeyType.PKCS1);
            rsa.KeySize = 1024;

            var signatureBytes = rsa.SignData(Encoding.UTF8.GetBytes(signContent), HashAlgorithmName.SHA1 , RSASignaturePadding.Pkcs1);

            // 把密文加入提交的参数列表中
            postDict["sign"] = Convert.ToBase64String(signatureBytes);

            //var result = Helper.PostQueryString(ServerUrl, Helper.BuildQuery(postDict), parameter.RequestTimeout );
            var body = BuildHtmlFormRequest(postDict, "POST");

            //先把jsonStr保存成一个临时文件
            string tranid = Guid.NewGuid().ToString("N");
            string tempFile = $"{Helper.GetSaveFilePath()}\\{tranid}.txt";
            System.IO.File.WriteAllText(tempFile, body, Encoding.UTF8);

            return $"{parameter.NotifyDomain}/{AlipayPostPage_RequestHandler.PageName}?tranId={tranid}";
            
        }

        protected string BuildHtmlFormRequest(IDictionary<string, string> sParaTemp, string strMethod)
        {
            //待请求参数数组
            IDictionary<string, string> dicPara = new Dictionary<string, string>();
            dicPara = sParaTemp;
            var formid = Guid.NewGuid().ToString("N");
            StringBuilder sbHtml = new StringBuilder();
            //sbHtml.Append("<head><meta http-equiv=\"Content-Type\" content=\"text/html\" charset= \"" + charset + "\" /></head>");

            sbHtml.Append("<form id='"+ formid + "' name='"+ formid + "' action='" + ServerUrl + "?charset=utf-8" + 
                 "' method='" + strMethod + "'>");
            ;
            foreach (KeyValuePair<string, string> temp in dicPara)
            {

                sbHtml.Append("<input type='hidden' name='" + temp.Key + "' value='" + temp.Value + "'/>");

            }

            //submit按钮控件请不要含有name属性
            sbHtml.Append("<input type='submit' value='POST' style='display:none;'></form>");

            //表单实现自动提交
            sbHtml.Append("<script>document.getElementById('"+ formid + "').submit();</script>");

            return sbHtml.ToString();
        }
    }
}
