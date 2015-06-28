using ApCommLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Stomp
{
    [ApService("ApService 测试类","1.0.0.1")]
    public class ApServer:ApService
    {
        [ApServiceMethod("等待执行")]
        public bool DoWait()
        {
            Logger.DebugFormat("{0} {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ffffff"), "DoWait");
            return true;
        }

        [ApServiceMethod("睡眠")]
        public void DoSomething()
        {
            Logger.DebugFormat("{0} {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ffffff"), "DoSomething");
            Thread.Sleep(10000);
        }

        [ApServiceMethod("请求")]
        public string Request(
            [ApServiceMethodParameter("请求信息")]string msg)
        {
            Logger.DebugFormat("{0} {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ffffff"), "Request");
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ffffff") + "   Reply :   " + msg;
        }

        [ApServiceMethod("数组测试")]
        public string[] TestArray(
            [ApServiceMethodParameter("数组测试")]string[] arr)
        {
            return arr.Reverse<string>().ToArray();
        }

        [ApServiceMethod("数组测试1")]
        public string[] TestArray1(
            [ApServiceMethodParameter("ID")]string id,
            [ApServiceMethodParameter("数组")]string[] arr,
            [ApServiceMethodParameter("count")]int count)
        {
            return arr.Reverse<string>().ToArray();
        }
    }
}
