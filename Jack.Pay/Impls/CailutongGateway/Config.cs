using System;
using System.Collections.Generic;
using System.Text;

namespace Jack.Pay.Impls.CailutongGateway
{
    class Config : BaseConfig
    {
        public string Account;
        public string Secret;

        public Config(string xml) : base(xml)
        {

            if (string.IsNullOrEmpty(Account) || string.IsNullOrEmpty(Secret))
                throw new Exception("网关不正确");
        }
    }
}
