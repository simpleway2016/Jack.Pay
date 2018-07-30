using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace Jack.Pay
{
    /// <summary>
    /// 商品信息
    /// </summary>
    public class GoodsDetail
    {
        /// <summary>
        /// 商品的编号
        /// </summary>
        public string GoodsId;
        /// <summary>
        /// 商品的名称
        /// </summary>
        public string GoodsName;
        /// <summary>
        /// 数量
        /// </summary>
        public int Quantity = 0;
        /// <summary>
        /// 单价
        /// </summary>
        public double Price = 0;
    }
}