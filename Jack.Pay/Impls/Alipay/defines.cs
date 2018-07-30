using System;
using System.Collections.Generic;
using System.Text;

namespace Jack.Pay.Impls.Alipay
{
    //下面是根据支付宝返回的json结构，定义的类，主要用于把json字符串转为对象
    internal class AlipayTradePayResult
    {
        public class AlipayTradePayResponse
        {
            public string code;
            public string sub_msg;
            public string msg;
            public string buyer_logon_id;
            public string buyer_pay_amount;
            public string buyer_user_id;
            public DateTime gmt_payment;
            public double invoice_amount;
            public string open_id;
            public string out_trade_no;
            public double point_amount;
            public double? receipt_amount;
            public double total_amount;
            public string trade_no;
            public string discount_goods_detail;
        }

        public class Discount_goods_detail
        {
            public string goods_id;
            public string goods_name;
            public double discount_amount;
            public string voucher_id;
            public string goods_num;
        }

        public AlipayTradePayResponse alipay_trade_pay_response;
        public string sign;
    }

    internal class AlipayTradeQueryResult
    {
        public class AlipayTradeQueryResponse
        {
            public string code;
            public string msg;
            public string sub_code;
            public string sub_msg;
            public string trade_status;
            public double? receipt_amount;
        }

        public AlipayTradeQueryResponse alipay_trade_query_response;
        public string sign;
    }

    internal class AlipayTradePrecreateResult
    {
        public class AlipayTradePrecreateResponse
        {
            public string code;
            public string msg;
            public string out_trade_no;
            public string qr_code;
            public string sub_code;
            public string sub_msg;
        }

        public AlipayTradePrecreateResponse alipay_trade_precreate_response;
        public string sign;
    }

    internal class AlipayTradeRefundResult
    {
        public class AlipayTradeRefundResponse
        {
            public string code;
            public string msg;
            public string sub_code;
            public string sub_msg;
        }

        public AlipayTradeRefundResponse alipay_trade_refund_response;
        public string sign;
    }
}
