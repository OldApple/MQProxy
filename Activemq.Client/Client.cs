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
    public partial class Client : Form
    {
        BindingList<string> publisher = new BindingList<string>();
        Thread t2;
        bool runing = true;

        String user = "admin";
        String password = "password";
        String host = "localhost";
        int port = 61613;
        String se_destination = "server";
        string re_destination = "client";

        DateTime start = DateTime.Now;

        NMSConnectionFactory factory2;
        IConnection connection2;
        ISession session2;
        IDestination dest2;
        IMessageConsumer consumer;
        IMessageProducer producer;

        long publishercount = 0;
        long listenercount = 0;

        public Client()
        {
            InitializeComponent();
            listBox2.DataSource = publisher;

            //客户端
            String brokerUri2 = "stomp:tcp://" + host + ":" + port;
            factory2 = new NMSConnectionFactory(brokerUri2);
            connection2 = factory2.CreateConnection(user, password);
            connection2.Start();
            session2 = connection2.CreateSession(AcknowledgementMode.AutoAcknowledge);
            dest2 = session2.GetQueue(se_destination);
            producer = session2.CreateProducer(dest2);
            consumer = session2.CreateConsumer(session2.GetQueue(re_destination));
            consumer.Listener += (lllll) =>
                {
                    publisher.Add(String.Format("Receive {0} CorrelationId {1}", listenercount++, lllll.NMSCorrelationID));
                };
            publisher.Add("Starting up Publisher.");
            publisher.Add("Sending  messages...");
            t2 = new Thread(new ThreadStart(StartPublisher));
        }

        void StartPublisher()
        {
            while (true)
            {
                if (!runing)
                    continue;
                if (publishercount == 0)
                {
                    start = DateTime.Now;
                }
                IMessage msg = session2.CreateTextMessage(publishercount.ToString());
                msg.NMSReplyTo = session2.GetQueue(re_destination);
                msg.NMSCorrelationID = msg.NMSMessageId;
                producer.Send(msg);
                publisher.Add(String.Format("Sent {0} MsgId {1}", publishercount++, msg.NMSMessageId));
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            t2.Start();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            runing = false;

            publisher.Add("Shutting down Publisher.");
            connection2.Close();
            t2.Abort();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            runing = !runing;
            if (runing)
            {
                button3.Text = "暂停";
                t2.Start();
            }
            else
            {
                button3.Text = "继续";
            }
        }

    }
}
