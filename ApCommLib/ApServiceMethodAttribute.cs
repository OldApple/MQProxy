/*
 * 2015-03-12
 * Simon
 * 用于标记服务方法
 */

using System;

namespace ApCommLib
{
    /// <summary>
    ///  所有服务方法均需用此属性标注
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ApServiceMethodAttribute : Attribute
    {
        /// <summary>
        /// 方法的描述
        /// </summary>
        public string Description { get; set; }

        public ApServiceMethodAttribute(string description)
        {
            Description = description;
        }
    }
}
