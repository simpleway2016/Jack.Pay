//using System;
//using Microsoft.VisualStudio.TestTools.UnitTesting;

//namespace Jack.Pay.NetFramework.UnitTest
//{
//    [TestClass]
//    public class NetUnitTest1
//    {
//        public NetUnitTest1()
//        {
//            PayFactory.Enable(new PayConfig(), new PayResultListener(),false);
//        }
//        [TestMethod]
//        public void WeiXinScanQRCode_Test()
//        {
//            var d = System.Web.HttpRuntime.BinDirectory;
//            var pay = PayFactory.CreatePay(PayInterfaceType.WeiXinScanQRCode);
//            var parameter = new PayParameter()
//            {
//                Amount = 0.01,
//                Description = "测试交易",
//                TradeID = Guid.NewGuid().ToString("N"),
//                TradeName = "myTradeName"
//            };
//            try
//            {
//                pay.BeginPay(parameter);
//                pay.CheckPayState(parameter);

//            }
//            catch (PayServerReportException)
//            {

//            }
//            catch
//            {
//                throw;
//            }
//        }
//        [TestMethod]
//        public void WeiXinTransfers_Test()
//        {
//            var pay = PayFactory.CreatePay(PayInterfaceType.WeiXinTransfers);
//            var parameter = new PayParameter()
//            {
//                Description = "测试",
//                Amount = 1,//微信规定最低1元
//                TradeID = Guid.NewGuid().ToString("N"),
//                AuthCode = "oZX1SwBlhk2OFc",//谭泳在小恒欢乐送的openid oZX1SwBVE3Ee8KOrQRPfYlhk2OFc
//            };
//            try
//            {
//                pay.BeginPay(parameter);
//                pay.CheckPayState(parameter);
//            }
//            catch (PayServerReportException)
//            {

//            }
//            catch
//            {
//                throw;
//            }
//        }

//        [TestMethod]
//        public void WeiXinH5_Test()
//        {
//            var pay = PayFactory.CreatePay(PayInterfaceType.WeiXinH5);
//            var parameter = new PayParameter()
//            {
//                Amount = 0.01,
//                Description = "测试交易",
//                TradeID = Guid.NewGuid().ToString("N"),
//                TradeName = "myTradeName",
//                Timeout = 30,
//                AuthCode = "20.168.3.8",//客户端ip
//            };
//            try
//            {
//                var url = pay.BeginPay(parameter);
//                pay.CheckPayState(parameter);
//            }
//            catch (PayServerReportException)
//            {

//            }
//            catch
//            {
//                throw;
//            }

//        }
//        [TestMethod]
//        public void WeiXinJSApi_Test()
//        {
//            var pay = PayFactory.CreatePay(PayInterfaceType.WeiXinJSApi);
//            var parameter = new PayParameter()
//            {
//                Amount = 0.01,
//                Description = "测试交易",
//                TradeID = Guid.NewGuid().ToString("N"),
//                TradeName = "myTradeName",
//                AuthCode = "oZX1SwBVE3Ee8KOrQRPfYlhk2OFc",//openid
//            };
//            try
//            {
//                var wxJsApiParam = pay.BeginPay(parameter);

//                /*
//                 js调用：
//                 function jsApiCall()
//                   {
//                       WeixinJSBridge.invoke(
//                       'getBrandWCPayRequest',
//                       <%=wxJsApiParam%>,//josn串
//                        function (res)
//                        {
//                            WeixinJSBridge.log(res.err_msg);
//                            alert(res.err_code + res.err_desc + res.err_msg);
//                         }
//                        );
//                   }
//                 */
//            }
//            catch (PayServerReportException)
//            {

//            }
//            catch
//            {
//                throw;
//            }
//        }

//        [TestMethod]
//        public void WeiXinBarcode_Test()
//        {
//            var pay = PayFactory.CreatePay(PayInterfaceType.WeiXinBarcode);
//            var parameter = new PayParameter()
//            {
//                Amount = 0.01,
//                Description = "测试交易",
//                TradeID = Guid.NewGuid().ToString("N"),
//                TradeName = "myTradeName",
//                AuthCode = "1301219918289956",//客户出示的二维码
//                Timeout = 30,
//            };
//            try
//            {
//                pay.BeginPay(parameter);
//                pay.CheckPayState(parameter);
//            }
//            catch (PayServerReportException)
//            {

//            }
//            catch
//            {
//                throw;
//            }

