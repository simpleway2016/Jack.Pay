using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
#if NET46
#else
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
#endif
using Way.Lib;
using System.Web;
using System.Threading;

namespace Jack.Pay
{
   /// <summary>
   ///  
   /// </summary>
    public class PayFactory
    {
        static bool UseHttps;
        static IPayConfig _Config;
        static Dictionary<PayInterfaceType, Type> _PayInterfaceDefineds;
        static List<IPayResultListener> _PayResultListeners = new List<IPayResultListener>();

        internal static bool SettedRequestHandlers = false;
        /// <summary>
        /// 记录每个支付接口对应的枚举类型
        /// </summary>
        static Dictionary<PayInterfaceType, Type> PayInterfaceDefineds
        {
            get {
                if(_PayInterfaceDefineds == null)
                {
                    _PayInterfaceDefineds = new Dictionary<PayInterfaceType, Type>();
                    //循环当前程序集的所有Type，取出所有接口类型
                    Type ipayType = typeof(IPay);
                    Type[] types = ipayType.GetTypeInfo().Assembly.GetTypes();
                    foreach( Type type in types )
                    {
                        //判断这个类是否实现了IPay接口
                        if ( type.GetTypeInfo().GetInterface(ipayType.FullName) != null )
                        {
                            PayInterfaceAttribute myAtt = type.GetTypeInfo().GetCustomAttribute<PayInterfaceAttribute>();
                            if (myAtt != null)
                            {
                                _PayInterfaceDefineds.Add(myAtt.InterfaceType, type);
                            }
                        }
                    }
                }
                return _PayInterfaceDefineds;
            }
        }

        
        /// <summary>
        /// 注册支付结果监听器
        /// </summary>
        /// <param name="listener"></param>
        public static void RegisterResultListener(IPayResultListener listener)
        {
            _PayResultListeners.Add(listener);
        }



#if NET46
        internal static string CurrentDomainUrl
        {
            get
            {
                string url;
                var request = System.Web.HttpContext.Current.Request;
                  var scheme = UseHttps ? "https" : "http";

                url = $"{scheme}://{ request.Headers["Host"]}";
                return url;
            }
        }

        /// <summary>
        /// 开启支付功能，如果是网站应用，需要在static Global 静态构造函数中调用
        /// </summary>
        /// <param name="config"></param>
        /// <param name="listener"></param>
        /// <param name="useHttps">运行的站点是否采用https</param>
        public static void Enable(IPayConfig config, IPayResultListener listener ,bool useHttps)
        {
            _Config = config;
            if (listener != null)
            {
                RegisterResultListener(listener);
            }
            UseHttps = useHttps;
            if (!SettedRequestHandlers)
            {
                SettedRequestHandlers = true;

                try
                {
                    using (Log log = new Log("Jack.Pay.Enable"))
                    {
                        log.Log("Begin AddRequestHandlers");
                  
                        Jack.HttpRequestHandlers.Manager.AddRequestHandlers(typeof(PayFactory).Assembly);
                        log.Log("End AddRequestHandlers");
                    }
                }
                catch (Exception ex)
                {
                    using (Log log = new Log("AddRequestHandlers error"))
                    {
                        log.Log(ex.ToString());
                    }
                }
            }
        }
        
#else
        static IServiceProvider _ServiceProvider;
        internal static string CurrentDomainUrl
        {
            get
            {
                if (_ServiceProvider == null)
                    return null;
                string url;
                var accessor = (Microsoft.AspNetCore.Http.IHttpContextAccessor)_ServiceProvider.GetService(typeof(Microsoft.AspNetCore.Http.IHttpContextAccessor));
                var scheme = UseHttps ? "https" : "http";

                url = $"{scheme}://{accessor.HttpContext.Request.Headers["Host"]}";
                return url;
            }
        }
        /// <summary>
        /// 开启支付功能，如果是mvc core，需要在Configure里面调用此方法
        /// </summary>
        /// <param name="config"></param>
        /// <param name="listener"></param>
        /// <param name="mvcApp">如果设为null，表示支付接口的服务器不会主动通知支付结果，Jack.Pay会在后台自动轮询获取支付结果</param>
        /// <param name="useHttps">运行的站点是否采用https</param>
        public static void Enable(IPayConfig config, IPayResultListener listener , IApplicationBuilder mvcApp,bool useHttps)
        {
          
            if (mvcApp != null)
            {
                _ServiceProvider = mvcApp.ApplicationServices;
            }
            UseHttps = useHttps;
            _Config = config;
            if(listener != null)
            {
                RegisterResultListener(listener);
            }
            if (!SettedRequestHandlers)
            {
                SettedRequestHandlers = true;
                if(mvcApp != null)
                {
                    try
                    {
                        Jack.HttpRequestHandlers.Manager.AddRequestHandlers(mvcApp, typeof(PayFactory).Assembly);

                    }
                    catch (Exception ex)
                    {
                        using (CLog logErr = new CLog("PayFactory.Enable error "))
                        {
                            logErr.Log(ex.ToString());
                        }
                    }
                }                               
            }
        }
#endif

