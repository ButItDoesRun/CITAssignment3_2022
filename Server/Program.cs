using Microsoft.VisualBasic;
using Server;
using System.Net;
using System.Net.Sockets;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

//Data Model
DataModel startDataModel = new DataModel(new List<Category>
{
    {new Category(){
            Cid = "1",
            Name = "Beverages"
        }
    },
    {new Category(){
            Cid = "2",
            Name = "Condiments"
        }
    },
    {new Category(){
            Cid = "3",
            Name = "Confections"
        }
    }
});

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

    bool isMethodMissing = string.IsNullOrEmpty(request?.Method);

    /*
    Category newCat = new Category
    {
        Cid = "1",
        Name = "Beverages"
    };
    */

    

    if (isMethodMissing)
    {
        Console.WriteLine("Method is missing");
        Response response = CreateReponse("4 Missing method");
        SendResponse(stream, response);

    }
    else if(!isMethodMissing)
    {
        Console.WriteLine("Method is present");
        if (IsMethodValid(request.Method))
        {
            Console.WriteLine("Method is valid");
            if (request.Method != "echo" && IsPathValid(request.Path))
            {
                Console.WriteLine("path is valid");

            }
            else if (request.Method != "echo" && !IsPathValid(request.Path))
            {
                Console.WriteLine("path is invalid");
                Response response = CreateReponse("4 Bad Request: missing resource");
                SendResponse(stream, response);

            }
            
           
        }
        else if(!IsMethodValid(request.Method))
        {
            
            Console.WriteLine("invalid method");
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

static bool IsPathValid(string path)
{
    bool ifValid = false;
    bool isPathMissing = string.IsNullOrEmpty(path);


    /*
    List<Category> defaultCategories = new List<Category>
    {
        {new(1, "Beverages") },
        {new (2, "Condiments")},
        {new (3, "Confections")},

    };
    */

    if (!isPathMissing)
    {
        if (path.StartsWith("/api/categories")
            || path.StartsWith("testing")
            )
        {
            ifValid = true;
        }
    }
    return ifValid;
}





/*

static Response StatusCodeCheck(Request request)
{
    var status = new (string statusCode, string reasonPhrase)[]
    {
        ("1", "Ok"), //delete, read, echo, create, update
        ("2", "Created"), //create
        ("3", "Updated"), //update
        ("4", "Bad Request"), //read, echo, delete, create, update
        ("5", "Not found"), //read, delete, create, update
        ("6", "Error") //read, echo, delete, create, update
    };
    string s = null;
    string b = request.Body;

    bool isMethodMissing = string.IsNullOrEmpty(request.Method);
    bool isPathMissing = string.IsNullOrEmpty(request.Path);
    bool isDateMissing = string.IsNullOrEmpty(request.Date);

    if (isMethodMissing || isPathMissing || isDateMissing)
    {
        s += "4 ";

        if (isMethodMissing) { s += "missing method "; }
        if (isPathMissing && request.Method != "echo") { s += "missing path "; }
        if (isDateMissing) { s += "missing date "; }
    }
    else if (!isMethodMissing)
    {
        string mV = IsMethodValid(request.Method) ? "1 OK " : "4 illegal method ";
        s += mV;


    }

    //create”, “read”, “update”, “delete”, “echo”

    foreach (var sa in status)
    {

    }



    Response res = CreateReponse(s, b);
    return res;
}

*/
