using ChatService.Application.Services;
using ChatService.Domain;
using System.Net;


TcpClientServices client = new TcpClientServices();

try
{
    client.ConnectServer(IPAddress.Parse("127.0.0.1"), 23122);
    Console.WriteLine("Client Connected");
    client.RunCient();
}
catch (Exception error)
{
    Console.WriteLine("Client failure : " + error.Message);

}

/*
Message messageRegister = new Message(Message.Header.REGISTER);
messageRegister.addData("DefaultUser2");
messageRegister.addData("Password");

client.SendMessage(messageRegister);

Message register = client.GetMessage();

if (register == null)
{
    Console.WriteLine("Server failure when registering" );
}

if (register.MessageList.First() == "success")
{
    Console.WriteLine("Registration success. You can now login using your credentials");
}
else if (register.MessageList.First() == "error")
{
    Console.WriteLine("Could not register");
}
*/

var inputuser = "";

while (true)
{
    Console.WriteLine("ENTER USER (DefaultUser1 or DefaultUser2) : ");
    inputuser = Console.ReadLine();
    if (inputuser.Equals("DefaultUser1", StringComparison.OrdinalIgnoreCase) || inputuser.Equals("DefaultUser2", StringComparison.OrdinalIgnoreCase))
    {
        break;
    }
}


Message messageJoin = new Message(Message.Header.JOIN);
messageJoin.addData(inputuser);
messageJoin.addData("Password");
client.SendMessage(messageJoin);

Message reply = client.GetMessage();

if (reply == null)
{
    Console.WriteLine("server failure : ");
}


/*
Message chatroomsMessage = new Message(Message.Header.CREATE_CR);
chatroomsMessage.addData("DefaultChatRoom");

client.SendMessage(chatroomsMessage);

reply = client.GetMessage();

if (reply == null)
{
    Console.WriteLine("server failure create chat room: ");
}

if (reply.MessageList.First() == "error")
{
    Console.WriteLine("Could not create chatroom");
}
*/

User user = client.GetUser();
user.Chatroom= new Chatroom("DefaultChatRoom");

Message joinCr = new Message(Message.Header.JOIN_CR);
joinCr.addData("DefaultChatRoom");
client.SendMessage(joinCr);


/*
Message messageToSend = new Message(Message.Header.POST);
messageToSend.addData("TEST MESSAGE");
client.SendMessage(messageToSend);

Thread.Sleep(51);
*/


Thread processData = new Thread(() => HelperMethod());
processData.Start();


//HelperMethod();



void HelperMethod()
{
    while (true)
    {
        Console.WriteLine("ENTER MESSAGE");
        var input = Console.ReadLine();
        if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
        {
            break;
        }

        Message messageToSend = new Message(Message.Header.POST);
        messageToSend.addData(input);
        client.SendMessage(messageToSend);
    }
}
