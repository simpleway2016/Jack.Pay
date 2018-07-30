using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jack.Pay
{
    /// <summary>
    /// 发起支付设置的参数
    /// </summary>
    public class PayParameter
    {
        string _NotifyDomain;
        /// <summary>
        /// 可自定义notify的域名，如 http://www.host.com
        /// </summary>
        public string NotifyDomain
        {
            get
            {
                return _NotifyDomain;
            }
            set
            {
                if (_NotifyDomain != value)
                {
                    if (value.EndsWith("/"))
                        _NotifyDomain = value.Substring(0, value.Length - 1);
                    else
                        _NotifyDomain = value;
                }
            }
        }
        /// <summary>
        /// 交易编号
        /// </summary>
        public string TradeID { get; set; }

        string _TradeName;
        /// <summary>
        /// 订单名称
        /// </summary>
        public string TradeName
        {
            get
            {
                return _TradeName ?? "";
            }
            set
            {
                _TradeName = value;
                if(_Description == null)
                {
                    _Description = value;
                }
            }
        }


        string _Description;
        /// <summary>
        /// 交易描述
        /// </summary>
        public string Description {
            get
            {
                return _Description ?? "";
            }
            set
            {
                _Description = value;
                if (_TradeName == null)
                {
                    _TradeName = value;
                }
            }
        }

        /// <summary>
        /// 交易金额
        /// </summary>
        public double Amount { get; set; }

        /// <summary>
        /// 失效时间
        /// </summary>
        public DateTime? ExpireTime { get; set; }

        /// <summary>
        /// 刷卡支付场景：表示客户出示的二维码内容；
        /// 微信企业付款场景：表示支付目标用户的openid；
        /// 微信H5支付场景，表示客户端的ip
        /// </summary>
        public string AuthCode { get; set; }

        /// <summary>
        /// 支付方法执行时，最长的等待时间，单位：秒，默认180秒
        /// </summary>
        public int Timeout { get; set; }

        /// <summary>
        /// 向支付服务器发起request请求时的timeout设置，单位：秒，默认20秒
        /// </summary>
        public int RequestTimeout { get; set; }

        /// <summary>
        /// 商品详情，如支付宝口碑支付，需要单品打折时，需要传递
        /// </summary>
        public List<GoodsDetail> GoodsDetails { get; private set; }
        /// <summary>
        /// 商品编号，如支付宝口碑支付，需要商铺整单打折时，需要传递
        /// 在口碑里，这个是表示外部门店编号，不是门店id
        /// </summary>
        public string StoreId { get; set; }

        /// <summary>
        /// 对于网页类型的支付接口，这里可以配置return url
        /// </summary>
        public string ReturnUrl { get; set; }
       
        public PayParameter()
        {
            this.Timeout = 180;
            this.RequestTimeout = 20;
            this.GoodsDetails = new List<GoodsDetail>();
            var url = PayFactory.CurrentDomainUrl;
            if (string.IsNullOrEmpty(url) == false)
            {
                this.NotifyDomain = url;
            }
        }
    }
}
