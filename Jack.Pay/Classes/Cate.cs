using System;
using System.Collections.Generic;
using System.Text;

namespace Jack.Pay
{
    /// <summary>
    /// 品类
    /// </summary>
    public class Cate
    {
        public string Name;
        public string Id;
        public string ParentId;
        public bool IsLeaf;
        public List<Cate> SubCates = new List<Cate>();
    }
}
