using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
/*
 * 2015-03-15
 * Simon
 * 远程服务器返回类型
 */

namespace ApCommLib
{
    [Serializable]
    public class ApInvokeReturnMessage
    {
        /// <summary>
        /// 返回值
        /// </summary>
        public object ReturnValue { get; set; }

        /// <summary>
        /// 远程服务器返回的异常信息
        /// </summary>
        public Exception RemoteException { get; set; }
    }
}
