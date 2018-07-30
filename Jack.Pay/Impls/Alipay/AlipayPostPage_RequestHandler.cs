using System;
using System.Collections.Generic;
using System.Text;
using Jack.HttpRequestHandlers;

namespace Jack.Pay.Impls.Alipay
{
    class AlipayPostPage_RequestHandler : Jack.HttpRequestHandlers.IRequestHandler
    {
        public const string PageName = "JACK_Pay_AlipayPostPage";
        public string UrlPageName => PageName;

        public TaskStatus Handle(IHttpProxy httpProxy)
        {
            var tranId = httpProxy.QueryString["tranId"];

            //读取临时文件，还原PayParameter参数
            string tempFile = $"{Helper.GetSaveFilePath()}\\{tranId}.txt";
            var body = System.IO.File.ReadAllText(tempFile, Encoding.UTF8);

            var html = Helper.ReadContentFromResourceStream("Jack.Pay.Impls.AlipayPayPage.html");
            html = html.Replace("$body$", body);

            httpProxy.ResponseWrite(html);

            return TaskStatus.Completed;
        }
    }
}
