using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jack.Pay
{
    public enum PayInterfaceType 
    {
        /// <summary>
        /// 支付宝接口
        /// </summary>
        Alipay = 1,
        /// <summary>
        /// 微信接口
        /// </summary>
        WeiXin = 1<<1,
        /// <summary>
        /// 马上支付
        /// </summary>
        IPaysoon = 1<<2,
        /// <summary>
        /// 支付传媒
        /// </summary>
        Bboqi = 1 << 3,

        /// <summary>
        /// 财路通支付网关
        /// </summary>
        CailutongGateway = 1 << 5,
        /// <summary>
        /// 联拓支付网关
        /// </summary>
        LianTuo = 1<<6,



        /// <summary>
        /// 支付宝 - 条码（刷卡）支付
        /// </summary>
        AlipayBarcode = Alipay | (1<<16),
        /// <summary>
        /// 支付宝 - 扫码支付（客户扫商家）
        /// </summary>
        AlipayScanQRCode = Alipay | (1 << 17),
        /// <summary>
        /// 支付宝 - 手机网站支付
        /// </summary>
        AlipayWap = Alipay | (1 << 18),
        /// <summary>
        /// 支付宝 - PC网站支付
        /// </summary>
        AlipayWeb = Alipay | (1 << 19),

        /// <summary>
        /// 微信 - 条码（刷卡）支付
        /// </summary>
        WeiXinBarcode = WeiXin | (1 << 16),
        /// <summary>
        /// 微信 - 扫码支付（客户扫商家）
        /// </summary>
        WeiXinScanQRCode = WeiXin | (1 << 17),
        /// <summary>
        /// 微信 - 企业支付给其他微信账号
        /// </summary>
        WeiXinTransfers = WeiXin | (1 << 18),
        /// <summary>
        /// 微信 - JSApi支付
        /// 微信支付的发起目录为网站的根目录，请在微信支付管理后台做相关设置
        /// </summary>
        WeiXinJSApi = WeiXin | (1 << 19),
        /// <summary>
        /// 微信 - H5支付
        /// 微信支付的发起目录为网站的根目录，请在微信支付管理后台做相关设置
        /// </summary>
        WeiXinH5 = WeiXin | (1 << 20),

        /// <summary>
        /// 马上支付 - 微信条码（刷卡）支付
        /// </summary>
        IPaysoonWeiXinBarcode = IPaysoon | (1 << 16),
        /// <summary>
        /// 马上支付 - 微信扫码支付（客户扫商家）
        /// </summary>
        IPaysoonWeiXinScanQRCode = IPaysoon | (1 << 17),
        /// <summary>
        /// 马上支付 - 微信公众号支付
        /// </summary>
        IPaysoonWeiXinJSApi = IPaysoon | (1 << 18),
        /// <summary>
        /// 马上支付 - 支付宝条码（刷卡）支付
        /// </summary>
        IPaysoonAlipayBarcode = IPaysoon | (1 << 19),
        /// <summary>
        /// 马上支付 - 支付宝扫码支付（客户扫商家）
        /// </summary>
        IPaysoonAlipayScanQRCode = IPaysoon | (1 << 20),

        /// 支付传媒 - 微信条码（刷卡）支付
        /// </summary>
        BboqiWeiXinBarcode = Bboqi | (1 << 16),
        /// 支付传媒 - 微信公众号支付
        /// 微信支付的发起目录为网站的根目录
        /// </summary>
        BboqiWeiXinJSApi = Bboqi | (1 << 17),

       

        /// <summary>
        /// BTC支付
        /// </summary>
        CailutongBTC = CailutongGateway | (1 << 16),

        /// <summary>
        /// 联拓 - 支付宝条码（刷卡）支付
        /// </summary>
        LianTuoAlipayBarcode = LianTuo | (1 << 16),
        /// <summary>
        /// 联拓 - 微信条码（刷卡）支付
        /// </summary>
        LianTuoWeixinBarcode = LianTuo | (1 << 17),
        /// <summary>
        /// 联拓 - 微信公众号支付
        /// </summary>
        LianTuoWeixinJSApi = LianTuo | (1 << 18),
    }
}
