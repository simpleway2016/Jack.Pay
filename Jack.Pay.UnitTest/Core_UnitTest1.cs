using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.Threading;
using System.Security.Cryptography;
using System.Linq;
using System.Text;

namespace Jack.Pay.UnitTest
{
    [TestClass]
    public class Core_UnitTest1
    {
        public Core_UnitTest1()
        {
            PayFactory.Enable(new PayConfig(), new PayResultListener(), null, false);
        }
        [TestMethod]
        public void WeiXinScanQRCode_Test()
        {
            var pay = PayFactory.CreatePay(PayInterfaceType.WeiXinScanQRCode);
            var parameter = new PayParameter()
            {
                Amount = 0.01,
                Description = "测试交易",
                TradeID = Guid.NewGuid().ToString("N"),
                TradeName = "myTradeName"
            };
            try
            {
                pay.BeginPay(parameter);
                pay.CheckPayState(parameter);

            }
            catch (PayServerReportException)
            {

            }
            catch
            {
                throw;
            }
        }
        [TestMethod]
        public void WeiXinTransfers_Test()
        {
            var pay = PayFactory.CreatePay(PayInterfaceType.WeiXinTransfers);
            var parameter = new PayParameter()
            {
                Description = "测试",
                Amount = 1,//微信规定最低1元
                TradeID = Guid.NewGuid().ToString("N"),
                AuthCode = "olqA5wtuH62E8dgqSnudCCZlIc3g",//谭泳在小恒欢乐送的openid oZX1SwBVE3Ee8KOrQRPfYlhk2OFc
            };
            try
            {
                pay.BeginPay(parameter);
                pay.CheckPayState(parameter);
            }
            catch (PayServerReportException)
            {

            }
            catch
            {
                throw;
            }
        }

        [TestMethod]
        public void WeiXinJSApi_Test()
        {
            var pay = PayFactory.CreatePay(PayInterfaceType.WeiXinJSApi);
            var parameter = new PayParameter()
            {
                Amount = 0.01,
                Description = "测试交易",
                TradeID = Guid.NewGuid().ToString("N"),
                TradeName = "myTradeName",
                AuthCode = "olqA5wtuH62E8dgqSnudCCZlIc3g",//openid
                ReturnUrl = "http://caiyun.xododo.net/home/weixinBack"
            };
            try
            {
                var url = pay.BeginPay(parameter);

                /*
                 js调用：
                 function jsApiCall()
                   {
                       WeixinJSBridge.invoke(
                       'getBrandWCPayRequest',
                       <%=wxJsApiParam%>,//josn串
                        function (res)
                        {
                            WeixinJSBridge.log(res.err_msg);
                            alert(res.err_code + res.err_desc + res.err_msg);
                         }
                        );
                   }
                 */
            }
            catch (PayServerReportException)
            {

            }
            catch
            {
                throw;
            }
        }

        [TestMethod]
        public void WeiXinBarcode_Test()
        {
            var pay = PayFactory.CreatePay(PayInterfaceType.WeiXinBarcode);
            var parameter = new PayParameter()
            {
                Amount = 0.01,
                Description = "测试交易",
                TradeID = Guid.NewGuid().ToString("N"),
                TradeName = "myTradeName",
                AuthCode = "135119430980976036",//客户出示的二维码
                Timeout = 30,
            };
            try
            {
                pay.BeginPay(parameter);
                pay.CheckPayState(parameter);
            }
            catch (PayServerReportException)
            {

            }
            catch
            {
                throw;
            }

        }

