/*
 * 2015-03-12
 * Simon
 * 服务端程序
 *      存放服务类的容器
 */
using Autofac;
using System;
using System.Collections.Generic;
using System.Reflection;
using log4net;
using Apache.NMS;
using ApCommLib.Middleware;
using JariSoft.Infrastructure;
using System.Threading;

namespace ApCommLib
{
    /// <summary>
    /// 服务端与客户端交互主服务类
    /// 服务可以通过 Autofac注册 也可通过调用AddService注册
    /// </summary>
    public class ApServiceApplication : IDisposable
    {
        #region Fields/Properties

        /// <summary>
        /// 消息中间件
        /// </summary>
        private IMiddleware _middleware;

        private ILog _logger;
        /// <summary>
        /// 日志记录器
        /// </summary>
        public ILog Logger
        {
            get
            {
                return _logger;
            }
        }

        /// <summary>
        /// 服务类集合
        /// </summary>
        private SortedList<string, ServiceObject> _serviceObjects;

        /// <summary>
        /// 服务端接收消息并处理消息后抛出请求信息及处理结果
        /// </summary>
        public event ApplicationReceivedHandler ApplicationReceived;

        #endregion

        #region Constructors/Initialize

        /// <summary>
        /// 构造函数
        /// </summary>
        public ApServiceApplication()
        {
            Initialize();
        }

        /// <summary>
        /// Initializes this object.
        /// </summary>
        private void Initialize()
        {
            if (!DIContainer.IsRegistered<IMiddleware>())
            {
                throw new Exception("未在 DIContainer 中注册 IMiddleware 类型的实例！");
            }
            if (!DIContainer.IsRegistered<ILog>())
            {
                throw new Exception("未在 DIContainer 中注册 ILog 类型的实例！");
            }
            _serviceObjects = new SortedList<string, ServiceObject>();
            _logger = DIContainer.Resolve<ILog>();
            _middleware = DIContainer.Resolve<IMiddleware>();
            //通过注册的服务初始化_serviceObjects
            foreach (ApService aps in DIContainer.Resolves<ApService>())
            {
                AddService(aps);
            }
            //初始化最大线程数
            InitializeMaxThread();
            _middleware.MessageReceived += (msg) =>
                {
                    if (!(msg is IBytesMessage))
                        return;

                    //同步调用
                    //DoInvoke(msg);

                    //异步调用
                    ThreadPool.QueueUserWorkItem(new WaitCallback(DoInvoke), msg);
                };
        }

        /// <summary>
        /// 初始化线程池
        /// </summary>
        /// <param name="threadnum">线程数</param>
        /// <param name="completionPortThread">线程池中异步I/O的线程数</param>
        private void InitializeMaxThread(int threadnum = 1000,int completionPortThread = 1000)
        {
            int maxThreadNum, maxportThreadNum;
            int minThreadNum, minportThreadNum;
            ThreadPool.GetMaxThreads(out maxThreadNum, out maxportThreadNum);
            ThreadPool.GetMinThreads(out minThreadNum, out minportThreadNum);

            if (threadnum <= minThreadNum)
            {
                threadnum = minThreadNum + 5;
            }
            if (threadnum >= maxThreadNum)
            {
                threadnum = maxThreadNum - 5;
            }
            if (completionPortThread <= minportThreadNum)
            {
                completionPortThread = minportThreadNum + 5;
            }
            if (completionPortThread >= maxportThreadNum)
            {
                completionPortThread = maxportThreadNum - 5;
            }
            if (!ThreadPool.SetMinThreads(100, 100))
            {
                Logger.DebugFormat("设置最小线程池数目出错！设置值 100,100 线程数 {0}~{1} I/O线程数 {2}~{3}", minThreadNum, maxThreadNum, minportThreadNum, maxportThreadNum);
            }
            if (!ThreadPool.SetMaxThreads(threadnum, completionPortThread))
            {
                Logger.DebugFormat("设置最大线程池数目出错！设置值 {0},{1} 线程数 {2}~{3} I/O线程数 {4}~{5}", threadnum, completionPortThread, minThreadNum, maxThreadNum, minportThreadNum, maxportThreadNum);
            }
        }
        #endregion

        #region Methods

        /// <summary>
        /// 连接消息中间件
        /// </summary>
        public void Connect()
        {
            _middleware.Connect();
        }

        /// <summary>
        /// 断开与消息中间件的连接
        /// </summary>
        public void Disconnect()
        {
            _middleware.Disconnect();
        }
        
        /// <summary>
        /// 添加服务类
        /// </summary>
        /// <param name="service">Service to add</param>
        public void AddService(ApService service)
        {
            if (service == null)
            {
                throw new ArgumentNullException("service");
            }

            var type = service.GetType();
            var attributes = type.GetCustomAttributes(typeof (ApServiceAttribute), true);
            if(attributes.Length <= 0)
            {
                throw new Exception("Service class must has ApService attribute to be added.");
            }

            if (_serviceObjects.ContainsKey(type.Name))
            {
                throw new Exception("Service '" + type.Name + "' is already added.");
            }
            service.ServiceApplication = this;
            _serviceObjects.Add(type.Name, new ServiceObject(service, (ApServiceAttribute)attributes[0]));
        }

