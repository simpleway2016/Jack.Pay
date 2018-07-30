using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Jack.Pay.Impls.Bboqi.WeixinJsApi
{
    [PayInterface(PayInterfaceType.BboqiWeiXinJSApi)]
    class Meituan_WeixinJsApi : Bboqi.AlipayBarcode.Bboqi_WeixinBarcode
    {
        const string Url = "http://api.bboqi.com/openapi/pay/unifiedorder/wxapp/v1";
        public override string BeginPay(PayParameter parameter)
        {
            var attrs = this.GetType().GetCustomAttributes(typeof(PayInterfaceAttribute), false);
            //获取当前接口类型
            var myInterfaceType = ((PayInterfaceAttribute)attrs[0]).InterfaceType;

            var config = new Config(PayFactory.GetInterfaceXmlConfig(myInterfaceType, parameter.TradeID));

            SortedDictionary<string, string> dict = new SortedDictionary<string, string>();
            dict["cashdesk_id"] = config.CashId ?? "";
            dict["cashdesk_name"] = config.CashDesc ?? "";
            dict["openid"] = parameter.AuthCode;
            dict["orig_fee"] = parameter.Amount.ToString();
            dict["favo_fee"] = "0";
            dict["total_fee"] = parameter.Amount.ToString();
            dict["out_trade_no"] = parameter.TradeID;

            var resultJson = Bboqi_Helper.PostJson(config, Url, dict, parameter.RequestTimeout);          

            var returnDict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string,object>>( resultJson["jsapipay"].Replace("\"packages\"" , "\"package\""));
            returnDict["ReturnUrl"] = parameter.ReturnUrl;
            returnDict["TradeID"] = parameter.TradeID;
            var jsonStr = Newtonsoft.Json.JsonConvert.SerializeObject(returnDict);

            //先把jsonStr保存成一个临时文件
            string tranid = Guid.NewGuid().ToString("N");
            string tempFile = $"{Helper.GetSaveFilePath()}\\{tranid}.txt";
            System.IO.File.WriteAllText(tempFile, jsonStr, Encoding.UTF8);

            new Thread(() => {
                CheckPayStateInLoop(parameter);
            }).Start();

            return $"{parameter.NotifyDomain}/{Weixin.WeiXinPayRedirect_RequestHandler.NotifyPageName}?tranId={tranid}";
        }
        
        
    }
}
