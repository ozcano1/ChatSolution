using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using ChatService.Domain;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace ChatService.ApplicationCore
{
    
    public class TcpServerInfrastructure 
    {
        protected volatile TcpClient commSocket;
        protected volatile TcpListener tcpListener;

        public string StartServer(int port, string ip)
        {
            IPAddress ipAddress = IPAddress.Parse(ip);

            try
            {
                tcpListener = new TcpListener(ipAddress, port);
                tcpListener.Start();
                return ("OK");
            }
            catch (SocketException e)
            {
              return ("Connection is not possible: " + e.Message);
            }
        }

        public void StopServer()
        {
            tcpListener.Stop();
        }

        /// <summary>
        /// Get a message object from a given client
        /// </summary>
        /// <param name="socket">Client to listen to</param>
        /// <returns>Deserialized Message object</returns>
        public Message getMessage(Socket socket)
        {
            Console.WriteLine("## TCPServer Receiving a message");

            try
            {
                NetworkStream strm = new NetworkStream(socket);
                IFormatter formatter = new BinaryFormatter();
                Message message = (Message)formatter.Deserialize(strm);
                Console.WriteLine("- message header: " + message.Head);
                return message;
            }
            catch (SerializationException e)
            {
                Console.WriteLine("TCPClient SendMessage exception: " + e.Message);
            }
            catch (IOException e)
            {
                Console.WriteLine("TCPClient SendMessage exception: " + e.Message);
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine("TCPClient SendMessage exception: " + e.Message);
            }

            return null;
        }

        /// <summary>
        /// Send a message to a given client
        /// </summary>
        /// <param name="message">Message to send</param>
        /// <param name="socket">Client to send the message to</param>
        public void SendMessage(Message message, Socket socket)
        {
            Console.WriteLine("## TCPServer Sending a message: " + message.Head);

            try
            {
                IFormatter formatter = new BinaryFormatter();
                NetworkStream strm = new NetworkStream(socket);
                formatter.Serialize(strm, message);
            }
            catch (SerializationException e)
            {
                Console.WriteLine("TCPClient SendMessage exception: " + e.Message);
            }
            catch (IOException e)
            {
                Console.WriteLine("TCPClient SendMessage exception: " + e.Message);
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine("TCPClient SendMessage exception: " + e.Message);
            }
        }

        /// <summary>
        /// Listen for new clients and create a new Session for each new client with its own TcpClient instance
        /// Since AcceptTcpClient is blocking, this is run in a thread
        /// </summary>
        public TcpClient Listen()
        {

                try
                {
                    Console.WriteLine("Waiting for a new connection...");
                    TcpClient client = this.tcpListener.AcceptTcpClient();

                    return client;
                }
                catch (SocketException)
                {
                    // Here we catch a WSACancelBlockingCall exception because this.tcpListener is probably closed
                    Console.WriteLine("Listener thread closed");
                }

            return null;
        }
    }
}
