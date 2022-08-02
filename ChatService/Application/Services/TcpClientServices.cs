using ChatService.Application.Interfaces;
using ChatService.Domain;
using ChatService.Infrastructure.ExternalServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatService.Application.Services
{
    internal class TcpClientServices: ITcpClientServices
    {
        protected volatile TcpClientInfrastructure tcpClientInf;
        private User user;
        private Boolean logged;


        private Boolean quit=false;


        public void ConnectServer(IPAddress ipAddress, int port)
        {
            try
            {
                tcpClientInf = new TcpClientInfrastructure();
                tcpClientInf.Connect( ipAddress,  port);
            }
            catch (Exception e)
            {
                quit = true;
                Console.WriteLine("Connection refused by the server: " + e.Message);
            }
        }

        public Message getMessage()
        {
            if (!quit)
                return tcpClientInf.GetMessage();
            else
            {
                quit = true;
                return null;
            }
        }

        /// <summary>
        /// Send a message to the server
        /// </summary>
        /// <param name="message">Message to send</param>
        public void SendMessage(Message message)
        {
            if (!quit)
                tcpClientInf.SendMessage(message);
            else
            {
                quit = true;
            }
        }

        public void RunCient()
        {
            Thread checkConnection = new Thread(new ThreadStart(this.checkData));
            checkConnection.Start();

            Thread checkQuit = new Thread(new ThreadStart(this.checkQuit));
            checkQuit.Start();
        }

        /// <summary>
        /// Check if we have messages incoming from the server
        /// </summary>
        private void checkData()
        {
            while (!quit)
            {
                try
                {
                    if (tcpClientInf.CheckData())
                    {
                        Thread.Sleep(25);
                        Message message = getMessage();

                        if (message != null)
                        {
                            // We have a message: call to processData
                            Thread processData = new Thread(() => this.processData(message));
                            processData.Start();
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
        /// Do what needs to be done if server is disconnected
        /// </summary>
        private void checkQuit()
        {
            while (!quit)
            {
                if (tcpClientInf.CheckQuit())
                {
                    quit = true;
                    Console.WriteLine("- Client checkQuit: Server disconnected");
                }

                Thread.Sleep(5);
            }
        }

        /// <summary>
        /// Deal with the message received
        /// </summary>
        /// <param name="message"></param>
        private void processData(Message message)
        {
            switch (message.Head)
            {
                case Message.Header.REGISTER:
                    if (message.MessageList[0] == "success")
                    {
                        Console.WriteLine("- Registration success: " + user.Login);
                    }
                    else
                    {
                        Console.WriteLine("- Registration failed: " + user.Login);
                    }
                    break;

                case Message.Header.JOIN:
                    if (message.MessageList[0] == "success")
                    {
                        logged = true;
                        Console.WriteLine("- Connection success: " + user.Login);
                    }
                    else
                    {
                        logged = false;
                        Console.WriteLine("- Connection failed: " + user.Login);
                    }
                    break;

                case Message.Header.QUIT:
                    quit = true;
                    logged = false;
                    Console.WriteLine("Message.Header.QUIT : Server disconnected");
                    break;

                case Message.Header.JOIN_CR:
                    if (message.MessageList[0] == "success")
                    {

                        user.Chatroom = new Chatroom(message.MessageList[0]);
                        Console.WriteLine("- Joined chatroom: " + message.MessageList[1]);
                    }
                    else
                    {
                        Console.WriteLine("- Could not join chatroom: " + message.MessageList[1]);
                    }
                    break;

                case Message.Header.QUIT_CR:
                    if (message.MessageList[0] == "success")
                    {
                        user.Chatroom = null;
                        Console.WriteLine("- Chatroom left: " + message.MessageList[1]);
                    }
                    else
                    {
                        Console.WriteLine("- Could not leave chatroom : " + message.MessageList[1]);
                    }
                    break;

                case Message.Header.CREATE_CR:
                    if (message.MessageList[0] == "success")
                    {
                        SendMessage(new Message(Message.Header.LIST_CR));
                        Console.WriteLine("- Chatroom created: " + message.MessageList[1]);
                    }
                    else
                    {
                        Console.WriteLine("- Could not create chatroom: " + message.MessageList[1]);
                    }
                    break;

                case Message.Header.LIST_CR:


                case Message.Header.LIST_USERS:

                case Message.Header.POST:
            }
        }

    }
}
