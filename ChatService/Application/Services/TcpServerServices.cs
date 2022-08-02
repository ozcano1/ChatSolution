using ChatService.Domain;
using ChatService.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatService.ApplicationCore
{
    [Serializable]
    public abstract class TcpServerServices : ITcpServerServices
    {
        protected volatile TcpServerInfrastructure tcpServerInf;
        protected volatile Boolean running;
        protected int port;
        protected Thread checkDataThread;
        protected Thread checkQuitThread;
        protected Thread listenerThread;
        public TcpServerServices()
        {
            tcpServerInf = new TcpServerInfrastructure();
            running = false;
        }

        public bool IsRunning()
        {
            return running;
        }
        public void StartServer(int port)
        {
            string result = tcpServerInf.StartServer(port, "127.0.0.1");

            if (result == "OK")
            {
                running = true;
            }
            else
            {
                Console.WriteLine(result);
            }
        }

        public void StopServer()
        {
            tcpServerInf.StopServer();
        }


        public  Message GetMessage(Socket socket)
        {
            return tcpServerInf.getMessage(socket);
        }

        public void SendMessage(Message message, Socket socket)
        {
            tcpServerInf.SendMessage(message, socket);
        }

        UserManager userManager;
        SessionManager sessionManager;
        ChatroomManager chatroomManager;

        public volatile Object readLock;

        public UserManager GetUserManager()
        {
            return userManager;
        }
        public SessionManager GetSessionManager()
        {
            return sessionManager;
        }
        public ChatroomManager GetChatroomManager()
        {
            return chatroomManager;
        }
        public void RunServer()
        {
            checkDataThread = new Thread(new ThreadStart(this.checkData));
            checkDataThread.Start();

            checkQuitThread = new Thread(new ThreadStart(this.checkQuit));
            checkQuitThread.Start();

            listenerThread = new Thread(new ThreadStart(this.listen));
            listenerThread.Start();
        }

        private void listen()
        {
            while (this.running)
            {
                try
                {
                    Console.WriteLine("Waiting for a new connection...");
                    TcpClient client = tcpServerInf.Listen();
                    SessionServices session = new SessionServices();
                    session.Client = client;
                    sessionManager.addSession(session);

                    Console.WriteLine("New client: " + session.Token);
                }
                catch (SocketException)
                {
                    // Here we catch a WSACancelBlockingCall exception because this.tcpListener is probably closed
                    Console.WriteLine("Listener thread closed");
                }

            }
        }

        /// <summary>
        /// Check data coming from clients
        /// </summary>
        private void checkData()
        {
            while (this.running)
            {
                try
                {
                    lock (readLock)
                    {
                        if (sessionManager.SessionList.Count > 0)
                        {
                            foreach (SessionServices session in sessionManager.SessionList.ToList())
                            {
                                if (session != null && session.Client.GetStream().DataAvailable)
                                {
                                    Thread.Sleep(25);
                                    Message message = GetMessage(session.Client.Client);

                                    if (message != null)
                                    {
                                        // We have data to process: call to the appropriate function
                                        Thread processData = new Thread(() => this.processData(session, message));
                                        processData.Start();
                                    }
                                }
                            }
                        }
                    }
                }
                catch (InvalidOperationException e)
                {
                    Console.WriteLine(e.Message);
                }

                Thread.Sleep(5);
            }
        }

        /// <summary>
        /// Process data sent by clients
        /// </summary>
        /// <param name="session">Session from where the message comes from</param>
        /// <param name="message">Message received</param>
        private void processData(SessionServices session, Message message)
        {
            if (session.User != null)
            {
                switch (message.Head)
                {
                    case Message.Header.QUIT:
                        {
                            // Warn the user he has been disconnected
                            Message messageSuccess = new Message(Message.Header.QUIT);
                            messageSuccess.addData("success");
                            SendMessage(messageSuccess, session.Client.Client);

                            if (session.User.Chatroom != null)
                            {
                                // Warn the other users that he left
                                broadcastToChatRoom(session, "left the chatroom \"" + session.User.Chatroom.Name + "\"");
                            }

                            session.Client.Close();
                            sessionManager.removeSession(session.Token);

                            Console.WriteLine("- User logout: " + session.Token);
                        }
                        break;

                    case Message.Header.JOIN_CR:
                        // Before joining a chatroom, let's leave the current one
                        quitCr(session, message);

                        try
                        {
                            List<string> messageList = message.MessageList;
                            if (chatroomManager.ChatroomList.Any(x => x.Name == messageList[0]))
                            {
                                session.User.Chatroom = new Chatroom(messageList[0]);
                                Console.WriteLine("- " + session.User.Login + " joined the chatroom: " + messageList[0]);

                                // Tell the client the channel has been joined
                                Message messageSuccess = new Message(Message.Header.JOIN_CR);
                                messageSuccess.addData("success");
                                messageSuccess.addData(messageList[0]);
                                SendMessage(messageSuccess, session.Client.Client);

                                //On broadcast à tous les participants de la conversations l'arrivée de l'utilisateur
                                Message messagePostBroadcast = new Message(Message.Header.POST);
                                broadcastToChatRoom(session, "joined the chatroom \"" + messageList[0] + "\"");
                            }
                        }
                        catch (ChatroomUnknownException e)
                        {
                            // Tell the client the channel has not been joined
                            Message messageSuccess = new Message(Message.Header.JOIN_CR);
                            messageSuccess.addData("error");
                            messageSuccess.addData(message.MessageList[0]);
                            SendMessage(messageSuccess, session.Client.Client);
                            messageSuccess.addData("Chatroom " + e.Message + " does not exist");
                        }
                        break;

                    case Message.Header.QUIT_CR:
                        quitCr(session, message);
                        break;

                    case Message.Header.CREATE_CR:
                        try
                        {
                            List<string> messageList = message.MessageList;
                            chatroomManager.addChatroom(new Chatroom(messageList[0]));
                            chatroomManager.save("chatrooms.db");

                            // Tell the users the chatroom has been created
                            Message messageSuccess = new Message(Message.Header.CREATE_CR);
                            messageSuccess.addData("success");
                            messageSuccess.addData(messageList[0]);
                            SendMessage(messageSuccess, session.Client.Client);

                            Console.WriteLine("- " + session.User.Login + " : chatroom has been created: " + messageList[0]);
                        }
                        catch (ChatroomAlreadyExistsException e)
                        {
                            // Warn the user the chatroom has not been created
                            Message messageError = new Message(Message.Header.CREATE_CR);
                            messageError.addData("error");
                            messageError.addData("Chatroom " + e.Message + " already exists");
                            SendMessage(messageError, session.Client.Client);
                        }
                        break;

                    case Message.Header.LIST_CR:
                        Message messageListCr = new Message(Message.Header.LIST_CR);

                        foreach (Chatroom chatroom in chatroomManager.ChatroomList.ToList())
                        {
                            messageListCr.addData(chatroom.Name);
                        }

                        SendMessage(messageListCr, session.Client.Client);
                        break;

                    case Message.Header.POST:
                        Console.WriteLine("- " + session.User.Login + " : message received : " + message.MessageList[0]);
                        broadcastToChatRoom(session, message.MessageList[0]);
                        break;

                    case Message.Header.LIST_USERS:
                        List<string> chatroomWantedList = message.MessageList;
                        string chatroomWanted = chatroomWantedList[0];

                        Message messageListUsers = new Message(Message.Header.LIST_USERS);

                        // For all users currently connected
                        foreach (SessionServices localSession in sessionManager.SessionList.ToList())
                        {
                            // If the user is in the chatroom we want the userlist
                            if (localSession.User != null &&
                                localSession.User.Chatroom != null &&
                                localSession.User.Chatroom.Name == chatroomWanted)
                            {
                                messageListUsers.addData(localSession.User.Login);
                            }
                        }

                        SendMessage(messageListUsers, session.Client.Client);

                        break;
                }
            }
            else
            {
                switch (message.Head)
                {
                    case Message.Header.REGISTER:
                        try
                        {
                            List<string> messageList = message.MessageList;
                            userManager.addUser(messageList[0], messageList[1]);
                            userManager.save("users.db");

                            // Tell the user his account has been created
                            Message messageSuccess = new Message(Message.Header.REGISTER);
                            messageSuccess.addData("success");
                            SendMessage(messageSuccess, session.Client.Client);

                            Console.WriteLine("- Registration success : " + messageList[0]);
                        }
                        catch (UserAlreadyExistsException e)
                        {
                            // Warn the user his account has not been created
                            Message messageSuccess = new Message(Message.Header.REGISTER);
                            messageSuccess.addData("error");
                            SendMessage(messageSuccess, session.Client.Client);

                            Console.WriteLine("- Registration failed : " + e.Message);
                        }
                        break;

                    case Message.Header.JOIN:
                        try
                        {
                            List<string> messageList = message.MessageList;
                            userManager.authentify(messageList[0], messageList[1]);
                            session.User = new User(messageList[0], messageList[1]);
                            userManager.save("users.db");

                            Message messageSuccess = new Message(Message.Header.JOIN);
                            messageSuccess.addData("success");
                            SendMessage(messageSuccess, session.Client.Client);

                            Console.WriteLine("- Login success : " + session.User.Login);
                        }
                        catch (WrongPasswordException e)
                        {
                            Message messageSuccess = new Message(Message.Header.JOIN);
                            messageSuccess.addData("error");
                            SendMessage(messageSuccess, session.Client.Client);

                            Console.WriteLine("- Login failed : " + e.Message);
                        }

                        break;
                }
            }
        }

        /// <summary>
        /// Function used to send a message to all users in a chatroom
        /// </summary>
        /// <param name="session"></param>
        /// <param name="message"></param>
        private void broadcastToChatRoom(SessionServices session, string message)
        {
            Chatroom chatroom = session.User.Chatroom;

            if (chatroom != null && message != "")
            {
                Message messageJoin = new Message(Message.Header.POST);
                messageJoin.addData(session.User.Login);
                messageJoin.addData(session.User.Login + ": " + message);

                foreach (SessionServices sessionUser in sessionManager.SessionList.ToList())
                {
                    if (sessionUser.User.Chatroom != null &&
                        sessionUser.User.Chatroom.Name == chatroom.Name)
                    {
                        SendMessage(messageJoin, sessionUser.Client.Client);
                    }
                }

                Console.WriteLine("- " + session.User.Login + "'s message broadcast");
            }
            else
            {
                Console.WriteLine("- User is not connected to any chatroom: " + session.User.Login);
            }
        }

        /// <summary>
        /// Check if a client has left
        /// </summary>
        private void checkQuit()
        {
            while (this.running)
            {
                for (int i = 0; i < sessionManager.SessionList.Count; i++)
                {
                    Socket socket = sessionManager.SessionList[i].Client.Client;

                    if (socket.Poll(10, SelectMode.SelectRead) && socket.Available == 0)
                    {
                        Console.WriteLine("- User logged out : " + sessionManager.SessionList[i].Token);

                        lock (readLock)
                        {
                            if (sessionManager.SessionList[i].User != null &&
                                sessionManager.SessionList[i].User.Chatroom != null)
                            {
                                // Tell the other users that he left
                                broadcastToChatRoom(sessionManager.SessionList[i], "left the chatroom \"" +
                                    sessionManager.SessionList[i].User.Chatroom.Name + "\"");
                            }

                            sessionManager.SessionList[i].Client.Close();
                            sessionManager.removeSession(sessionManager.SessionList[i].Token);
                        }
                    }
                }

                Thread.Sleep(5);
            }
        }

        /// <summary>
        /// Check if a client has left a chatroom
        /// </summary>
        /// <param name="session"></param>
        /// <param name="message"></param>
        private void quitCr(SessionServices session, Message message)
        {
            try
            {
                if (session.User.Chatroom != null)
                {
                    // Warn the user he left the chatroom
                    Message messageSuccess = new Message(Message.Header.QUIT_CR);
                    messageSuccess.addData("success");
                    messageSuccess.addData(session.User.Chatroom.Name);
                    SendMessage(messageSuccess, session.Client.Client);

                    // Warn the other users that this one left
                    broadcastToChatRoom(session, "left the chatroom \"" + session.User.Chatroom.Name + "\"");

                    Console.WriteLine("- " + session.User.Login + " left the chatroom: " + session.User.Chatroom.Name);

                    session.User.Chatroom = null;
                }
            }
            catch (ChatroomUnknownException e)
            {
                // Warn the user the chatroom does not exist
                Message messageError = new Message(Message.Header.QUIT_CR);
                messageError.addData("error");
                messageError.addData(message.MessageList[0]);
                SendMessage(messageError, session.Client.Client);

                messageError.addData("Chatroom " + e.Message + " does not exist");
            }
        }

    }
}