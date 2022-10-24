using Microsoft.VisualBasic;
using Server;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Net;
using System.Net.Sockets;
using System.Security.Claims;
using System.Security.Cryptography;
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
    /*
    //Data Model
    List<Category> allCategories = new List<Category>
    {
        {
            new()
            {
                Cid = "1",
                Name = "Beverages"
            }
        },
        {
            new()
            {
                Cid = "2",
                Name = "Condiments"
            }
        },
        {
            new()
            {
                Cid = "3",
                Name = "Confections"
            }
        }
    };*/

    //DataModel Model = new DataModel(request.allCategories);

    //client start
    var stream = client.GetStream();

    var buffer = new byte[2048];

    var rcnt = stream.Read(buffer);

    var requestText = Encoding.UTF8.GetString(buffer, 0, rcnt);
    Console.WriteLine(requestText);

    var request = JsonSerializer.Deserialize<Request>(requestText);

    DataModel Model = new DataModel(request.allCategories);

    Response response = CreateReponse("", "");
    
    response = request.status4Check();

    bool noBadRequest = string.IsNullOrEmpty(response.Status);

    if (noBadRequest)
    {
        //valid request
        string m = request.Method;
        string p = request.Path;

        //Execute read method
        if (m.Equals("read"))
        {
            response = Model.read(p);
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

