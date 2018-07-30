using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Reflection;
namespace Jack.Pay.Impls.IPaysoon
{
    class Config:BaseConfig
    {
        public string merchantCode;
        public string operatorCode;
        public string platformPublicKey;
        public string merchantPrivateKey;
        public Config(string xml) : base(xml)
        {

            if (string.IsNullOrEmpty(merchantCode) || string.IsNullOrEmpty(operatorCode) || string.IsNullOrEmpty(merchantPrivateKey))
                throw new Exception("马上支付的xml配置不正确");

            //if (!System.IO.File.Exists(this.merchantPrivateKey))
            //{
            //    this.merchantPrivateKey = Helper.GetApplicationPath() + this.merchantPrivateKey;
            //}
            //this.merchantPrivateKey = System.IO.File.ReadAllText(this.merchantPrivateKey, Encoding.UTF8);
            //this.merchantPrivateKey = this.merchantPrivateKey.Substring("-----BEGIN PRIVATE KEY-----".Length);
            //this.merchantPrivateKey = this.merchantPrivateKey.Substring(0, this.merchantPrivateKey.IndexOf("-----END ")).Trim().Replace("\n", "").Replace("\r", "");

            //if (!System.IO.File.Exists(this.merchantPublicKey))
            //{
            //    this.merchantPublicKey = Helper.GetApplicationPath() + this.merchantPublicKey;
            //}
            //this.merchantPublicKey = System.IO.File.ReadAllText(this.merchantPublicKey, Encoding.UTF8);
            //this.merchantPublicKey = this.merchantPublicKey.Substring("-----BEGIN PUBLIC KEY-----".Length);
            //this.merchantPublicKey = this.merchantPublicKey.Substring(0, this.merchantPublicKey.IndexOf("-----END ")).Trim().Replace("\n", "").Replace("\r", "");

            //if (!System.IO.File.Exists(this.platformPublicKey))
            //{
            //    this.platformPublicKey = Helper.GetApplicationPath() + this.platformPublicKey;
            //}
            //this.platformPublicKey = System.IO.File.ReadAllText(this.platformPublicKey, Encoding.UTF8);
            //this.platformPublicKey = this.platformPublicKey.Substring("-----BEGIN PUBLIC KEY-----".Length);
            //this.platformPublicKey = this.platformPublicKey.Substring(0, this.platformPublicKey.IndexOf("-----END ")).Trim().Replace("\n", "").Replace("\r", "");
        }
    }
}
