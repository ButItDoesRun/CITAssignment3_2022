using Server;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

Console.WriteLine("Hello, World!");

var server = new TcpListener(IPAddress.Loopback, 5000);
server.Start();
Console.WriteLine("Server started...");

while (true)
{
    var client = server.AcceptTcpClient();
    Console.WriteLine("Client connected...");

    try
    {
        HandleClient(client);
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
        Console.WriteLine("Unable to communicate with client...");
    }

}

static void HandleClient(TcpClient client)
{
    var stream = client.GetStream();

    var buffer = new byte[2048];

    var rcnt = stream.Read(buffer);

    var requestText = Encoding.UTF8.GetString(buffer, 0, rcnt);
    Console.WriteLine(requestText);

    var request = JsonSerializer.Deserialize<Request>(requestText);

    
    if (string.IsNullOrEmpty(request?.Method))
    {
        Response response = CreateReponse("4 Missing method");
        SendResponse(stream, response);

    }
    else if(!string.IsNullOrEmpty(request.Method))
    {
        Console.WriteLine("I got to here");
        if (IsMethodValid(request.Method))
        {
            Console.WriteLine("Method is valid");
            Response response = CreateReponse("1  ok");
            SendResponse(stream, response);
        }
        else if(!IsMethodValid(request.Method))
        {
            Console.WriteLine("I got to here2");
            Response response = CreateReponse("4  illegal method", request.Body);
            SendResponse(stream, response);
            
        }

    }
    

    stream.Close();
}



static void SendResponse(NetworkStream stream, Response response)
{
    var responseText = JsonSerializer.Serialize<Response>(response);
    var responseBuffer = Encoding.UTF8.GetBytes(responseText);
    stream.Write(responseBuffer);
}

static Response CreateReponse(string status, string body = "")
{
    return new Response
    {
        Status = status,
        Body = body
    };
}




static bool IsMethodValid(string tempMethod)
{
    bool ifValid = false;
    string[] validMethods = { "create", "read", "update", "delete", "echo" };
    foreach (string method in validMethods)
    {
        if (string.Equals(tempMethod, method)) { ifValid = true; break;}
    }
    return ifValid;
}





