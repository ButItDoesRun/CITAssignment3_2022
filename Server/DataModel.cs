using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

    }

    
}
