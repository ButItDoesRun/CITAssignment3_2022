using Microsoft.VisualBasic;
using Server;
using System.Collections;
using System.Collections.Generic;
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

static void HandleClient(TcpClient client)
{
    //Data Model
    List<Category> allCategories = new List<Category>
    {
        {
            new Category()
            {
                Cid = "1",
                Name = "Beverages"
            }
        },
        {
            new Category()
            {
                Cid = "2",
                Name = "Condiments"
            }
        },
        {
            new Category()
            {
                Cid = "3",
                Name = "Confections"
            }
        }
    };

    DataModel Model = new DataModel(allCategories);

    /*
    //get all cids into a static list
    List<string> allCids = new List<string>();
    foreach (var element in allCategories)
    {
        allCids.Add(element.Cid);
        Console.WriteLine(element.Cid + ": " + element.Name);

    }
    String[] cids = allCids.ToArray();

    Console.WriteLine("and we try again");
    foreach (string c in cids)
    {
        Console.WriteLine(c);
    }
    */

    //client start
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

    /*
    if (request.Date != null)
    {
        Console.Write(request.Date);
    }

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
            if (request.Method != "echo" && IsPathValid(request.Path, allCategories))
            {
                Console.WriteLine("path is valid");

            }
            else if (request.Method != "echo" && !IsPathValid(request.Path, allCategories))
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

    }*/
    if (IsDateValid(request.Date))
    {
        Console.WriteLine("date method works!");
        Response response = CreateReponse("1 ok", request.Body);
        SendResponse(stream, response);
    }else if (!IsDateValid(request.Date))
    {
        Console.WriteLine("date method works2!");
        Response response = CreateReponse("missing date", request.Body);
        SendResponse(stream, response);
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


static bool IsCidValid(string path, List<Category> allCategories)
{
    bool validCid = false;
    string pCid = path.Remove(0, 15);
    foreach (var cid in allCategories)
    {
        if (string.Equals(cid, pCid))
        {
            validCid = true;
            break;
        }

    }
    return validCid;
}


static bool IsPathValid(string path, List<Category> allCategories)
{
    bool ifValid = false;
    bool isPathMissing = string.IsNullOrEmpty(path);

    //checks if path is valid
    if (!isPathMissing)
    {
        if (string.Equals(path, "/api/categories")
            || path.StartsWith("testing")
            || path.StartsWith("/api/categories/") && IsCidValid(path, allCategories)
            )
        { ifValid = true;}
    }
    return ifValid;
}


static bool IsDateValid(string date)
{
    bool isValid = false;
    bool isDateMissing = string.IsNullOrEmpty(date);
    long currentDate = DateTimeOffset.Now.ToUnixTimeSeconds();
    bool isDate = Int64.TryParse(date, out long requestDate);

    if (!isDateMissing && isDate)
    {
        long subtractedDates = currentDate - requestDate;
        if (subtractedDates >= 0)
        {
            isValid = true;
        }

    }

    return isValid;
}

/*
var status = new (string statusCode, string reasonPhrase)[]
{
    ("1", "Ok"), //delete, read, echo, create, update
    ("2", "Created"), //create
    ("3", "Updated"), //update
    ("4", "Bad Request"), //read, echo, delete, create, update
    ("5", "Not found"), //read, delete, create, update
    ("6", "Error") //read, echo, delete, create, update
};

static Response StatusCodeCheck(Request request)
{
    
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
