﻿/* 
 * This code file is generated by ApService Proxy Generator tool.
 * Service Name    : ApServer
 * Service version : 1.0.0.1
 * Generating date : 2015-06-28 11:32:20
 */

using ApCommLib;

using System;
using System.Data;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Stomp.Proxy
{
    /// <summary>
    /// This class is a proxy class to use ApServer service.
    /// Description: ApService 测试类
    /// </summary>
    public partial class ApServerProxy : ApServiceProxyBase
    {
        #region Constructor
        
        /// <summary>
        ///  Creates a new instance of ApServerProxy.
        /// </summary>
        /// <param name="serviceConsumerName">Reference to a ApServiceConsumer object to send/receive messages</param>
        public ApServerProxy(string serviceConsumerName = "Activemq.Server")
            : base("ApServer",serviceConsumerName)
        {
            
        }
        
        #endregion
        
        #region ApServer methods
        
        /// <summary>
        /// 等待执行
        /// </summary>
        public bool DoWait()
        {
            return (bool) InvokeMethodAndResult("DoWait");
        }
        
        /// <summary>
        /// 睡眠
        /// </summary>
        public void DoSomething()
        {
            InvokeMethod("DoSomething");
        }
        
        /// <summary>
        /// 请求
        /// </summary>
        /// <param name="msg">请求信息</param>
        public string Request(string msg)
        {
            return (string) InvokeMethodAndResult("Request", msg);
        }
        
        /// <summary>
        /// 数组测试
        /// </summary>
        /// <param name="arr">数组测试</param>
        public string[] TestArray(string[] arr)
        {
            return (string[]) InvokeMethodAndResult("TestArray", new object[] { arr });
        }
        
        /// <summary>
        /// 数组测试1
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="arr">数组</param>
        /// <param name="count">count</param>
        public string[] TestArray1(string id, string[] arr, int count)
        {
            return (string[]) InvokeMethodAndResult("TestArray1", id, arr, count);
        }
        
        #endregion
        
        #region Default (predefined) service methods
        
        /// <summary>
        /// This method can be used to check if service is available.
        /// </summary>
        /// <param name="message">A message to reply</param>
        /// <returns>Reply to message as formatted: 'RE: message'</returns>
        public string CheckServiceIsAvailable(string message)
        {
            return (string) InvokeMethodAndResult("CheckServiceIsAvailable", message);
        }
        
        #endregion
    }
}

