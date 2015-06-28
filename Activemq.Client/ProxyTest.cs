using Apache.NMS;
using ApCommLib;
using ApCommLib.Middleware;
using Autofac;
using JariSoft.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Stomp
{
    public partial class ProxyTest : Form
    {
       
        Proxy.ApServerProxy tt;

        BindingList<string> LisComeMessage = new BindingList<string>();

        public ProxyTest()
        {
            InitializeComponent();

            InitializeDIContainer();

            listBox1.DataSource = LisComeMessage;
            tt = new Proxy.ApServerProxy();
        }

        private static void InitializeDIContainer()
        {
            ContainerBuilder container = new ContainerBuilder();
            container.Register(c => new ApServiceConsumer(new OpenWireMiddleware("activemq:tcp://localhost:61613", "Server_192.168.0.106", "Client_" + IpAddress,
                "admin", "password", 60000, true))).Named<ApServiceConsumer>("Activemq.Server").SingleInstance();
            DIContainer.RegisterContainer(container.Build());
        }

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

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                bool te = tt.DoWait();
                OnShowMessage(DateTime.Now, "返回 " + te.ToString());
                OnShowMessage(DateTime.Now, "等待 DoWait 结果");
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            tt.DoSomething();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            for (int i = 1; i < ndMsgs.Value; i++)
            {
                OnShowMessage(DateTime.Now, tt.Request(i.ToString()));
            }
        }

        /// <summary>
        /// 在服务窗口中显示信息
        /// </summary>
        /// <param name="time">时间戳</param>
        /// <param name="message">显示内容</param>
        /// <param name="issuccess">是否执行成功</param>
        protected void OnShowMessage(DateTime time, string message)
        {
            LisComeMessage.Insert(0, string.Format("{0}  {1}", time.ToString("yyyy-MM-dd hh:mm:ss"), message));
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            OnShowMessage(DateTime.Now, string.Concat(tt.TestArray(new string[] { "1", "2", "3", "4", "5", "6" })));
        }

        private void button5_Click(object sender, EventArgs e)
        {
            OnShowMessage(DateTime.Now, string.Concat(tt.TestArray1("button5", new string[] { "1", "2", "3", "4", "5", "6" }, 8)));
        }
    }
}
