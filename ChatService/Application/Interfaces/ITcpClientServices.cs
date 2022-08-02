using ChatService.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ChatService.Application.Interfaces
{
    public interface ITcpClientServices
    {
        public void ConnectServer(IPAddress ipAddress, int port);
        public Message GetMessage();
        public void SendMessage(Message message);
        public UserManager GetUserManager();
        public SessionManager GetSessionManager();
        public void RunCient();

        public bool IsClientQuitted();

        public bool Quit();


    }
}
