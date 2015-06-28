using Apache.NMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
/*
 * 2015-03-16
 * Simon
 * 消息接收委托、事件参数类
 */ 
namespace ApCommLib
{
    /// <summary>
    /// 消息到达委托
    /// </summary>
    /// <param name="msg">消息</param>
    public delegate void MessageReceivedHandler(IMessage msg);
    /// <summary>
    /// 连接中断
    /// </summary>
    /// <param name="connection">连接对象</param>
    public delegate void ConnectClosedHandler(IConnection connection);

}