        /// <summary>
        /// 移除服务类
        /// </summary>
        /// <param name="service">Service to add</param>
        public void RemoveService(ApService service)
        {
            if (service == null)
            {
                throw new ArgumentNullException("service");
            }

            var type = service.GetType();
            if (!_serviceObjects.ContainsKey(type.Name))
            {
                return;
            }

            _serviceObjects.Remove(type.Name);
        }

        /// <summary>
        /// 释放占用的资源
        /// </summary>
        public void Dispose()
        {
            //释放消息中间件
            _middleware.Dispose();

            //释放服务资源
            foreach (var item in _serviceObjects)
            {
                item.Value.Dispose();
            }
            _serviceObjects.Clear();
        }

        /// <summary>
        /// 调用服务
        /// </summary>
        /// <param name="inmessage">接收的消息</param>
        private void DoInvoke(object inmessage)
        {
            bool isSuccess = true;
            IBytesMessage msg = inmessage as IBytesMessage;
            ApInvokeMessage invokeMessage = null;//Deserialize message
            try
            {
                invokeMessage = (ApInvokeMessage)SerializeHelper.DeserializeObject(msg.Content);
                //Check service class name
                if (!_serviceObjects.ContainsKey(invokeMessage.ServiceClassName))
                {
                    throw new Exception("There is no service with name '" + invokeMessage.ServiceClassName + "'");
                }

                //Get service object
                var serviceObject = _serviceObjects[invokeMessage.ServiceClassName];

                //Invoke service method and get return value
                object returnValue = serviceObject.InvokeMethod(invokeMessage.MethodName, invokeMessage.Parameters);

                if (invokeMessage.HasReturn)
                {
                    //Send return value to sender application
                    SendReturnValue(msg, returnValue);
                }
            }
            catch (Exception ex)
            {
                isSuccess = false;
                SendException(msg, ex);
            }
            finally
            {
                if (ApplicationReceived != null)
                {
                    ApplicationReceived(new ApplicationReceivedEventArgs { InvokeMessage = invokeMessage, IsSuccess = isSuccess });
                }
            }
        }
        /// <summary>
        /// 回复异常信息
        /// </summary>
        /// <param name="inMessage">请求信息</param>
        /// <param name="exception">异常</param>
        private void SendException(IMessage inMessage, Exception exception)
        {
            try
            {
                //Create return message
                var reMessage = new ApInvokeReturnMessage { RemoteException = exception};
                (_middleware as OpenWireMiddleware).Reply(inMessage, SerializeHelper.SerializeObject(reMessage));
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
            }
        }
        /// <summary>
        /// 回复请求结果
        /// </summary>
        /// <param name="inMessage">请求信息</param>
        /// <param name="returnValue">结果值</param>
        private void SendReturnValue(IMessage inMessage, object returnValue)
        {
            try
            {
                //Create return message
                var reMessage = new ApInvokeReturnMessage { ReturnValue = returnValue};
                (_middleware as OpenWireMiddleware).Reply(inMessage, SerializeHelper.SerializeObject(reMessage));
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
            }
        }

        #endregion

        #region Sub classes
        private class ServiceObject : IDisposable
        {
            /// <summary>
            /// 服务类
            /// </summary>
            public ApService Service { get; private set; }

            /// <summary>
            /// 服务类属性
            /// </summary>
            public ApServiceAttribute ServiceAttribute { get; private set; }

            /// <summary>
            /// 服务类名称
            /// </summary>
            private readonly string _serviceClassName;

            /// <summary>
            /// 服务类的公开方法列表
            /// Key: Method name
            /// Value: True, if it can be invoked from remote application. 
            /// </summary>
            private readonly SortedList<string, bool> _methods;

            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="service">The service object that is used to invoke methods on</param>
            /// <param name="serviceAttribute">MDSService attribute of service object's class</param>
            public ServiceObject(ApService service, ApServiceAttribute serviceAttribute)
            {
                Service = service;
                ServiceAttribute = serviceAttribute;

                _serviceClassName = service.GetType().Name;

                //Find all methods
                _methods = new SortedList<string, bool>();
                foreach (var methodInfo in Service.GetType().GetMethods())
                {
                    var attributes = methodInfo.GetCustomAttributes(typeof(ApServiceMethodAttribute), true);
                    _methods.Add(methodInfo.Name, attributes.Length > 0);
                }
            }

            /// <summary>
            /// 调用方法
            /// </summary>
            /// <param name="methodName">方法名</param>
            /// <param name="parameters">方法参数</param>
            /// <returns>返回值</returns>
            public object InvokeMethod(string methodName, params object[] parameters)
            {
                try
                {
                    var returnValue = Service.GetType().GetMethod(methodName).Invoke(Service, parameters);
                    return returnValue;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            public void Dispose()
            {
                Service.Dispose();
            }
        }
        #endregion
    }
}
