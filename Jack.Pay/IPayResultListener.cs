using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jack.Pay
{
    public enum LogEventType
    {
        /// <summary>
        /// 支付接口通知支付结果的信息
        /// </summary>
        ReceiveNotify = 1,
        /// <summary>
        /// 发起支付后，支付接口返回的信息
        /// </summary>
        ReceivePayResult = 2,
    }
    /// <summary>
    /// 监听付款结果
    /// </summary>
    public interface IPayResultListener
    {
        /// <summary>
        /// 交易成功触发
        /// </summary>
        /// <param name="tradeID">交易编号</param>
        /// <param name="receiptAmount">商家实际到账金额，未知将会是null</param>
        /// <param name="discountAmount">优惠金额，未知将会是null，
        /// “优惠金额+商家实际到账金额” 不一定等于 “订单金额”，因为优惠金额不一定都是商家承担，可能支付宝会承担一部分
        /// </param>
        /// <param name="message">官方返回的信息</param>
        void OnPaySuccessed(string tradeID,double? receiptAmount ,double? discountAmount,string message);

        /// <summary>
        /// 交易失败触发
        /// </summary>
        /// <param name="tradeID">交易编号</param>
        /// <param name="reason">失败原因</param>
        /// <param name="message">官方返回的信息</param>
        void OnPayFailed(string tradeID,string reason , string message);



        /// <summary>
        /// 捕获支付接口返回的信息
        /// </summary>
        /// <param name="tradeID"></param>
        /// <param name="eventType"></param>
        /// <param name="message"></param>
        void OnLog(string tradeID,LogEventType eventType, string message);
    }
}