        [TestMethod]
        public void WeiXinH5_Test()
        {
            var pay = PayFactory.CreatePay(PayInterfaceType.WeiXinH5);
            var parameter = new PayParameter()
            {
                Amount = 0.01,
                Description = "测试交易",
                TradeID = Guid.NewGuid().ToString("N"),
                TradeName = "myTradeName",
                Timeout = 30,
                AuthCode = "20.168.3.8",//客户端ip
            };
            try
            {
                var url = pay.BeginPay(parameter);
                pay.CheckPayState(parameter);
            }
            catch (PayServerReportException)
            {

            }
            catch
            {
                throw;
            }

        }
        [TestMethod]
        public void AlipayRefund_Test()
        {
            var pay = PayFactory.CreatePay(PayInterfaceType.AlipayBarcode);
            var result = pay.Refund(new RefundParameter()
            {
                Amount = 0.01,
                TradeID = "5a0d7698e94f475c9e4113fc9dea9b4b",
            });
        }
        [TestMethod]
        public void WeixinRefund_Test()
        {
            try
            {
                var pay = PayFactory.CreatePay(PayInterfaceType.WeiXinBarcode);
                var result = pay.Refund(new RefundParameter()
                {
                    Amount = 0.01,
                    TradeID = "842040ed09434b36aeb4fc475ed636fd",
                });
            }
            catch(PayServerReportException)
            {

            }
            catch
            {
                throw;
            }
        }
        [TestMethod]
        public void AlipayBarcode_Test()
        {

            var pay = PayFactory.CreatePay(PayInterfaceType.AlipayBarcode);
            var parameter = new PayParameter()
            {
                Amount = 0.01,
                Description = "测试交易",
                TradeID = Guid.NewGuid().ToString("N"),
                TradeName = "myTradeName",
                AuthCode = "283996484850531686",//客户出示的二维码
                Timeout = 30,
                StoreId = "897",
            };

            parameter.GoodsDetails.Add(new GoodsDetail()
            {
                GoodsId = "P001",
                GoodsName = "测试商品01",
                Quantity = 1,
                Price = 0.01
            });
            try
            {

                pay.BeginPay(parameter);
                pay.CheckPayState(parameter);
            }
            catch (PayServerReportException)
            {

            }
            catch
            {
                throw;
            }
        }

        [TestMethod]
        public void AlipayScanQRCode_Test()
        {

            var pay = PayFactory.CreatePay(PayInterfaceType.AlipayScanQRCode);
            var parameter = new PayParameter()
            {
                Amount = 0.01,
                Description = "测试交易",
                TradeID = Guid.NewGuid().ToString("N"),
                TradeName = "myTradeName",
                Timeout = 30,
            };
            try
            {
                var code = pay.BeginPay(parameter);
                pay.CheckPayState(parameter);
            }
            catch (PayServerReportException)
            {

            }
            catch
            {
                throw;
            }

        }
        [TestMethod]
        public void AlipayWap_Test()
        {

            var pay = PayFactory.CreatePay(PayInterfaceType.AlipayWap);
            var parameter = new PayParameter()
            {
                Amount = 0.01,
                Description = "测试交易",
                TradeID = Guid.NewGuid().ToString("N"),
                TradeName = "myTradeName",
                Timeout = 30,
            };
            try
            {
                var code = pay.BeginPay(parameter);
                pay.CheckPayState(parameter);
            }
            catch (PayServerReportException)
            {

            }
            catch
            {
                throw;
            }

        }

        [TestMethod]
        public void AlipayWeb_Test()
        {

            var pay = PayFactory.CreatePay(PayInterfaceType.AlipayWeb);
            var parameter = new PayParameter()
            {
                Amount = 0.01,
                Description = "测试交易",
                TradeID = Guid.NewGuid().ToString("N"),
                TradeName = "myTradeName",
                Timeout = 30,
            };
            try
            {
                var code = pay.BeginPay(parameter);
                pay.CheckPayState(parameter);
            }
            catch (PayServerReportException)
            {

            }
            catch
            {
                throw;
            }

        }

