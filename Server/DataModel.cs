using System.Text.Json;

namespace Server
{
    internal class DataModel
    {
     
        public DataModel(List<Category> categories)
        {
            dataModel = categories;
        }
        public List<Category> dataModel { get; set; }

        //read method - error because 
        public Response read(string path)
        {
            var response = new Response();
            int freq = path.Count(f => f == '/');

            if (freq == 3)
            {
                string pCid = path.Remove(0, 16);
                var requested = dataModel.Find(x => x.Cid == pCid);
                if (requested != null)
                {
                    response.Status = "1 Ok";
                    response.Body = requested.Name;
                }
                else
                {
                    response.Status = "5 Not Found";
                }
            }
            else if (freq == 2)
            {
                response.Status = "1 Ok";
                var toJson = JsonSerializer.Serialize(dataModel);
                response.Body = toJson;
            }

            return response;

        }

        //haven't gotten further
        public Response create(string path)
        {
            var response = new Response();

            //int createCid = dataModel.Capacity + 1;
         

            return response;
        }


    }

}
