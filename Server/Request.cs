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

        //Default datamodel
        public List<Category> allCategories = new List<Category>
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
        };

        //ErrorMessages 
        private List<string> Errors = new List<string>(10);
        private string cid { get; set; }

        //check if method is valid - add error messages if not
        private void isMethodValid()
        {
            bool isMethodMissing = string.IsNullOrEmpty(Method);

            if (isMethodMissing) 
            {
                Errors.Add("missing method ");
                return;
            }

            if (!isMethodMissing)
            {
                if (Method == "create"
                    || Method == "read"
                    || Method == "update"
                    || Method == "delete" 
                    || Method == "echo" ) 
                {
                    return;
                }
                Errors.Add("illegal method ");
                return;
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
                Errors.Add("missing resource ");
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
                    Errors.Add("4 Bad Request ");
                    return;
                }

                int freq = Path.Count(f => f == '/');
                string pCid = Path.Remove(0, 15);
                //get cid
                cid = pCid;
                bool isCidInt = int.TryParse(pCid, out int result);

                //reject paths without id when method is delete or update
                if (freq < 3 && Method == "delete" || freq < 3 && Method == "update")
                {
                    Errors.Add("4 Bad Request ");
                    return;
                }

                if (freq == 3 && !isCidInt)
                {
                    Errors.Add("illegal resource ");
                    return;

                }

                if (freq == 3 && isCidInt && Method == "create")
                {
                    Errors.Add("4 Bad Request ");
                    return;
                }
            }
        }

        private void isDateValid()
        {
            bool isDateMissing = string.IsNullOrEmpty(Date);
            long currentDate = DateTimeOffset.Now.ToUnixTimeSeconds();
            bool isDate = Int64.TryParse(Date, out long requestDate);

            Console.WriteLine(isDate);
            if (isDateMissing)
            {
                Errors.Add("missing date ");
                return;
            }

            if (!isDateMissing && isDate)
            {
                Console.WriteLine(currentDate);
                long subtractedDates = currentDate - requestDate;
                Console.WriteLine(subtractedDates);
                if (subtractedDates < 0)
                {
                    Errors.Add("illegal date ");
                    return;
                }
            }else if (!isDateMissing && !isDate)
            {
                Errors.Add("illegal date ");
                return;
            }
            
        }

        private void isBodyValid()
        { 
            bool isBodyMissing = string.IsNullOrEmpty(Body);

            if (Method == "read" || isBodyMissing && Method == "delete")
            {
                return; 
            }

            if (isBodyMissing && Method == "echo"
                || isBodyMissing && Method == "create"
                || isBodyMissing && Method == "update")
            {
                Errors.Add("missing body ");
                return;
            }

       
            if (!isBodyMissing && Method == "create")
            {
                if (!Body.Contains("name:"))
                {
                    Errors.Add("illegal body ");
                    return;
                }
                
            }

            if (!isBodyMissing && Method == "update")
            {
                Category newCat = bodyToCategory();
                if (!string.IsNullOrEmpty(newCat.Cid) && !string.IsNullOrEmpty(newCat.Name))
                {
                    foreach (var catCid in allCategories)
                    {
                        if (catCid.Cid != cid)
                        {
                            Errors.Add("illegal body ");
                            return;
                        }
                    }
                }
            }
        }

        public Response status4Check()
        {
            Response response = new Response();
            response.Status = "";

            isMethodValid();
            isPathValid();
            isDateValid();
            isBodyValid();



            foreach (string error in Errors)
            {
                //Console.WriteLine(error);
                response.Status += error;
            }

            return response;
        }
        private Category bodyToCategory()
        {
            var newCategory = new Category { Cid = "", Name = "" };

            var charToReplace = '"';
            string charToString = charToReplace.ToString();


            if (Body.Contains("cid") && Body.Contains("name"))
            {
                Body = Body.Replace("{", "");
                Body = Body.Replace("}", "");
                Body = Body.Replace(charToString, "");
                var pathSplit = Body.Split(",");

                foreach (string element in pathSplit)
                {
                    if (element.Contains("cid:"))
                    {
                        string gotCid = element.Remove(0, 4);
                        //Console.WriteLine(gotCid);
                        newCategory.Cid = gotCid;
                    }
                    if (element.Contains("name:"))
                    {
                        string gotName = element.Remove(0, 5);
                        //Console.WriteLine(gotName);
                        newCategory.Name = gotName;
                    }
                }

            }
            else if (!Body.Contains("cid") && Body.Contains("name"))
            {
                Body = Body.Replace("{", "");
                Body = Body.Replace("}", "");
                Body = Body.Replace(charToString, "");
                var pathSplit = Body.Split(":");

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

    }
}
