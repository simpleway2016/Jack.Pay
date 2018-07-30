using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Jack.Pay.Impls.IPaysoon
{
    [PayInterface(PayInterfaceType.IPaysoonAlipayScanQRCode)]
    class Paysoon_AlipayScanQRCode : Paysoon_WeixinScanQRCode
    {
        protected override string ServerUrl => "http://gateway.ipaysoon.com/MSZFPlatform/alipays/qrpay";
        protected override string BusinessCode => "ALIPAYF2FT1";
        protected override string QueryUrl => "http://gateway.ipaysoon.com/MSZFPlatform/alipay/query";

    }
}
