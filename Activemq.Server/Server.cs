using Apache.NMS;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Stomp
{
    public partial class Server : Form
    {
        BindingList<string> listener = new BindingList<string>();
        
        Thread t1;
        
        String user = "admin";
        String password = "password";
        String host = "localhost";
        int port = 61613;
        string re_destination = "server";

        NMSConnectionFactory factory;
        IConnection connection;
        ISession session;
        IMessageConsumer consumer;
        IMessageProducer producer;

        long listenercount = 0;
        long publishercount = 0;
        
        bool runing = true;

        public Server()
        {
            InitializeComponent();
            listBox1.DataSource = listener;
            
            //服务端
            String brokerUri = "stomp:tcp://" + host + ":" + port + "?transport.useLogging=true";
            factory = new NMSConnectionFactory(brokerUri);
            connection = factory.CreateConnection(user, password);
            connection.Start();
            session = connection.CreateSession(AcknowledgementMode.AutoAcknowledge);
            consumer = session.CreateConsumer(session.GetQueue(re_destination));
            producer = session.CreateProducer();
            listener.Add("Starting up Listener.");
            listener.Add("Waiting for messages...");
            t1 = new Thread(new ThreadStart(StartListener));
        }

        void StartListener()
        {
            while (true)
            {
                if (!runing)
                    continue;
                IMessage msg = consumer.Receive();
                if (msg is ITextMessage)
                {
                    ITextMessage txtMsg = msg as ITextMessage;
                    String body = txtMsg.Text;
                    listener.Add(String.Format("Received {0} {1}.", listenercount++, body));
                    ITextMessage rep =  session.CreateTextMessage("I am a Reply ");
                    rep.NMSCorrelationID = txtMsg.NMSCorrelationID;
                    producer.Send(txtMsg.NMSReplyTo, rep);
                    listener.Add(String.Format("Send {0} {1}.", publishercount++, rep));
                }
                else
                {
                    listener.Add("Unexpected message type: " + msg.GetType().Name);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            listener.Add("Shutting down Listener.");
            connection.Close();
            t1.Abort();
        }

        private void Server_Load(object sender, EventArgs e)
        {
            t1.Start();
        }

    }
}
