using ChatService.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace ChatService.Infrastructure.ExternalServices
{
    public class TcpClientInfrastructure
    {
        protected TcpClient tcpClient;

        public void Connect(IPAddress ipAddress, int port)
        {
            try
            {
                tcpClient = new TcpClient(ipAddress.ToString(), port);
            }
            catch (SocketException e)
            {
                throw new Exception(e.Message);
            }
        }
        public Message GetMessage()
        {
            try
            {
                NetworkStream strm = tcpClient.GetStream();
                IFormatter formatter = new BinaryFormatter();
                Message message = (Message)formatter.Deserialize(strm);
                Console.WriteLine("## TCPClient Receiving a message: " + message.Head);
                return message;
            }
            catch (SerializationException e)
            {
                Console.WriteLine("TCPClient sendMessage exception: " + e.Message);
            }
            catch (IOException e)
            {
                Console.WriteLine("TCPClient sendMessage exception: " + e.Message);
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine("TCPClient sendMessage exception: " + e.Message);
            }

            return null;
        }

        /// <summary>
        /// Send a message to the server
        /// </summary>
        /// <param name="message">Message to send</param>
        public void SendMessage(Message message)
        {
            Console.WriteLine("## TCPClient Sending a message: " + message.Head);

            try
            {
                IFormatter formatter = new BinaryFormatter();
                NetworkStream strm = tcpClient.GetStream();
                formatter.Serialize(strm, message);
            }
            catch (SerializationException e)
            {
                Console.WriteLine("TCPClient sendMessage exception: " + e.Message);
            }
            catch (IOException e)
            {
                Console.WriteLine("TCPClient sendMessage exception: " + e.Message);
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine("TCPClient sendMessage exception: " + e.Message);
            }
        }

        public bool CheckData()
        {
            return tcpClient.GetStream().DataAvailable;
        }

        public bool CheckQuit()
        {
            Socket socket = tcpClient.Client;

            return socket.Poll(10, SelectMode.SelectRead) && socket.Available == 0;
        }
    }
}