        /// <summary>
        /// 根据支付类型，创建支付接口
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IPay CreatePay(PayInterfaceType type )
        {
            if(SettedRequestHandlers == false)
            {
                throw new Exception("请先执行PayFactory.Enable()");
            }
            if (_PayResultListeners.Count == 0)
            {
                throw new Exception("必须先添加一个PayResultListener");
            }

            var objType = PayInterfaceDefineds[type];
            if(objType != null)
            {
                return (IPay)Activator.CreateInstance(objType);
            }
            return null;
        }

        /// <summary>
        /// 触发所有监听器的OnPayFailed
        /// </summary>
        /// <param name="tradeID">交易编号</param>
        /// <param name="reason">失败原因</param>
        /// <param name="message">官方返回的信息</param>
        internal static void OnPayFailed(string tradeID, string reason, string message)
        {
            new Thread(()=> {
                foreach (IPayResultListener listener in _PayResultListeners)
                {
                    try
                    {
                        listener.OnPayFailed(tradeID, reason, message);
                    }
                    catch
                    {

                    }

                }
            }).Start();            
        }

        /// <summary>
        /// 触发所有监听器的OnPaySuccessed
        /// </summary>
        /// <param name="tradeID">交易编号</param>
        /// <param name="receiptAmount">实收金额</param>
        /// <param name="discountAmount">优惠金额</param>
        /// <param name="message">官方返回的信息</param>
        internal static void OnPaySuccessed(string tradeID,double? receiptAmount,double? discountAmount, string message)
        {
            new Thread(()=> {
                foreach (IPayResultListener listener in _PayResultListeners)
                {
                    try
                    {
                        listener.OnPaySuccessed(tradeID, receiptAmount, discountAmount, message);
                    }
                    catch
                    {

                    }
                }
            }).Start();
            
        }

        /// <summary>
        /// 获取Config对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="payInterfaceType"></param>
        /// <param name="tradeId"></param>
        /// <returns></returns>
        internal static T GetConfig<T>(Type payInterfaceType , string tradeId)
        {
            var attrs = payInterfaceType.GetCustomAttributes(typeof(PayInterfaceAttribute), false);
            //获取当前接口类型
            var myInterfaceType = ((PayInterfaceAttribute)attrs[0]).InterfaceType;

            //获取xml配置信息，并创建Config对象
            return (T)Activator.CreateInstance(typeof(T), new object[] { PayFactory.GetInterfaceXmlConfig(myInterfaceType, tradeId) });
        }

        internal static void OnLog(string tradeID,LogEventType eventType, string message)
        {
            foreach (IPayResultListener listener in _PayResultListeners)
            {
                try
                {
                    listener.OnLog(tradeID, eventType, message);
                }
                catch
                {

                }

            }
        }
        /// <summary>
        /// 获取支付接口的xml配置信息
        /// </summary>
        /// <param name="interfacetype">接口类型</param>
        /// <param name="tradeID">交易编号</param>
        /// <returns>返回xml配置信息</returns>
        internal static string GetInterfaceXmlConfig(PayInterfaceType interfacetype, string tradeID)
        {
            if (_Config == null)
                throw new Exception("请先调用PayFactory.Enable配置支付环境");

             return _Config.GetInterfaceXmlConfig(interfacetype, tradeID);

        }
    }

}
