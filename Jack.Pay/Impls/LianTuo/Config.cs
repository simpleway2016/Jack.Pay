using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Reflection;
namespace Jack.Pay.Impls.LianTuo
{
    class Config:BaseConfig
    {
        public string merchant_id;
        public string key;
        public string partner_id;
        public string core_merchant_no;
        /// <summary>
        /// 与联拓商户绑定的微信appid，获取openid时，也应该是此appid
        /// </summary>
        public string weixin_appid;
        public Config(string xml) : base(xml)
        {           

            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(partner_id) || string.IsNullOrEmpty(core_merchant_no) || string.IsNullOrEmpty(merchant_id) || string.IsNullOrEmpty(weixin_appid))
                throw new Exception("联拓支付接口的xml配置不正确");
        }
    }
}
