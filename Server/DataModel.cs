using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Server
{
    internal class DataModel
    {
        public List<Category> dataModel { get; set; }
        public DataModel(List<Category> categories)
        {
            dataModel = categories;
        }

        public Response read(string path)
        {
            var response = new Response();
            int freq = path.Count(f => f == '/');

            if (freq == 3)
            {
                string pCid = path.Remove(0, 15);
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
            else if (freq < 3)
            {
                response.Status = "1 Ok";
                var allListElements = JsonSerializer.Serialize(dataModel);
                response.Body = allListElements;
            }

            return response;
        }
    }

    
}
