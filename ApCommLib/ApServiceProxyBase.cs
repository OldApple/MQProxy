/*
 * 2015-03-12
 * Simon
 * 服务端代理生成器
 * 服务类名用属性ApServiceAttribute标注  公开方法用ApServiceMethodAttribute标注 参数或返回值用ApServiceMethodParameterAttribute标注
 */

using Apache.NMS;
using ApCommLib.Middleware;
using JariSoft.Infrastructure;
using System;
using System.Threading.Tasks;

namespace ApCommLib
{
    /// <summary>
    /// 服务代理基类
    /// </summary>
    public abstract class ApServiceProxyBase
    {
        #region Field
        /// <summary>
        /// 服务端消费对象
        /// </summary>
        private readonly ApServiceConsumer _serverConsumer;

        /// <summary>
        /// 服务类名称
        /// </summary>
        private readonly string _serviceClassName;

        #endregion

        #region Constructor
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="serviceClassName">服务端类名</param>
        /// <param name="serviceConsumerName">服务消费类名称</param>
        protected ApServiceProxyBase(string serviceClassName,string serviceConsumerName)
        {
            if (string.IsNullOrEmpty(serviceClassName))
            {
                throw new ArgumentNullException("serviceClassName");
            }
           
            _serviceClassName = serviceClassName;
            if (!DIContainer.IsRegisteredWithName<ApServiceConsumer>(serviceConsumerName))
            {
                throw new Exception(string.Format("未在 DIContainer 中注册名为{0}的 ApServiceConsumer 类型的实例！", serviceConsumerName));
            }
            _serverConsumer = DIContainer.ResolveNamed<ApServiceConsumer>(serviceConsumerName);
        }
        #endregion

        #region Method
        /// <summary>
        /// 调用远程方法
        /// </summary>
        /// <param name="methodName">远程方法名</param>
        /// <param name="args">远程方法使用的参数</param>
        protected void InvokeMethod(string methodName, params object[] args)
        {
            //创建调用方法信息
            var invokeMessage = new ApInvokeMessage
            {
                ServiceClassName = _serviceClassName,
                HasReturn = false,
                MethodName = methodName,
                Parameters = args
            };

            //创建消息并发送
            _serverConsumer.SendMessage(SerializeHelper.SerializeObject(invokeMessage));
        }

        /// <summary>
        /// 调用远程方法并获取返回值
        /// </summary>
        /// <param name="methodName">远程方法名</param>
        /// <param name="args">远程方法使用的参数</param>
        /// <returns>远程方法返回的值</returns>
        protected object InvokeMethodAndResult(string methodName, params object[] args)
        {
            //调用方法信息
            var invokeMessage = new ApInvokeMessage 
            { 
                ServiceClassName = _serviceClassName, 
                HasReturn = true, 
                MethodName = methodName, 
                Parameters = args 
            };

            //创建消息并发送然后阻塞等候回复消息
            ApInvokeReturnMessage objRet = _serverConsumer.SendMessageForResponse(SerializeHelper.SerializeObject(invokeMessage)) as ApInvokeReturnMessage;
            if (objRet.ReturnValue != null)
                return objRet.ReturnValue;
            else
                throw objRet.RemoteException;
        }
        #endregion

    }
}
