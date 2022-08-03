using System.Reflection;
using ChatService.Application.Services;

string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

/*
if (File.Exists(dir + @"\chatrooms.db"))
    File.Delete(dir + @"\chatrooms.db");

if (File.Exists(dir + @"\users.db"))
    File.Delete(dir + @"\users.db");
*/


TcpServerServices server = new TcpServerServices();

server.StartServer(23122); // Start the server

if (server.IsRunning())
{
    server.RunServer();
}
else
{
    Console.WriteLine("Server failure, Connection error");
}

