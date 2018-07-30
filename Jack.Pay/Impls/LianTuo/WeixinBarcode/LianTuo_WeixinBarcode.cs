using System;
using System.Collections.Generic;
using System.Text;

namespace Jack.Pay.Impls.LianTuo.WeixinBarcode
{
    [PayInterface(PayInterfaceType.LianTuoWeixinBarcode)]
    class LianTuo_WeixinBarcode : LianTuo.AlipayBarcode.LianTuo_AlipayBarcode
    {
        protected override string ChannelType => "WX";
    }
}
