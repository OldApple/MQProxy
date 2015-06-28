/*
 * 2015-03-12
 * Simon
 * 用于标记服务类
 */

using System;

namespace ApCommLib
{
    /// <summary>
    /// 所有的服务类必须标注此属性方能生成代理类
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ApServiceAttribute : Attribute
    {
        /// <summary>
        /// 服务类的描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 服务类的版本信息
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public ApServiceAttribute(string description, string version = "NO_VERSION")
        {
            Description = description;
            Version = version;
        }
    }
}
