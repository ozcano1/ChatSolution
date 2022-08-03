using ChatService.Application.Services;
using ChatService.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatService.Application.Interfaces
{
    public interface ITcpServerServices
    {
        public bool IsRunning();
        public void StartServer(int port);
        public void StopServer();
        public Message GetMessage(Socket socket);
        public void SendMessage(Message message, Socket socket);
        public UserManager GetUserManager();
        public SessionManagerServices GetSessionManager();
        public ChatroomManager GetChatroomManager();
        public void RunServer();
    }
}
