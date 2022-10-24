﻿using Microsoft.VisualBasic;
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

    //client start
    var stream = client.GetStream();

    var buffer = new byte[2048];

    var rcnt = stream.Read(buffer);

    var requestText = Encoding.UTF8.GetString(buffer, 0, rcnt);
    Console.WriteLine(requestText);

    var request = JsonSerializer.Deserialize<Request>(requestText);

    bool isBadRequest = checkForStatus4(request, allCategories);

    if (isBadRequest)
    {
        //return error messenges
        Response response = handleStatus4Response(request);
        //Console.WriteLine(response.Status);
        //Console.WriteLine(response.Body);
        SendResponse(stream, response);
    }
    else if (!isBadRequest)
    {
        //do datamodel methods
        string m = request.Method;
        string status = "";

        Console.WriteLine("request valid");
        //execute echo method
        if (m.Equals("echo"))
        {
            status += "1 Ok";
        }

        Response response = CreateReponse(status, request.Body);
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
    int freq = path.Count(f => f == '/');
    if (freq == 3)
    {
        string pCid = path.Remove(0, 15);
        foreach (var cid in allCategories)
        {
            if (string.Equals(cid, pCid))
            {
                validCid = true;
                break;
            }

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

static bool IsBodyValid(Request r, List<Category> allCategories)
{
    string m = r.Method;
    string body = r.Body;
    bool isValid = false;
    bool isBodyMissing = string.IsNullOrEmpty(body);

    if (!isBodyMissing)
    {
        if (m == "create")
        {
            if(body.Contains("name:")){ isValid = true;}

        }

        if (m == "update")
        {
            Category newCat = bodyToCategory(body);
            if (!string.IsNullOrEmpty(newCat.Cid) && !string.IsNullOrEmpty(newCat.Name))
            {
                foreach (var cid in allCategories)
                {
                    if (cid.Cid.Equals(newCat.Cid))
                    {
                        isValid = true;
                    }
                }

            }
        }
       
    }

    return isValid;
}

static Category bodyToCategory(string body)
{
    var newCategory = new Category{Cid ="", Name =""};

    var charToReplace = '"';
    string charToString = charToReplace.ToString();


    if (body.Contains("cid") && body.Contains("name"))
    {
        body = body.Replace("{", "");
        body = body.Replace("}", "");
        body = body.Replace(charToString, "");
        var pathSplit = body.Split(","); 
        
        foreach (string element in pathSplit) 
        { 
            if (element.Contains("cid:")) 
            { 
                string gotCid = element.Remove(0, 4); 
                Console.WriteLine(gotCid);
                newCategory.Cid = gotCid;
            } 
            if (element.Contains("name:")) 
            { 
                string gotName = element.Remove(0, 5); 
                Console.WriteLine(gotName); 
                newCategory.Name = gotName;
            }
        }
        
    }else if (!body.Contains("cid") && body.Contains("name"))
    {
        body = body.Replace("{", "");
        body = body.Replace("}", "");
        body = body.Replace(charToString, "");
        var pathSplit = body.Split(":");

        foreach (string element in pathSplit)
        {
           if (!element.Contains("name"))
           {
                string gotName = element.Remove(0, 4);
                Console.WriteLine(gotName);
                newCategory.Name = gotName;
                newCategory.Cid = "noCid";
}
        }
    }

    return newCategory;
}


static bool checkForStatus4(Request request, List<Category> allCategories)
{
    bool badRequest = false;
    string m = request.Method;

    bool method = IsMethodValid(m);
    bool path = IsPathValid(request.Path, allCategories);
    bool date = IsDateValid(request.Date);
    bool body = IsBodyValid(request, allCategories);
    bool isBodyMissing = string.IsNullOrEmpty(request.Body);
    

    if (!method)
    {
        badRequest = true;
    }else if (method)
    {
        if (m.Equals("create"))
        {
            Console.Write(m + ": create");
            Console.Write("i get here");
            if (!path || !date || !body) { badRequest = true; }
            else if (path && request.Path != "/api/categories" && date && body)
            {
                badRequest = true;
            }
        }

        if (m.Equals( "update"))
        {
            if (!path || !date || !body) { badRequest = true; }
            else if (path && !IsCidValid(request.Path, allCategories) && date && body)
            {
                badRequest = true;
            }
        }

        if (m.Equals("read")
            || m.Equals("delete"))
        {
            if (!path || !date) { badRequest = true; }
            else if (path
                     && !IsCidValid(request.Path, allCategories)
                     && date)
            {
                badRequest = true;
            }
        }

        if (m.Equals("echo"))
        {
            if (!date || isBodyMissing) { badRequest = true; }
        }
    }

 
    return badRequest;
}

static Response handleStatus4Response(Request request)
{
    
    string m = request.Method;
    string s = "4 Bad Request : ";
    bool isMethodMissing = string.IsNullOrEmpty(m);
    bool isPathMissing = string.IsNullOrEmpty(request.Path);
    bool isDateMissing = string.IsNullOrEmpty(request.Date);
    bool isBodyMissing = string.IsNullOrEmpty(request.Body);
    bool date = IsDateValid(request.Date);


    //method error message
    if (isMethodMissing)
    {
        s += "missing method, ";
        //date error message
        if (isDateMissing)
        { s += "missing date, "; }
        else if (!date)
        { s += "illegal date, "; }

        //Only check for missing as illegal cannot be determined without method
        //path error message
        if (isPathMissing)
        { s += "missing resource, "; }
        //body error message
        if (isBodyMissing)
        { s += "missing body, "; }

    }
    else if(!isMethodMissing) { s += "illegal method, "; }

    if (!isMethodMissing)
    {
        //path error message
        if (isPathMissing && !m.Equals("echo"))
        {
            s += "missing resource, ";
        }
        else
        {
            s += "illegal resource, ";
        }

        //date error message
        if (isDateMissing)
        {
            s += "missing date, ";
        }
        else if (!isDateMissing)
        {
            s += "illegal date, ";
        }

        //body error message
        if (isBodyMissing && m.Equals("create")
            || isBodyMissing && m.Equals("update"))
        {
            s += "missing body, ";
        }
        else
        {
            s += "illegal body, ";
        }

        if(isBodyMissing && m.Equals("echo")){ s += "missing body, "; }
    }

    Response response = CreateReponse(s, request.Body);
    return response;
}

