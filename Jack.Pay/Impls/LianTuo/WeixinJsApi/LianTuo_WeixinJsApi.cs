using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Jack.Pay.Impls.LianTuo.WeixinJsApi
{
    [PayInterface(PayInterfaceType.LianTuoWeixinJSApi)]
    class LianTuo_WeixinJsApi:LianTuo.AlipayBarcode.LianTuo_AlipayBarcode
    {
        public override string BeginPay(PayParameter parameter)
        {
            bool enableNotify = false;
            var config = PayFactory.GetConfig<Config>(this.GetType(), parameter.TradeID);

            var head = new Dictionary<string, object>();
            head["service"] = "front.jsapi";

            var body = new Dictionary<string, object>();
            body["merchant_no"] = config.merchant_id;
            body["channel_type"] = "WX";
            body["out_trade_no"] = parameter.TradeID;
            body["total_amount"] = parameter.Amount.ToString();
            body["subject"] = parameter.TradeName;
            body["spbill_create_ip"] = "8.8.8.8";
            body["open_id"] = parameter.AuthCode;
            body["sub_appid"] = config.weixin_appid;
            if (!string.IsNullOrEmpty(parameter.NotifyDomain))
            {
                enableNotify = true;
                body["notify_url"] = $"{parameter.NotifyDomain}/{PayResult_RequestHandler.NotifyPageName}";
            }
           

            var strResult = LianTuo_Helper.PostJsonReturnString(config, URL, head, body, parameter.RequestTimeout);
            PayFactory.OnLog(parameter.TradeID, LogEventType.ReceivePayResult, strResult);

            var responseObj = Newtonsoft.Json.JsonConvert.DeserializeObject<ResponseObject>(strResult);

            string serverSign = responseObj.head["sign"].ToString();
            if (LianTuo_Helper.Sign(config.key, responseObj.head, responseObj.body) != serverSign)
                throw new Exception("服务器返回信息签名检验失败");

            if ((string)responseObj.body["is_success"] == "S")
            {
                if (enableNotify == false)
                {
                    new Thread(() =>
                    {
                        CheckPayStateInLoop(parameter);
                    }).Start();
                }
                //pay_info参数是微信jsapi发起的参数
                var jsonWeiXinBody = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(responseObj.body["pay_info"].ToString());
                Dictionary<string, string> returnDict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonWeiXinBody);

                returnDict["ReturnUrl"] = parameter.ReturnUrl;
                returnDict["TradeID"] = parameter.TradeID;
                var jsonStr = Newtonsoft.Json.JsonConvert.SerializeObject(returnDict);


                //先把jsonStr保存成一个临时文件
                string tranid = Guid.NewGuid().ToString("N");
                string tempFile = $"{Helper.GetSaveFilePath()}\\{tranid}.txt";
                System.IO.File.WriteAllText(tempFile, jsonStr, Encoding.UTF8);

                return $"{parameter.NotifyDomain}/{Weixin.WeiXinPayRedirect_RequestHandler.NotifyPageName}?tranId={tranid}";
            }
            else if ((string)responseObj.body["is_success"] == "F")
            {
                throw new Exception((string)responseObj.body["message"]);
            }
            return null;
        }
    }
}
