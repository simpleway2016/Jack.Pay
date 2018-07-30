using System;
using System.Collections.Generic;
using System.Text;

namespace Jack.Pay
{
    public class PayServerReportException : Exception
    {
        public PayServerReportException(string msg):base(msg)
        { }
    }
}
