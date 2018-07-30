using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Jack.Pay;
using System.Net.WebSockets;
using System.Threading;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace MvcTest
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddSession();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            Jack.Pay.PayFactory.Enable(new PayConfig(),new PayResultListener() , app, false);
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseWebSockets();
            app.UseStaticFiles();
            app.UseSession();
            app.Use(async (httpcontext, next) =>
            {
                if (httpcontext.WebSockets.IsWebSocketRequest)
                {
                    var socket =await httpcontext.WebSockets.AcceptWebSocketAsync();

                    await websocketHandle(socket, httpcontext.Request);
                }
                else
                {
                    if (next != null)
                    {
                        await next();
                    }
                    else
                    {
                        await Task.CompletedTask;
                    }
                }                
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        async Task websocketHandle(System.Net.WebSockets.WebSocket socket,HttpRequest request)
        {
            var bs = new byte[204800];
            while (true)
            {
                if (socket.State == System.Net.WebSockets.WebSocketState.Open)
                {
                    try
                    {
                        ArraySegment<byte> buffer = new ArraySegment<byte>(bs);
                        WebSocketReceiveResult result = await socket.ReceiveAsync(buffer, CancellationToken.None);
                        if (result.Count == 0)
                        {
                            //客户端要求关闭
                            socket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None).Wait();
                            continue;
                        }
                        StringBuilder str = new StringBuilder();
                        foreach (var item in request.Headers)
                        {
                            foreach (var v in item.Value)
                            {
                                str.Append($"{item.Key}={v}\r\n");
                            }
                        }
                        byte[] sendbackBs = System.Text.Encoding.UTF8.GetBytes(str.ToString());
                        
                        buffer = new ArraySegment<byte>(sendbackBs);
                        await socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                    catch
                    {
                    }
                }
                else
                {
                    break;
                }
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
            using (Way.Lib.CLog log = new Way.Lib.CLog("支付失败"))
            {
                log.Log("tradeId:{0} reason:{1} message:{2}", tradeID, reason, message);
            }
        }

        public void OnPaySuccessed(string tradeID, double? receiptAmount,double? discountAmount, string message)
        {
            using (Way.Lib.CLog log = new Way.Lib.CLog("支付成功"))
            {
                log.Log(message);
                log.Log("tradeId:{0} receiptAmount:{1} discountAmount:{2} message:{3}",tradeID , receiptAmount,discountAmount , message);
            }
        }
    }
    class PayConfig : IPayConfig
    {
        public string GetInterfaceXmlConfig(PayInterfaceType interfacetype, string tradeID)
        {
            if (interfacetype.HasFlag(PayInterfaceType.WeiXin))
                return System.IO.File.ReadAllText(AppContext.BaseDirectory + "/weixin.xml", System.Text.Encoding.UTF8);
            else if (interfacetype.HasFlag(PayInterfaceType.Alipay))
                return System.IO.File.ReadAllText(AppContext.BaseDirectory + "/alipay.xml", System.Text.Encoding.UTF8);
            else if (interfacetype.HasFlag(PayInterfaceType.IPaysoon))
                return System.IO.File.ReadAllText(AppContext.BaseDirectory + "/ipaysoon.xml", System.Text.Encoding.UTF8);
            else if (interfacetype.HasFlag(PayInterfaceType.Bboqi))
                return System.IO.File.ReadAllText(AppContext.BaseDirectory + "/bboqi.xml", System.Text.Encoding.UTF8);
            else if (interfacetype.HasFlag(PayInterfaceType.Meituan))
                return System.IO.File.ReadAllText(AppContext.BaseDirectory + "/meituan.xml", System.Text.Encoding.UTF8);
            else if (interfacetype.HasFlag(PayInterfaceType.LianTuo))
                return System.IO.File.ReadAllText(AppContext.BaseDirectory + "/liantuo.xml", System.Text.Encoding.UTF8);
            return null;
        }
    }
}
