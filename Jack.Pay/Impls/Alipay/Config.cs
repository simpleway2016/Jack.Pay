using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Reflection;
namespace Jack.Pay.Impls.Alipay
{
    class Config:BaseConfig
    {
        public string appid;
        //public string pid;
        public string alipayPublicKey;
        public string merchantPrivateKey;
        public string merchantPublicKey;
        public Config(string xml) : base(xml)
        {

            if (string.IsNullOrEmpty(appid) || string.IsNullOrEmpty(merchantPrivateKey) || string.IsNullOrEmpty(alipayPublicKey))
                throw new Exception("支付宝的xml配置不正确");
        }
    }
}
