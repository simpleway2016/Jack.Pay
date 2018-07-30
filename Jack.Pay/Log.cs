﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Jack.Pay
{
    class Log : Way.Lib.CLog
    {
        public override string SavePath
        {
            get
            {
                string path = null;
#if NET46
            try
            {
                if (!string.IsNullOrEmpty(System.Web.HttpRuntime.BinDirectory))
                {
                    var bin = System.Web.HttpRuntime.BinDirectory;
                    while (bin.EndsWith("\\"))
                        bin = bin.Substring(0, bin.Length - 1);
                    bin = System.IO.Path.GetDirectoryName(bin);
                    path = bin + "\\App_Data\\Log\\Jack.Pay";
                }
                else
                {
                    path = Way.Lib.PlatformHelper.GetAppDirectory() + "Log\\Jack.Pay";
                }
            }
            catch
            {
                path = Way.Lib.PlatformHelper.GetAppDirectory() + "Log\\Jack.Pay";
            }
#else
                path = Way.Lib.PlatformHelper.GetAppDirectory() + "Log\\Jack.Pay";
#endif
                if (Directory.Exists(path) == false)
                    Directory.CreateDirectory(path);
                return path;
            }
        }
        public Log(string name):base(name)
        {

        }
        public Log(string name , bool autoDispose):base(name , autoDispose)
        {

        }
    }
}
