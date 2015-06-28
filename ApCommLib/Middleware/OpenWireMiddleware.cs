using Apache.NMS;
using Apache.NMS.ActiveMQ.Commands;
using JariSoft.Infrastructure;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApCommLib.Middleware
{
    public class OpenWireMiddleware : IMiddleware
    {
        #region Field/Properties
        readonly OpenWireConsumer _consumer = null;
        readonly OpenWireProducer _producer = null;
        /// <summary>
        /// 请求字典
        /// </summary>
        private readonly IDictionary requestMap = Hashtable.Synchronized(new Hashtable());

        #endregion

        #region Constructor/Initialize
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="brokerUri">中间件地址</param>
        /// <param name="sendDest">发送消息目的地</param>
        /// <param name="clientId">客户端标识</param>
        /// <param name="userName">用户名</param>
        /// <param name="psw">密码</param>
        /// <param name="timeout">超时时间 单位毫秒</param>
        /// <param name="isClient">true 客户端；false 服务端</param>
        public OpenWireMiddleware(string brokerUri, string sendDest, string clientId, string userName, string psw, int timeout,bool isClient)
        {
            _consumer = new OpenWireConsumer(brokerUri, userName, psw, clientId,isClient);
            _producer = new OpenWireProducer(brokerUri, userName, psw, sendDest,isClient);
            _timeout = timeout;
            _consumer.Listener += (message) =>
                {
                    string correlationId = message.NMSCorrelationID;
                    if (!string.IsNullOrEmpty(correlationId))
                    {
                        //回复信息
                        ApFutureResponse future = (ApFutureResponse)requestMap[correlationId];
                        if (future != null)
                        {
                            future.Response = SerializeHelper.DeserializeObject((message as ActiveMQBytesMessage).Content) as ApInvokeReturnMessage;
                            requestMap.Remove(correlationId);
                        }
                    }
                    //再次抛出消息到达事件
                    if (MessageReceived != null)
                    {
                        MessageReceived(message);
                    }
                };
        }
        #endregion

        #region Methods
        /// <summary>
        /// 回复信息
        /// </summary>
        /// <param name="inmessage">请求信息</param>
        /// <param name="body">回复内容</param>
        public void Reply(IMessage inmessage,byte[] body)
        {
            _producer.Reply(inmessage, body);
        }
        #endregion

        #region Interface
        public event MessageReceivedHandler MessageReceived;
        /// <summary>
        /// 连接
        /// </summary>
        public void Connect()
        {
            _consumer.Start();
            _producer.Start();
        }
        /// <summary>
        /// 关闭
        /// </summary>
        public void Disconnect()
        {
            _consumer.Close();
            _producer.Close();
        }

        public bool Isconnect
        {
            get { return _consumer.IsConnect & _producer.IsConnect; }
        }

        public void Dispose()
        {
            _producer.Dispose();
            _consumer.Dispose();
        }

        private int _timeout;
        /// <summary>
        /// 超时时间 单位毫秒
        /// </summary>
        public int RequestTimeOut
        {
            get { return _timeout; }
            set { _timeout = value; }
        }

        public void OnSend(byte[] body)
        {
            _producer.OnSend(body);
        }

        public object OnSendForResponse(byte[] body)
        {
            //定义回复信息
            string correlationId = IdFactory.Int64KeyStr;
            ApFutureResponse response = new ApFutureResponse();
            response.ResponseTimeout = TimeSpan.FromMilliseconds(_timeout);
            requestMap[correlationId] = response;
            _producer.OnSendForResponse(body, _consumer.QReceiveDest, correlationId);

            //等待回复消息
            return response.Response;
        }

        public event ConnectionInterruptedListener ConnectionClosed;

        public event ConnectionResumedListener ConnectionStarted;

        #endregion
    }

    public class OpenWireProducer
    {
        /// <summary>
        /// 连接
        /// </summary>
        private IConnection _connection;
        /// <summary>
        /// 会话
        /// </summary>
        private ISession _session;
        /// <summary>
        /// 消息生产者
        /// </summary>
        private IMessageProducer _messageProducer;
        /// <summary>
        /// 队列---发送消息目的地
        /// </summary>
        private IQueue _qSendDest;
        /// <summary>
        /// 主题---发送消息目的地
        /// 暂未使用
        /// </summary>
        private ITopic _tSendDest;

        /// <summary>
        /// 消息发送
        /// </summary>
        /// <param name="brokerUri">Apollo地址</param>
        /// <param name="username">用户名</param>
        /// <param name="psw">密码</param>
        /// <param name="qsenddest">队列发送目的地</param>
        /// <param name="isClient">true 客户端；false 服务端</param>
        public OpenWireProducer(string brokerUri,string username,string psw,string qsenddest,bool isClient)
        {
            NMSConnectionFactory _factory = new NMSConnectionFactory(brokerUri);
            _connection = _factory.CreateConnection(username, psw);
            _connection.Start();
            _session = _connection.CreateSession(AcknowledgementMode.AutoAcknowledge);
            
            if (isClient)
            {
                _qSendDest = _session.GetQueue(qsenddest);
                _messageProducer = _session.CreateProducer(_qSendDest);
            }
            else
            {
                //服务端不需要发送地址
                _messageProducer = _session.CreateProducer();
            }
            
            _messageProducer.DeliveryMode = MsgDeliveryMode.NonPersistent;//非持久化
        }

        public bool IsConnect
        {
            get
            {
                return _connection.IsStarted;
            }
        }
        public void Start()
        {
            _connection.Start();
        }
        /// <summary>
        /// 暂时停止 调用Start可以恢复
        /// </summary>
        public void Stop()
        {
            _connection.Stop();
        }
        /// <summary>
        /// 关闭连接 释放资源
        /// </summary>
        public void Close()
        {
            _connection.Close();
        }
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            _messageProducer.Dispose();
            _session.Dispose();
            _connection.Dispose();
        }

        /// <summary>
        /// 发送消息
        /// 
        /// </summary>
        /// <param name="body">消息内容</param>
        /// <param name="sendDest">发送目的地 服务端需指定目的地 客户端已在MessageProducer初始化时指定了目的地</param>
        public void OnSend(byte[] body, IDestination sendDest = null)
        {
            IBytesMessage message = _messageProducer.CreateBytesMessage(body);
            if (sendDest == null)
            {
                _messageProducer.Send(message);
            }
            else
            {
                _messageProducer.Send(sendDest, message);
            }
        }

        /// <summary>
        /// 发送消息并获取返回值
        /// </summary>
        /// <param name="body">消息内容</param>
        /// <param name="listenerdest">回复目的地</param>
        /// <param name="correlationId">请求关联Id</param>
        /// <param name="sendDest">发送目的地 服务端需指定目的地 客户端已在MessageProducer初始化时指定了目的地</param>
        /// <returns>返回消息</returns>
        public void OnSendForResponse(byte[] body,IDestination listenerdest,string correlationId,IDestination sendDest = null)
        {
            IBytesMessage message = _messageProducer.CreateBytesMessage(body);
            message.NMSCorrelationID = correlationId;//关联 请求与回复标识
            message.NMSReplyTo = listenerdest;

            //发送消息
            if (sendDest == null)
            {
                _messageProducer.Send(message);
            }
            else
            {
                _messageProducer.Send(sendDest, message);
            }
        }

        /// <summary>
        /// 回复消息
        /// </summary>
        /// <param name="inmessage">请求信息</param>
        /// <param name="body">回复内容</param>
        public void Reply(IMessage inmessage, byte[] body)
        {
            IBytesMessage msg = _messageProducer.CreateBytesMessage(body);
            msg.NMSCorrelationID = inmessage.NMSCorrelationID;
            msg.NMSDestination = inmessage.NMSReplyTo;
            _messageProducer.Send(inmessage.NMSReplyTo, msg);
        }

    }

    public class OpenWireConsumer
    {
        /// <summary>
        /// 连接
        /// </summary>
        private IConnection _connection;
        /// <summary>
        /// 会话
        /// </summary>
        private ISession _session;
        /// <summary>
        /// 消息消费者
        /// </summary>
        private IMessageConsumer _messageConsumer;
        /// <summary>
        /// 队列---接收消息目的地
        /// </summary>
        private IDestination _qReceiveDest;
        /// <summary>
        /// 队列---接收消息目的地
        /// </summary>
        public IDestination QReceiveDest
        {
            get
            {
                return _qReceiveDest;
            }
        }
        /// <summary>
        /// 主题---接收消息目的地
        /// </summary>
        private ITopic _tReceiveDest;

        /// <summary>
        /// 消息到达事件
        /// </summary>
        public MessageListener Listener;

        /// <summary>
        /// 消息消费构造器
        /// </summary>
        /// <param name="brokerUri">地址</param>
        /// <param name="username">用户名</param>
        /// <param name="psw">密码</param>
        /// <param name="clientId">客户端标识 兼做队列接收目的地</param>
        /// <param name="isClient">true 客户端；false 服务端</param>
        public OpenWireConsumer(string brokerUri, string username, string psw, string clientId,bool isClient)
        {
            NMSConnectionFactory _factory = new NMSConnectionFactory(brokerUri, clientId);
            _connection = _factory.CreateConnection(username, psw);
            _connection.Start();
            _session = _connection.CreateSession(AcknowledgementMode.AutoAcknowledge);

            if (isClient)
            {
                _qReceiveDest = _session.GetDestination(clientId, DestinationType.TemporaryQueue);
            }
            else
            {
                _qReceiveDest = _session.GetQueue(clientId);
            }
            
            _messageConsumer = _session.CreateConsumer(_qReceiveDest);
            _messageConsumer.Listener += (message) =>
            {
                if (Listener != null)
                {
                    Listener(message);
                }
            };
        }

        public bool IsConnect
        {
            get
            {
               return  _connection.IsStarted;
            }
        }
        public void Start()
        {
            _connection.Start();
        }
        /// <summary>
        /// 暂时停止 调用Start可以恢复
        /// </summary>
        public void Stop()
        {
            _connection.Stop();
        }
        /// <summary>
        /// 关闭连接 释放资源
        /// </summary>
        public void Close()
        {
            _connection.Close();
        }
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            _messageConsumer.Dispose();
            _session.Dispose();
            _connection.Dispose();
        }
    }
}
