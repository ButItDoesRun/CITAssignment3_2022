

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
                    Errors.Add("4 Bad Request");
                    return;
                }

                //check if path has a cid, and save cid
                int freq = Path.Count(f => f == '/');
                bool isCidInt = false;
                if (freq >= 3)
                {
                    string pCid = Path.Remove(0, 16); 
                    //save cid
                    cid = pCid;
                    isCidInt = int.TryParse(pCid, out int result);
                }

                //reject paths without id when method is delete or update
                if (freq < 3 && Method == "delete" || freq < 3 && Method == "update")
                {
                    Errors.Add("4 Bad Request");
                    return;
                }

                //reject paths with invalid cid
                if (freq == 3 && !isCidInt)
                {
                    Errors.Add("4 Bad Request");
                    return;

                }

                //reject paths with cid if method 'create'
                if (freq == 3 && isCidInt && Method == "create")
                {
                    Errors.Add("4 Bad Request ");
                    return;
                }
            }
        }

        //check if date is valid - add error messages if not
        private void isDateValid()
        {
            bool isDateMissing = string.IsNullOrEmpty(Date);
            long currentDate = DateTimeOffset.Now.ToUnixTimeSeconds();
            bool isDate = Int64.TryParse(Date, out long requestDate);

            if (isDateMissing)
            {
                Errors.Add("missing date ");
                return;
            }

            //check if valid date stamp is an actual unix stamp
            if (!isDateMissing && isDate)
            {
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

        //check if body is valid - add error messages if not
        private void isBodyValid()
        { 
            bool isBodyMissing = string.IsNullOrEmpty(Body);

            //body is unnecessary for method "read" & "delete"
            if (Method == "read" || isBodyMissing && Method == "delete")
            {
                return; 
            }

            //check if body missing
            if (isBodyMissing && Method == "echo"
                || isBodyMissing && Method == "create"
                || isBodyMissing && Method == "update")
            {
                Errors.Add("missing body ");
                return;
            }

            //check if body is illegal for method "create"
            if (!isBodyMissing && Method == "create")
            {
                if (!Body.Contains("name:"))
                {
                    Errors.Add("illegal body ");
                    return;
                }
                
            }

            //check if body is illegal for method "update"
            if (!isBodyMissing && Method == "update")
            {
                //Json objects will always be encapsulated in {}
                if (!Body.Contains("{ }"))
                {
                    Errors.Add("illegal body ");
                    return;
                }

                //check if cid in body is valid 
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

        //check for status 4 requests and append error messages
        public Response status4Check()
        {
            Response response = new Response();
            response.Status = "";

            //check request parameters using error append methods
            isMethodValid();
            isPathValid();
            isDateValid();
            isBodyValid();


            //add error messages to status
            foreach (string error in Errors)
            {
                response.Status += error;
            }

            //handle create/update status 4
            String badR = "4 Bad Request";
            if (response.Status.Contains(badR) && Method == "create"
                || response.Status.Contains(badR) && Method == "update")
            {
                response.Status = badR;
                response.Body = null;
            }else{ response.Body = Body; }

            return response;
        }

        //this is a big work around to transform the body parameter in string format into an object
        //instead of using Serializer.Serialize<Category>(category) as that wouldn't work for me

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
