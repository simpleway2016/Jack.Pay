﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jack.Pay
{
    /// <summary>
    /// 支付接口
    /// </summary>
    public interface IPay
    {
        /// <summary>
        /// 发起支付
        /// </summary>
        /// <param name="parameter">支付参数</param>
        /// <returns>可能是null，可能是生成提交付款的Html代码，可能是二维码字符，具体由接口决定</returns>
        string BeginPay(PayParameter parameter);


        /// <summary>
        /// 根据TradeID检查支付状态，如果有结果，会从监听器返回
        /// </summary>
        /// <param name="parameter">相关参数，只需要TradeID</param>
        /// <returns>true：表示已经有结果，可能支付成功，可能支付失败，false表示还不明确结果</returns>
        bool CheckPayState(PayParameter parameter);
        //void CheckPayState(PayParameter parameter);


        /// <summary>
        /// 退款
        /// </summary>
        /// <param name="parameter">退款参数，TradeID必须设置</param>
        /// <returns></returns>
        RefundResult Refund(RefundParameter parameter);
    }
}
