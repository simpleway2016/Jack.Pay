using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Jack.Pay.Impls.Bboqi.AlipayBarcode
{
    [PayInterface(PayInterfaceType.BboqiWeiXinBarcode)]
    class Bboqi_WeixinBarcode : BasePay
    {
        public const string PayUrl = "http://api.bboqi.com/openapi/pay/micropay/v1";
        public const string QueryUrl = "http://api.bboqi.com/openapi/pay/orderquery/v1";
        public const string OrderUrl = "http://api.bboqi.com/openapi/pay/unifiedorder/v1";
        public override string BeginPay(PayParameter parameter)
        {
            var attrs = this.GetType().GetCustomAttributes(typeof(PayInterfaceAttribute), false);
            //获取当前接口类型
            var myInterfaceType = ((PayInterfaceAttribute)attrs[0]).InterfaceType;

            var config = new Config(PayFactory.GetInterfaceXmlConfig(myInterfaceType, parameter.TradeID));

            SortedDictionary<string, string> dict = new SortedDictionary<string, string>();
            dict["cashdesk_id"] = config.CashId??"";
            dict["cashdesk_name"] = config.CashDesc??"";
            dict["orig_fee"] = parameter.Amount.ToString();
            dict["favo_fee"] = "0";
            dict["total_fee"] = parameter.Amount.ToString();
            dict["out_trade_no"] = parameter.TradeID;

            var resultJson = Bboqi_Helper.PostJson(config, OrderUrl, dict, parameter.RequestTimeout);


            string trade_no = resultJson["trade_no"];
            dict = new SortedDictionary<string, string>();
            dict["trade_no"] = trade_no;
            dict["body"] = parameter.Description;
            dict["auth_code"] = parameter.AuthCode;
            dict["channel"] = "wx";
            var strResult = Bboqi_Helper.PostJsonReturnString(config, PayUrl, dict, parameter.RequestTimeout);

            resultJson = (SortedDictionary<string, string>)Newtonsoft.Json.JsonConvert.DeserializeObject(strResult, typeof(SortedDictionary<string, string>));
            if (resultJson["result_code"] == "FAIL" && (resultJson.ContainsKey("trade_status") == false || resultJson["trade_status"] != "2"))
                throw new Exception(resultJson["return_msgs"]);
            string serverSign = resultJson["sign"];
            if (Bboqi_Helper.Sign(config, resultJson) != serverSign)
                throw new Exception("服务器返回信息签名检验失败");

            if (resultJson["result_code"] == "SUCCESS")
            {
                if (resultJson["trade_status"] == "1")
                {
                    PayFactory.OnPaySuccessed(parameter.TradeID, parameter.Amount, null, strResult);
                    return null;
                }
                else if (resultJson["trade_status"] == "0")
                {
                    PayFactory.OnPayFailed(parameter.TradeID, resultJson["return_msgs"], strResult);
                    return null;
                }
            }
            if(resultJson.ContainsKey("trade_status") && resultJson["trade_status"] == "2")
            {
                new Thread(() => {
                    CheckPayStateInLoop(parameter);
                }).Start();
            }
            return null;
        }

     
        public override bool CheckPayState(PayParameter parameter)
        {

            var attrs = this.GetType().GetCustomAttributes(typeof(PayInterfaceAttribute), false);
            //获取当前接口类型
            var myInterfaceType = ((PayInterfaceAttribute)attrs[0]).InterfaceType;

            var config = new Config(PayFactory.GetInterfaceXmlConfig(myInterfaceType, parameter.TradeID));
            SortedDictionary<string, string> dict = new SortedDictionary<string, string>();
            dict["out_tradeno"] = parameter.TradeID;
            var result = Bboqi_Helper.PostJson(config ,QueryUrl, dict, parameter.RequestTimeout);
            if(result["pay_status"] == "2")
            {
                PayFactory.OnPayFailed(parameter.TradeID, "退款中", Newtonsoft.Json.JsonConvert.SerializeObject(result));
                return true;
            }
            else if (result["pay_status"] == "3")
            {
                PayFactory.OnPayFailed(parameter.TradeID, "已退款", Newtonsoft.Json.JsonConvert.SerializeObject(result));
                return true;
            }
            else if (result["pay_status"] == "4")
            {
                PayFactory.OnPayFailed(parameter.TradeID, "退款失败", Newtonsoft.Json.JsonConvert.SerializeObject(result));
                return true;
            }
            else if (result["pay_status"] == "5")
            {
                PayFactory.OnPayFailed(parameter.TradeID, "已撤销", Newtonsoft.Json.JsonConvert.SerializeObject(result));
                return true;
            }
            else if (result["pay_status"] == "1")
            {
                PayFactory.OnPaySuccessed(parameter.TradeID, Convert.ToDouble(result["total_fee"]), null, Newtonsoft.Json.JsonConvert.SerializeObject(result));
                return true;
            }
            return false;
        }

        public override RefundResult Refund(RefundParameter parameter)
        {
            throw new NotImplementedException();
        }
    }
}
