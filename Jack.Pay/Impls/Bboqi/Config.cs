using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Reflection;
namespace Jack.Pay.Impls.Bboqi
{
    class Config:BaseConfig
    {
        public string AppId;
        public string Key;
        public string ShopId;
        public string CashId;
        public string CashDesc;
        public Config(string xml) : base(xml)
        {           

            if (string.IsNullOrEmpty(AppId) || string.IsNullOrEmpty(Key) || string.IsNullOrEmpty(ShopId))
                throw new Exception("支付传媒的xml配置不正确");
        }
    }
}