        [TestMethod]
        public void IPaysoon_WeiXinBarcode_Test()
        {
            var pay = PayFactory.CreatePay(PayInterfaceType.IPaysoonWeiXinBarcode);
            var parameter = new PayParameter()
            {
                Amount = 0.01,
                Description = "测试交易",
                TradeID = Guid.NewGuid().ToString("N"),
                TradeName = "myTradeName",
                AuthCode = "134548413943541106",//客户出示的二维码
                Timeout = 30,
            };
            try
            {
                pay.BeginPay(parameter);
                //pay.CheckPayState(parameter);
            }
            catch (PayServerReportException)
            {

            }
            catch
            {
                throw;
            }

        }
        [TestMethod]
        public void IPaysoon_WeiXinQuery_Test()
        {
            var pay = PayFactory.CreatePay(PayInterfaceType.IPaysoonWeiXinBarcode);
            var parameter = new PayParameter()
            {
                TradeID = "37665d7fd41a4cb5b381922358495f91",
            };
            try
            {
                pay.CheckPayState(parameter);
            }
            catch (PayServerReportException)
            {

            }
            catch
            {
                throw;
            }

        }
        [TestMethod]
        public void IPaysoon_AlipayScanQRCode_Test()
        {
            var pay = PayFactory.CreatePay(PayInterfaceType.IPaysoonAlipayScanQRCode);
            var parameter = new PayParameter()
            {
                Amount = 0.01,
                Description = "测试交易",
                TradeID = Guid.NewGuid().ToString("N"),
                TradeName = "myTradeName",
                Timeout = 30,
            };
            try
            {
                var url = pay.BeginPay(parameter);
                //pay.CheckPayState(parameter);
            }
            catch (PayServerReportException)
            {

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        [TestMethod]
        public void IPaysoon_WeiXinScanQRCode_Test()
        {
            var pay = PayFactory.CreatePay(PayInterfaceType.IPaysoonWeiXinScanQRCode);
            var parameter = new PayParameter()
            {
                Amount = 0.01,
                Description = "测试交易",
                TradeID = Guid.NewGuid().ToString("N"),
                TradeName = "myTradeName",
                Timeout = 30,
            };
            try
            {
                var url = pay.BeginPay(parameter);
                //pay.CheckPayState(parameter);
            }
            catch (PayServerReportException)
            {

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        [TestMethod]
        public void IPaysoon_AlipayBarcode_Test()
        {
            var pay = PayFactory.CreatePay(PayInterfaceType.IPaysoonAlipayBarcode);
            var parameter = new PayParameter()
            {
                Amount = 0.03,
                Description = "测试交易",
                TradeID = Guid.NewGuid().ToString("N"),
                TradeName = "myTradeName",
                AuthCode = "283829134783524951" +
                "",//客户出示的二维码
                Timeout = 30,
            };
            parameter.GoodsDetails.Add(new GoodsDetail()
            {
                GoodsId = "P002",
                GoodsName = "测试商品A1",
                Price = 0.03,
                Quantity = 1
            });
            try
            {
                pay.BeginPay(parameter);
                //pay.CheckPayState(parameter);
            }
            catch (PayServerReportException)
            {

            }
            catch
            {
                throw;
            }

        }
        [TestMethod]
        public void IPaysoon_AlipayQuery_Test()
        {
            var pay = PayFactory.CreatePay(PayInterfaceType.IPaysoonAlipayBarcode);
            var parameter = new PayParameter()
            {
                TradeID = "657e4025b8de41f7974924cadc12740f",
            };
            try
            {
                pay.CheckPayState(parameter);
            }
            catch (PayServerReportException)
            {

            }
            catch
            {
                throw;
            }

        }

        [TestMethod]
        public void IPaysoon_WeiXinJSApi_Test()
        {
            var pay = PayFactory.CreatePay(PayInterfaceType.IPaysoonWeiXinJSApi);
            var parameter = new PayParameter()
            {
                Amount = 0.01,
                Description = "测试交易",
                TradeID = Guid.NewGuid().ToString("N"),
                TradeName = "myTradeName",
                AuthCode = "oysxe0Y95B27OyL1b1OLOuRAXyks",//openid
                NotifyDomain = "http://tan.xododo.com",
                ReturnUrl = "http://test"
            };
            try
            {
                var url = pay.BeginPay(parameter);
            }
            catch (PayServerReportException)
            {

            }
            catch
            {
                throw;
            }
        }

       
        /// <summary>
        /// 获取支付传媒的门店编号
        /// </summary>
        [TestMethod]
        public void Bboqi_RegisterShop()
        {
            //这是我们自己的门店编号
            var myshopcodes = @"
19904
XTN001
TJN001
TJN002
BJN001
BJN002
BJN003
BJN004
BJN005
BJN006
BJN007
BJN008
BJN009
BJN010
BJN011
BJN012
BJN013
BJN014
BJN015
BJN016
BJN017
BJN018
BJN019
BJN020
BJN021
BJN022
BJN023
BJN024
BJN025
BJN026
BJN027
BJN028
BJN029
BJN030
BJN031
BJN032
BJN033
BJN034
BJN035
BJN036

".Split('\n');
            myshopcodes = (from m in myshopcodes where m.Trim().Length > 0 select m.Trim()).ToArray();
            StringBuilder result_SHOP_CODE = new StringBuilder();
            StringBuilder result_CASH_ID = new StringBuilder();
            StringBuilder result_CASH_NAME = new StringBuilder();
            foreach (var mycode in myshopcodes)
            {
                var jsonStr = ((Newtonsoft.Json.Linq.JArray)Newtonsoft.Json.JsonConvert.DeserializeObject(Jack.Pay.Impls.Bboqi.Bboqi_Helper.RegisterShop(mycode)))[0];
                result_SHOP_CODE.AppendLine(jsonStr.Value<string>("SHOP_CODE"));
                result_CASH_ID.AppendLine(jsonStr.Value<string>("CASH_ID"));
                result_CASH_NAME.AppendLine(jsonStr.Value<string>("CASH_NAME"));
            }

        }

        [TestMethod]
        public void Bboqi_WeixinBarcode()
        {
            var pay = PayFactory.CreatePay(PayInterfaceType.BboqiWeiXinBarcode);
            var parameter = new PayParameter()
            {
                Amount = 0.01,
                Description = "测试交易",
                TradeID = Guid.NewGuid().ToString("N"),
                TradeName = "myTradeName",
                AuthCode = "135138355822429035",//客户出示的二维码
                Timeout = 30,
            };
            try
            {
                pay.BeginPay(parameter);
                Thread.Sleep(10000);
            }
            catch (PayServerReportException)
            {

            }
            catch
            {
                throw;
            }
        }

        [TestMethod]
        public void Bboqi_CheckPayState()
        {
            var pay = PayFactory.CreatePay(PayInterfaceType.BboqiWeiXinBarcode);
            var parameter = new PayParameter()
            {
                Amount = 0.01,
                Description = "测试交易",
                TradeID = "4fbc6cbcb0be4600802d566564ebc443",
                TradeName = "myTradeName",
                AuthCode = "134580297422568560",//客户出示的二维码
                Timeout = 30,
            };
            try
            {
                pay.CheckPayState(parameter);
            }
            catch (PayServerReportException)
            {

            }
            catch
            {
                throw;
            }
        }



        [TestMethod]
        public void Bboqi_WeiXinJSApi_Test()
        {
            var pay = PayFactory.CreatePay(PayInterfaceType.BboqiWeiXinJSApi);
            var parameter = new PayParameter()
            {
                Amount = 0.01,
                Description = "测试交易",
                TradeID = Guid.NewGuid().ToString("N"),
                TradeName = "myTradeName",
                AuthCode = "oysxe0Y95B27OyL1b1OLOuRAXyks",//openid
            };
            try
            {
                var wxJsApiParam = pay.BeginPay(parameter);
                Thread.Sleep(10000);
                /*
                 js调用：
                 function jsApiCall()
                   {
                       WeixinJSBridge.invoke(
                       'getBrandWCPayRequest',
                       <%=wxJsApiParam%>,//josn串
                        function (res)
                        {
                            WeixinJSBridge.log(res.err_msg);
                            alert(res.err_code + res.err_desc + res.err_msg);
                         }
                        );
                   }
                 */
            }
            catch (PayServerReportException)
            {

            }
            catch
            {
                throw;
            }
        }
        
        [TestMethod]
        public void Cailutong_BTC()
        {
            var pay = PayFactory.CreatePay(PayInterfaceType.CailutongBTC);
            var parameter = new PayParameter()
            {
                Amount = 0.0001,
                Description = "测试交易",
                TradeID = Guid.NewGuid().ToString("N"),
                TradeName = "myTradeName",
            };
            try
            {
                pay.BeginPay(parameter);
                pay.CheckPayState(parameter);
            }
            catch (PayServerReportException)
            {

            }
            catch
            {
                throw;
            }
        }


        [TestMethod]
        public void LianTuo_AlipayBarcode()
        {
            var pay = PayFactory.CreatePay(PayInterfaceType.LianTuoAlipayBarcode);
            var parameter = new PayParameter()
            {
                Amount = 0.01,
                Description = "测试交易",
                TradeID = Guid.NewGuid().ToString("N"),
                TradeName = "测试交易",
                AuthCode = "287716238670913661",//客户出示的二维码
                Timeout = 15,
            };
            try
            {
                pay.BeginPay(parameter);
                Thread.Sleep(10000);
            }
            catch (PayServerReportException)
            {

            }
            catch
            {
                throw;
            }
        }

        [TestMethod]
        public void LianTuo_WeixinBarcode()
        {
            var pay = PayFactory.CreatePay(PayInterfaceType.LianTuoWeixinBarcode);
            var parameter = new PayParameter()
            {
                Amount = 0.01,
                Description = "测试交易",
                TradeID = Guid.NewGuid().ToString("N"),
                TradeName = "myTradeName",
                AuthCode = "134731850825699894",//客户出示的二维码
                Timeout = 15,
            };
            try
            {
                pay.BeginPay(parameter);
                Thread.Sleep(10000);
            }
            catch (PayServerReportException)
            {

            }
            catch
            {
                throw;
            }
        }
        [TestMethod]
        public void LianTuo_WeixinJSApi()
        {
            var pay = PayFactory.CreatePay(PayInterfaceType.LianTuoWeixinJSApi);
            var parameter = new PayParameter()
            {
                Amount = 0.01,
                Description = "测试交易",
                TradeID = Guid.NewGuid().ToString("N"),
                TradeName = "myTradeName",
                AuthCode = "oLNmRjrZe5hqTd0eXMvZQTUBjR94",//open id
                Timeout = 15,
            };
            try
            {
                pay.BeginPay(parameter);
                Thread.Sleep(10000);
            }
            catch (PayServerReportException)
            {

            }
            catch
            {
                throw;
            }
        }
        [TestMethod]
        public void LianTuo_Refund()
        {
            var pay = PayFactory.CreatePay(PayInterfaceType.LianTuoAlipayBarcode);
            
            var parameter = new RefundParameter()
            {
                Amount = 0.01,
                TradeID = "6f4066943146464eb7a8720d139bbfee",
                Reason = "买错了",
            };
            try
            {
                var result = pay.Refund(parameter);
            }
            catch (PayServerReportException)
            {

            }
            catch
            {
                throw;
            }
        }
    }
    class PayResultListener : IPayResultListener
    {

        public void OnLog(string tradeID, LogEventType eventType, string message)
        {
            
        }

        public void OnPayFailed(string tradeID, string reason, string message)
        {
           
        }

        public void OnPaySuccessed(string tradeID, double? receiptAmount,double? discountAmount, string message)
        {
            
        }
    }
    class PayConfig : IPayConfig
    {
        public string GetInterfaceXmlConfig(PayInterfaceType interfacetype, string tradeID)
        {
            if( interfacetype.HasFlag(PayInterfaceType.WeiXin) )
                return System.IO.File.ReadAllText(AppContext.BaseDirectory  + "/weixin.xml" , System.Text.Encoding.UTF8);
            else if (interfacetype.HasFlag(PayInterfaceType.Alipay))
                return System.IO.File.ReadAllText(AppContext.BaseDirectory + "/alipay.xml", System.Text.Encoding.UTF8);
            else if (interfacetype.HasFlag(PayInterfaceType.IPaysoon))
                return System.IO.File.ReadAllText(AppContext.BaseDirectory + "/ipaysoon.xml", System.Text.Encoding.UTF8);
            else if (interfacetype.HasFlag(PayInterfaceType.Bboqi))
                return System.IO.File.ReadAllText(AppContext.BaseDirectory + "/bboqi.xml", System.Text.Encoding.UTF8);
            else if (interfacetype.HasFlag(PayInterfaceType.CailutongGateway))
                return System.IO.File.ReadAllText(AppContext.BaseDirectory + "/cailutong.xml", System.Text.Encoding.UTF8);
            else if (interfacetype.HasFlag(PayInterfaceType.LianTuo))
                return System.IO.File.ReadAllText(AppContext.BaseDirectory + "/liantuo.xml", System.Text.Encoding.UTF8);
            return null;
        }
    }
}
