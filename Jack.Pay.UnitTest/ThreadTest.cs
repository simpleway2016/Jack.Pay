using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
namespace Jack.Pay.UnitTest
{
    [TestClass]
    public class ThreadTest
    {
        [TestMethod]
        public void Test()
        {
            ObjectItem[] items = new ObjectItem[10000];
            for(int i = 0; i < items.Length; i ++)
            {
                   items[i] = new ObjectItem();
            }

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            ParallelLoopResult result = Parallel.For(0 , items.Length , (i)=>{
                start(i + 1, items);
            });

            stopWatch.Stop();

            items = items.OrderBy(m => m.Owner).ToArray();
            for (int i = 0; i < items.Length; i++)
            {
                if(items[i].Owner - i != 1)
                {
                    throw new Exception("结果错误");
                }
            }

            var elapsedTime = stopWatch.ElapsedMilliseconds;
        }

        void start(int yourId, ObjectItem[] items)
        {
            for (int j = 0; j < items.Length; j++)
            {
                int originalValue = Interlocked.CompareExchange(ref items[j].Owner, yourId, 0);
                if (originalValue == 0)
                {
                    //证明成功占用这个item
                    return;
                }
                else
                {
                    //失败了，别人已经认领，所以还原这个值
                    items[j].Owner = originalValue;
                }
            }
        }
    }

    class ObjectItem
    {
        public int Owner;
        public override string ToString()
        {
            return Owner.ToString();
        }
    }
}