//        }


//        [TestMethod]
//        public void AlipayRefund_Test()
//        {
//            var pay = PayFactory.CreatePay(PayInterfaceType.AlipayBarcode);
//            var result = pay.Refund(new RefundParameter()
//            {
//                Amount = 0.01,
//                TradeID = "5a0d7698e94f475c9e4113fc9dea9b4b",
//            });
//        }
//        [TestMethod]
//        public void WeixinRefund_Test()
//        {
//            var pay = PayFactory.CreatePay(PayInterfaceType.WeiXinBarcode);
//            var result = pay.Refund(new RefundParameter()
//            {
//                Amount = 0.01,
//                TradeID = "842040ed09434b36aeb4fc475ed636fd",
//            });
//        }
//        [TestMethod]
//        public void AlipayBarcode_Test()
//        {

//            var pay = PayFactory.CreatePay(PayInterfaceType.AlipayBarcode);
//            var parameter = new PayParameter()
//            {
//                Amount = 0.01,
//                Description = "测试交易",
//                TradeID = Guid.NewGuid().ToString("N"),
//                TradeName = "myTradeName",
//                AuthCode = "283442244306586262",//客户出示的二维码
//                Timeout = 30,
//            };
//            try
//            {

//                pay.BeginPay(parameter);
//                pay.CheckPayState(parameter);
//            }
//            catch (PayServerReportException)
//            {

//            }
//            catch
//            {
//                throw;
//            }
//        }

//        [TestMethod]
//        public void AlipayScanQRCode_Test()
//        {

//            var pay = PayFactory.CreatePay(PayInterfaceType.AlipayScanQRCode);
//            var parameter = new PayParameter()
//            {
//                Amount = 0.01,
//                Description = "测试交易",
//                TradeID = Guid.NewGuid().ToString("N"),
//                TradeName = "myTradeName",
//                Timeout = 30,
//            };
//            try
//            {
//                var code = pay.BeginPay(parameter);
//                pay.CheckPayState(parameter);
//            }
//            catch (PayServerReportException)
//            {

//            }
//            catch
//            {
//                throw;
//            }

//        }
//        [TestMethod]
//        public void AlipayWap_Test()
//        {

//            var pay = PayFactory.CreatePay(PayInterfaceType.AlipayWap);
//            var parameter = new PayParameter()
//            {
//                Amount = 0.01,
//                Description = "测试交易",
//                TradeID = Guid.NewGuid().ToString("N"),
//                TradeName = "myTradeName",
//                Timeout = 30,
//            };
//            try
//            {
//                var code = pay.BeginPay(parameter);
//                pay.CheckPayState(parameter);
//            }
//            catch (PayServerReportException)
//            {

//            }
//            catch
//            {
//                throw;
//            }

//        }

//        [TestMethod]
//        public void AlipayWeb_Test()
//        {

//            var pay = PayFactory.CreatePay(PayInterfaceType.AlipayWeb);
//            var parameter = new PayParameter()
//            {
//                Amount = 0.01,
//                Description = "测试交易",
//                TradeID = Guid.NewGuid().ToString("N"),
//                TradeName = "myTradeName",
//                Timeout = 30,
//            };
//            try
//            {
//                var code = pay.BeginPay(parameter);
//                pay.CheckPayState(parameter);
//            }
//            catch (PayServerReportException)
//            {

//            }
//            catch
//            {
//                throw;
//            }

//        }
//    }
//    class PayResultListener : IPayResultListener
//    {

//        public void OnLog(string tradeID, LogEventType eventType, string message)
//        {

//        }

//        public void OnPayFailed(string tradeID, string reason, string message)
//        {

//        }


//        public void OnPaySuccessed(string tradeID, double? receiptAmount, double? discountAmount, string message)
//        {
//        }
//    }
//    class PayConfig : IPayConfig
//    {
//        public string GetInterfaceXmlConfig(PayInterfaceType interfacetype, string tradeID)
//        {
//            if (interfacetype.HasFlag(PayInterfaceType.WeiXin))
//                return System.IO.File.ReadAllText(AppContext.BaseDirectory + "/weixin.xml", System.Text.Encoding.UTF8);
//            else if (interfacetype.HasFlag(PayInterfaceType.Alipay))
//                return System.IO.File.ReadAllText(AppContext.BaseDirectory + "/alipay.xml", System.Text.Encoding.UTF8);
//            return null;
//        }
//    }
//}
