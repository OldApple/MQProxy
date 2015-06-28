using Apache.NMS;
using ApCommLib;
using ApCommLib.Middleware;
using Autofac;
using JariSoft.Infrastructure;
using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Stomp
{
    public partial class ProxyMain : Form
    {
        ApServiceApplication _MainServer;

        BindingList<string> LisComeMessage = new BindingList<string>();

        private static string ipAddress = string.Empty;
        /// <summary>
        /// 本机的IP地址
        /// </summary>
        public static string IpAddress
        {
            get
            {
                if (string.IsNullOrEmpty(ipAddress))
                {
                    IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());
                    foreach (IPAddress ip in localIPs)
                    {
                        //根据AddressFamily判断是否为ipv4,如果是InterNetworkV6则为ipv6
                        if (ip.AddressFamily == AddressFamily.InterNetwork)
                        {
                            ipAddress = ip.ToString();
                            break;
                        }
                    }
                }
                return ipAddress;
            }
        }

        public ProxyMain()
        {
            InitializeComponent();

            InitializeDIContainer();

            _MainServer = new ApServiceApplication();

            listBox1.DataSource = LisComeMessage;
            //_MainServer.Connect();

            //绑定服务端接收请求事件
            ApplicationReceivedHandler dd = delegate(ApplicationReceivedEventArgs e)
            {
                OnShowMessage(DateTime.Now, e.InvokeMessage.ServiceClassName + "." + e.InvokeMessage.MethodName, e.IsSuccess);
            };
            _MainServer.ApplicationReceived += (msg) =>
                {
                    listBox1.Invoke(dd, msg);
                };
        }

        /// <summary>
        /// 初始化DIContainer容器
        /// </summary>
        private void InitializeDIContainer()
        {
            ContainerBuilder builder = new ContainerBuilder();
            builder.Register(c => new OpenWireMiddleware("activemq:tcp://localhost:61613?wireFormat.tightEncodingEnabled=true",
                "Activemq.Client", "Server_" + IpAddress, "admin","password",60000,false)).As<IMiddleware>().SingleInstance();
            builder.Register(c => LogManager.GetLogger("ApServiceApplication")).As<ILog>().SingleInstance();
            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly()).Where(t => typeof(ApService).IsAssignableFrom(t)).Named<ApService>(t=>t.Name).SingleInstance();
            DIContainer.RegisterContainer(builder.Build());
        }

        /// <summary>
        /// 在服务窗口中显示信息
        /// </summary>
        /// <param name="time">时间戳</param>
        /// <param name="message">显示内容</param>
        /// <param name="issuccess">是否执行成功</param>
        protected void OnShowMessage(DateTime time, string message,bool issuccess)
        {
            LisComeMessage.Insert(0, string.Format("{0}  {1} {2}", time.ToString("yyyy-MM-dd hh:mm:ss"), message, issuccess.ToString()));
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _MainServer.Disconnect();
            _MainServer.Dispose();
        }
    }
}
