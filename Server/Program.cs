using Server;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;


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

void HandleClient(TcpClient client)
{
   //client start
    var stream = client.GetStream();
    //get request
    var buffer = new byte[2048];
    var rcnt = stream.Read(buffer);
    var requestText = Encoding.UTF8.GetString(buffer, 0, rcnt);
    var request = JsonSerializer.Deserialize<Request>(requestText);

    //Data Model
    DataModel Model = new DataModel(request.allCategories);

    //create new response
    Response response = CreateReponse("", "");
    
    //check for status 4 requests, and save errors
    response = request.status4Check();

    //condition for handling of requests
    bool noBadRequest = string.IsNullOrEmpty(response.Status);

    //if request valid
    if (noBadRequest)
    {
        string m = request.Method;
        string p = request.Path;

        //Execute read method
        if (m.Equals("read"))
        {
            response = Model.read(p);
            Console.WriteLine(response.Status);
            Console.WriteLine(response.Body);
        }

        //Execute read method
        if (m.Equals("create"))
        {
            response = Model.create(p);
            Console.WriteLine(response.Status);
        }

        //Execute echo method
        if (m.Equals("echo"))
        {
            response = CreateReponse("1 Ok", request.Body);
        }
    }


    SendResponse(stream, response);
    stream.Close();
}



static void SendResponse(NetworkStream stream, Response response)
{
    var responseText = JsonSerializer.Serialize<Response>(response);
    var responseBuffer = Encoding.UTF8.GetBytes(responseText);
    Console.WriteLine(responseText);
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

