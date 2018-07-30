using System;
using System.Collections.Generic;
using System.Text;

namespace Jack.Pay
{
    public class Bank
    {
        /// <summary>
        /// 银行名称
        /// </summary>
        public string Name { get; set; }
        public string Id { get; set; }
    }
    /// <summary>
    /// 银行分行
    /// </summary>
    public class BankBranch
    {
        /// <summary>
        /// 分行名称
        /// </summary>
        public string BranchName { get; set; }
        /// <summary>
        /// 分行id
        /// </summary>
        public string BranchId { get; set; }
    }
}
