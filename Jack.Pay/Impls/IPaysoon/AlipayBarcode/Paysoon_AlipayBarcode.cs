using System;
using System.Collections.Generic;
using System.Text;

namespace Jack.Pay.Impls.IPaysoon
{
    [PayInterface(PayInterfaceType.IPaysoonAlipayBarcode)]
    class Paysoon_AlipayBarcode : Paysoon_WeiXinBarcode
    {
        
#if TESTING
        protected override string ServerUrl => "http://210.14.139.198:8080/Czech/alipays/micropay";
        protected override string QueryUrl => "http://210.14.139.198:8080/Czech/alipay/query";
#else
        protected override string ServerUrl => "http://gateway.ipaysoon.com/MSZFPlatform/alipays/micropay";
        protected override string QueryUrl => "http://gateway.ipaysoon.com/MSZFPlatform/alipay/query";
#endif
        protected override string BusinessCode => "ALIPAYF2FT1";

    }
}
