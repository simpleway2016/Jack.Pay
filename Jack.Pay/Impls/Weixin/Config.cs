using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Reflection;
namespace Jack.Pay.Impls.Weixin
{
    class Config:BaseConfig
    {
        public string AppID;
        public string AppSecret;
        public string MchID;
        public string Key;
        public string SSLCERT_PATH;
        public string SSLCERT_PASSWORD;
        public Config(string xml):base(xml)
        {            

            if(!System.IO.File.Exists(this.SSLCERT_PATH))
            {
                this.SSLCERT_PATH = Helper.GetApplicationPath() + SSLCERT_PATH;
            }

            if (string.IsNullOrEmpty(AppID) || string.IsNullOrEmpty(AppSecret) || string.IsNullOrEmpty(MchID))
                throw new Exception("微信支付接口的xml配置不正确");
        }
    }
}
