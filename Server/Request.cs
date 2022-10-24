using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Server
{
    internal class Request
    {
        public string? Method { get; set; }
        public string? Path { get; set; }
        public string? Date { get; set; }
        public string? Body { get; set; }


        //ErrorMessages 
        private List<string> Errors = new List<string>(10);

        //check if method is valid - add error messages if not
        private void isMethodValid()
        {
            string[] validMethods = { "create", "read", "update", "delete", "echo" };
            bool isMethodMissing = string.IsNullOrEmpty(Method);

            if (isMethodMissing) 
            {
                Errors.Add("missing method");
                return;
            }

            if (!isMethodMissing)
            {
                foreach (string method in validMethods)
                {
                    //method is valid
                    if (string.Equals(method, Method))
                    {
                        return;
                    }

                    //method is invalid
                    if (!string.Equals(method, Method))
                    {
                        Errors.Add("illegal method");
                        return;
                    }
                }
            }

        }

        //check if path is valid - add error messages if not
        private void isPathValid()
        {
            bool isPathMissing = string.IsNullOrEmpty(Path);

            if (Method == "echo")
            {
                return;
            }

            if (isPathMissing)
            {
                Errors.Add("missing resource");
                return;
            }

            if (!isPathMissing)
            {
                //accept testing paths
                if (Path.StartsWith("testing"))
                {
                    return;
                }

                //reject paths without the /api/categories format
                if (!Path.StartsWith("/api/categories"))
                {
                    Errors.Add("4 Bad Request");
                    return;
                }

                int freq = Path.Count(f => f == '/');
                string pCid = Path.Remove(0, 15);
                bool isCidInt = int.TryParse(pCid, out int result);

                //reject paths without id when method is delete or update
                if (freq < 3 && Method == "delete" || freq < 3 && Method == "update")
                {
                    Errors.Add("4 Bad Request");
                    return;
                }

                if (freq == 3 && !isCidInt)
                {
                    Errors.Add("illegal resource");
                    return;

                }

                if (freq == 3 && isCidInt && Method == "create")
                {
                    Errors.Add("4 Bad Request");
                    return;
                }


            }





        }

        public Response status4Check()
        {
            Response response = new Response();
            response.Status = "";

            isMethodValid();
            isPathValid();
            

            foreach (string error in Errors)
            {
                //Console.WriteLine(error);
                response.Status += error;
            }

            return response;
        }

    }
}
