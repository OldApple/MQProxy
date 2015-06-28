using Apache.NMS;
/*
 * 2015-03-15
 * Simon
 * 中间件接口
 */
using System;
using System.Collections.Generic;

namespace ApCommLib.Middleware
{
    /// <summary>
    /// 中间件的接口
    /// </summary>
    public interface IMiddleware : IDisposable
    {
        /// <summary>
        /// 连接中间件
        /// </summary>
        void Connect();

        /// <summary>
        /// 断开中间件
        /// </summary>
        void Disconnect();

        /// <summary>
        /// 是否连接中间件
        /// </summary>
        bool Isconnect { get;}

        /// <summary>
        /// 消息到达事件
        /// </summary>
        event MessageReceivedHandler MessageReceived;

        /// <summary>
        /// 连接中断
        /// </summary>
        event ConnectionInterruptedListener ConnectionClosed;

        /// <summary>
        /// 消息超时设置 单位毫秒
        /// </summary>
        int RequestTimeOut { get; set; }

        /// <summary>
        /// 消息发送
        /// </summary>
        /// <param name="body">发送的消息</param>
        void OnSend(byte[] body);

        /// <summary>
        /// 发送消息并获取回复消息
        /// </summary>
        /// <param name="body">发送的消息</param>
        /// <returns>回复的消息</returns>
        object OnSendForResponse(byte[] body);
    }
}
