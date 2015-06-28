/*
 * 2015-03-12
 * Simon
 * 用于标记方法的参数
 */

using System;

namespace ApCommLib
{
    /// <summary>
    /// 标注方法的参数和返回值
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    public class ApServiceMethodParameterAttribute : Attribute
    {
        /// <summary>
        /// 参数或返回值的描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="description">描述</param>
        public ApServiceMethodParameterAttribute(string description)
        {
            Description = description;
        }
    }
}
