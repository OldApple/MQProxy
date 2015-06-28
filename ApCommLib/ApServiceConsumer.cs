/*
 * 2015-03-12
 * Simon
 * 服务代理使用，服务消费类
 *      提供客户端与服务端的通信
 */

using Apache.NMS;
using Apache.NMS.ActiveMQ.Commands;
using ApCommLib.Middleware;
using JariSoft.Infrastructure;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ApCommLib
{
    public class ApServiceConsumer : IDisposable
    {
        #region Fields

        /// <summary>
        /// 消息中间件
        /// </summary>
        private readonly IMiddleware _middleware;

        /// <summary>
        /// 接收到的服务端返回信息
        /// </summary>
        private static List<Message> _inMessages;

        #endregion

        #region Constructors/Initialize
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="middleware">消息中间件</param>
        public ApServiceConsumer(IMiddleware middleware)
        {
            _middleware = middleware;

            Initialize();
        }
        private void Initialize()
        {
            _inMessages = new List<Message>();
            _middleware.Connect();
            _middleware.MessageReceived += (msg) =>
                {
                    //广播消息或者其他消息
                    _inMessages.Add(msg as Message);
                };
        }
        #endregion

        #region Public methods

        /// <summary>
        /// 连接消息中间件
        /// </summary>
        public void Connect()
        {
            _middleware.Connect();
        }

        /// <summary>
        /// 断开消息中间件连接
        /// </summary>
        public void Disconnect()
        {
            _middleware.Disconnect();
        }

        /// <summary>
        /// 释放占用的资源
        /// </summary>
        public void Dispose()
        {
            _middleware.Dispose();
        }

        /// <summary>
        /// 创建消息并发送
        /// </summary>
        /// <param name="content">消息内容</param>
        /// <remarks>
        /// 创建默认设置的消息并发送
        /// </remarks>
        public void SendMessage(byte[] content)
        {
            _middleware.OnSend(content);
        }
        /// <summary>
        /// 创建消息并发送
        /// 然后异步等候回复消息
        /// </summary>
        /// <param name="content">消息内容</param>
        public object SendMessageForResponse(byte[] content)
        {
            return _middleware.OnSendForResponse(content);
        }

        #endregion

    }
}
