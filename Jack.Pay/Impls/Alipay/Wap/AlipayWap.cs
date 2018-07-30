using System;
using System.Collections.Generic;
using System.Text;
using Jack.Pay.Impls.Alipay;
using System.Security.Cryptography;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Jack.Pay.Impls
{
    [PayInterface(PayInterfaceType.AlipayWap)]
    class AlipayWap : AlipayWeb
    {
        protected override string Method => "alipay.trade.wap.pay";
    }
}
