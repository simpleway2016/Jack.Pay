
mvc的Startup里面的Configure，需要加入这句：


Jack.Pay.PayFactory.Enable(new PayConfig() , new PayResultListener() , false );

//自己写两个类，配置账号信息，和接收支付结果
 class PayResultListener : IPayResultListener
    {
        public void OnLog(string tradeID, string message)
        {

        }

        public void OnPayFailed(string tradeID, string reason, string message)
        {

        }

        public void OnPaySuccessed(string tradeID, string message)
        {

        }
    }
    class PayConfig : IPayConfig
    {
        /// <summary>
        /// 获取支付接口账户信息
        /// </summary>
        /// <param name="interfacetype"></param>
        /// <param name="tradeID"></param>
        /// <returns></returns>
        public string GetInterfaceXmlConfig(PayInterfaceType interfacetype, string tradeID)
        {
			//返回美团支付的xml配置信息
            return @"<?xml version=""1.0"" encoding=""utf-8""?>
<r>
  <AppId>31165</AppId>
  <Key>a0be4bd57707487f829a14251bf96aab</Key>
<MerchantId>166214062</MerchantId>
<SignType>SHA-256</SignType>
</r>";
        }
    }



发起支付代码例子：

 var pay = PayFactory.CreatePay(PayInterfaceType.WeiXinScanQRCode);
            var parameter = new PayParameter()
            {
                Amount = 0.01,
                Description = "测试交易",
                TradeID = Guid.NewGuid().ToString("N"),
                TradeName = "myTradeName"
            };
            //发起支付，得到二维码内容
            var qrCode = pay.BeginPay(parameter);
			//把qrCode转成二维码图片，显示给用户

mvc controller发起支付宝网页支付：
        public IActionResult AlipayWeb()
        {
            var pay = PayFactory.CreatePay(PayInterfaceType.AlipayWeb);
            var parameter = new PayParameter()
            {
                ReturnUrl = $"http://{Request.Host.ToString()}/home/PayReturn",
                Amount = 0.01,
                Description = "测试交易",
                TradeID = Guid.NewGuid().ToString("N"),
                TradeName = "myTradeName",
                Timeout = 180,
            };
            var code = pay.BeginPay(parameter);
            return Content(code, "text/html");
        }

微信H5支付(需要在微信后台配置正确发起域名)：
		 public static string GetUserIp(Microsoft.AspNetCore.Http.HttpContext context)
        {
            var ip = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            var X_Real_IP = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (string.IsNullOrEmpty(ip))
            {
                ip = X_Real_IP;
            }
            if (string.IsNullOrEmpty(ip))
            {
                ip = context.Connection.RemoteIpAddress.ToString();
            }
            return ip;
        }

        public IActionResult WeixinH5()
        {
            var pay = PayFactory.CreatePay(PayInterfaceType.WeiXinH5);
            var parameter = new PayParameter()
            {
                Amount = 0.01,
                Description = "测试交易",
                TradeID = Guid.NewGuid().ToString("N"),
                TradeName = "myTradeName",
                Timeout = 180,
                AuthCode = GetUserIp(Request.HttpContext),
            };
            var url = pay.BeginPay(parameter);
            return Content("<script>location.href='"+url+"';</script>", "text/html");
        }


配置参数格式示例：

支付宝：
<?xml version="1.0" encoding="utf-8"?>
<alipayConfig>
  <appId>20160910477416</appId>
  <alipay_public_key>6FTFY99uhpiqTcZ32oWpwIDAQAB</alipay_public_key>
  <merchant_private_key>mWZEpYxg/QD/Qx2gcX6W4Ua3Co8B1/iUEA==</merchant_private_key>
  <merchant_public_key>8PcifJb6MdJ0w/K+FZJMXmWZEpYxg/QD/Qx2gcGtCza/xElcHR3s2z</merchant_public_key>
</alipayConfig>


微信：
<?xml version="1.0" encoding="utf-8"?>
<wechatPayConfig>
  <AppID>wx07896444</AppID>
  <AppSecret>cb2bed3626c8f147b67</AppSecret>
  <MchID>14042502</MchID>
  <Key>shanghuzuzhifumiy</Key>
  <SSLCERT_PATH>d:\apiclient_cert.p12</SSLCERT_PATH>
<SSLCERT_PASSWORD>14042502</SSLCERT_PASSWORD>
</wechatPayConfig>

支付传媒：
{"AppId":"bqwjrpird","Key":"w9jFr8p6i7r7d2bCr","ShopId":"bQET86U6q3gq","CashId":"371","CashDesc":"1号收银台"}

马上支付：
<?xml version="1.0" encoding="utf-8"?>                    
<r>                      
	<merchantCode>3970466559</merchantCode>             
        <operatorCode>00002620779224</operatorCode>
	<platformPublicKey>MIGfMA</platformPublicKey>
  <merchantPrivateKey>MII9pD02tgk+IT8D1dGo2yNo2uFa1BL8j3JQ==</merchantPrivateKey>     
<merchantPublicKey>MIGfMA0GCAGXyngkLZ3Qds1IChwIHR/67dQIDAQAB</merchantPublicKey>
</r>

证书路径可以是apiclient_cert.p12，表示在和应用程序相同目录下