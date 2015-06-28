using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/*
 * 2015-03-18
 * Simon
 * 服务端接收消息并处理完消息后抛出的处理结果
 * 供服务端监视运行结果
 * 
 */ 

namespace ApCommLib
{
    /// <summary>
    /// 服务器消息到达委托
    /// </summary>
    /// <param name="e">消息参数</param>
    public delegate void ApplicationReceivedHandler(ApplicationReceivedEventArgs e);

    public class ApplicationReceivedEventArgs:EventArgs
    {
        /// <summary>
        /// 请求信息
        /// </summary>
        public ApInvokeMessage InvokeMessage { get; set; }
        /// <summary>
        /// 是否执行成功
        /// </summary>
        public bool IsSuccess { get; set; }
    }
}
