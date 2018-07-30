using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Jack.Pay;
using ZXing.Common;
using ZXing;
using System.IO;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Drawing;
using System.Text;
using Way.Lib;

namespace MvcTest.Controllers
{
    public class HomeController : Controller
    {

        public static Bitmap toBitmap(BitMatrix matrix)
        {
            int width = matrix.Width;
            int height = matrix.Height;
            Bitmap bmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    bmap.SetPixel(x, y, matrix[x, y] ? Color.Black : Color.White);
                }
            }
            return bmap;
        }

        public object TestJson()
        {
            return new
            {
                name = "hell"
            };
        }

        public string weixinBack(string payStatus, string TradeID, string payErrorMsg)
        {
            return "支付结果:" + payStatus + "，交易id：" + TradeID + " 信息：" + payErrorMsg;
        }

        public string getOpenId(string openid, string access_token, string err)
        {
            if (err != null)
                return err;
            if (openid != null)
            {
                return "openid:" + openid + " userinfo:" + Jack.Pay.WeixinAuth.Auth2.GetUserInfo(access_token, openid);
            }
            //wx9e0e20859501fefe
            //var url =  Jack.Pay.WeixinAuth.Auth2.GetOpenId("wx56cf81b9b930da28", "d89f6153de773b7a6c4e36ee5c3b0afb", Jack.Pay.WeixinAuth.Auth2.Scope.snsapi_userinfo, "http://caiyun.xododo.net/home/getOpenId?te=1");
            var url = Jack.Pay.WeixinAuth.Auth2.GetOpenId("wx9e0e20859501fefe", "3912cee6afcac5fb75852e27ff7144db", Jack.Pay.WeixinAuth.Auth2.Scope.snsapi_userinfo, "http://caiyun.xododo.net/home/getOpenId?te=1");
            Response.Redirect(url);
            return null;
        }

       
        public void test()
        {
            var request = Request;
            var pay = PayFactory.CreatePay(PayInterfaceType.LianTuoWeixinJSApi);
            var parameter = new PayParameter()
            {
                Amount = 0.01,
                Description = "测试交易",
                TradeID = Guid.NewGuid().ToString("N"),
                TradeName = "myTradeName",
                AuthCode = "oXenWt_-ULS1e5Mafl8pYpw1Y5j8",
                ReturnUrl = "http://caiyun.xododo.net/home/weixinBack"
            };
            Response.Redirect(pay.BeginPay(parameter));
        }

        public void alipayWeb()
        {
            var pay = PayFactory.CreatePay(PayInterfaceType.AlipayWeb);
            var parameter = new PayParameter()
            {
                Amount = 0.01,
                Description = "测试交易",
                TradeID = Guid.NewGuid().ToString("N"),
                TradeName = "myTradeName",
                ReturnUrl = "http://caiyun.xododo.net/home/weixinBack",
            };

            var url = pay.BeginPay(parameter);
            Response.Redirect(url);
        }

        public void weixinTest()
        {
            var pay = PayFactory.CreatePay(PayInterfaceType.WeiXinJSApi);
            var parameter = new PayParameter()
            {
                Amount = 0.01,
                Description = "测试交易",
                TradeID = Guid.NewGuid().ToString("N"),
                TradeName = "myTradeName",
                AuthCode = "oZX1SwBVE3Ee8KOrQRPfYlhk2OFc",//openid
                ReturnUrl = "http://caiyun.xododo.net/home/weixinBack",
            };

            var url = pay.BeginPay(parameter);
            Response.Redirect(url);
        }
        public void bboqi_weixinTest()
        {
            var pay = PayFactory.CreatePay(PayInterfaceType.BboqiWeiXinJSApi);
            var parameter = new PayParameter()
            {
                Amount = 0.01,
                Description = "测试交易",
                TradeID = Guid.NewGuid().ToString("N"),
                TradeName = "myTradeName",
                AuthCode = "oysxe0Y95B27OyL1b1OLOuRAXyks",//openid
                ReturnUrl = "http://caiyun.xododo.net/home/weixinBack",
            };

            var url = pay.BeginPay(parameter);
            Response.Redirect(url);
        }
        public IActionResult Index()
        {
            //var pay = PayFactory.CreatePay(PayInterfaceType.WeiXinJSApi);
            //var parameter = new PayParameter()
            //{
            //    Amount = 0.01,
            //    Description = "测试交易",
            //    TradeID = Guid.NewGuid().ToString("N"),
            //    TradeName = "myTradeName",
            //    AuthCode = "oZX1SwBVE3Ee8KOrQRPfYlhk2OFc",//openid
            //};
            //ViewBag.payJson = pay.BeginPay(parameter);

            return View();
        }

        public object StoreCb()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            foreach (var key in Request.Query)
            {
                dict[key.Key] = key.Value;
            }
            Response.ContentType = "application/json";
            return new { data = "success" };
        }

        public IActionResult QRCode()
        {

            var pay = PayFactory.CreatePay(PayInterfaceType.IPaysoonAlipayScanQRCode);
            var parameter = new PayParameter()
            {
                Amount = 0.02,
                Description = "测试交易",
                TradeID = Guid.NewGuid().ToString("N"),
                TradeName = "myTradeName"
            };
            //发起支付，得到二维码
            var qrCode = pay.BeginPay(parameter);

            BitMatrix byteMatrix = new MultiFormatWriter().encode(qrCode, BarcodeFormat.QR_CODE, 300, 300);
            System.Drawing.Bitmap bitmap = toBitmap(byteMatrix);

            MemoryStream ms = new MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            bitmap.Dispose();
            return File(ms.ToArray(), "image/png");

        }
        public IActionResult ip()
        {
            StringBuilder str = new StringBuilder();
            foreach (var item in Request.Headers)
            {
                foreach (var v in item.Value)
                {
                    str.Append($"{item.Key}={v}");
                }
            }
            return Content(Request.HttpContext.Connection.RemoteIpAddress.ToString() + "<br>" + str);
        }
        public void AlipayWap()
        {
            var pay = PayFactory.CreatePay(PayInterfaceType.AlipayWap);
            var parameter = new PayParameter()
            {
                Amount = 0.01,
                Description = "测试交易",
                TradeID = Guid.NewGuid().ToString("N"),
                TradeName = "myTradeName",
                ReturnUrl = "http://caiyun.xododo.net/home/weixinBack",
            };

            var url = pay.BeginPay(parameter);
            Response.Redirect(url);
        }

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
            return Content("<script>location.href='" + url + "';</script>", "text/html");
        }


        public IActionResult PayReturn(string TradeID)
        {
            return Content($"TradeID:{TradeID} return!");
        }
        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
