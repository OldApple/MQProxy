/*
 * 2015-03-12
 * Simon
 * 远程函数调用信息
 */

using System;

namespace ApCommLib
{
    [Serializable]
    public class ApInvokeMessage
    {
        /// <summary>
        /// 服务类名
        /// </summary>
        public string ServiceClassName { get; set; }

        /// <summary>
        /// 是否有返回值
        /// </summary>
        public bool HasReturn { get; set; }

        /// <summary>
        /// 方法名
        /// </summary>
        public string MethodName { get; set; }

        /// <summary>
        /// 方法参数集合
        /// </summary>
        public object[] Parameters { get; set; }
    }
}
