using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jack.Pay
{
    /// <summary>
    /// 退款提交的参数
    /// </summary>
    public class RefundParameter
    {
        /// <summary>
        /// 交易编号
        /// </summary>
        public string TradeID { get; set; }
        /// <summary>
        /// 退款原因
        /// </summary>
        public string Reason { get; set; }

        double _TotalAmount = 0;
        /// <summary>
        /// 订单总金额
        /// </summary>
        public double TotalAmount
        {
            get
            {
                return _TotalAmount;
            }
            set
            {
                _TotalAmount = value;
                if (_Amount == 0)
                    _Amount = value;
            }
        }


        double _Amount = 0;
        /// <summary>
        /// 退款金额
        /// </summary>
        public double Amount
        {
            get
            {
                return _Amount;
            }
            set
            {
                _Amount = value;
                if (_TotalAmount == 0)
                    _TotalAmount = value;
            }
        }
    }
}
