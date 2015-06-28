using ApCommLib;
using ApServerProxyGenerator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stomp
{
    [ApService("邮件服务","1.0.0.0")]
    public class MailServer:ApService
    {
        [ApServiceMethod("发送邮件")]
        public void SendMial(
            [ApServiceMethodParameter("收件人")]string touser)
        {
            
        }
    }
}
