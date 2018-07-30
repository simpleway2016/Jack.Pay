using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Jack.Pay.Impls
{
    abstract class BasePay : IPay
    {
        public abstract string BeginPay(PayParameter parameter);

        public abstract bool CheckPayState(PayParameter parameter);

        public abstract RefundResult Refund(RefundParameter parameter);

        /// <summary>
        /// 循环查询支付状态
        /// </summary>
        /// <param name="parameter"></param>
        internal virtual void CheckPayStateInLoop(PayParameter parameter)
        {
            DateTime startTime = DateTime.Now;
            while (true)
            {
                Thread.Sleep(1000);
                try
                {
                    if ((DateTime.Now - startTime).TotalSeconds > parameter.Timeout || this.CheckPayState(parameter))
                    {
                        return;
                    }
                }
                catch (Exception ex)
                {
                    using (Log log = new Log("CheckPayStateInLoop error"))
                    {
                        log.Log(this.GetType().FullName);
                        log.Log(ex.ToString());
                    }
                }
            }
        }
    }
}
