using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    internal class Request
    {
        public string? Method { get; set; }
        public string? Path { get; set; }
        public string? Date { get; set; }
        public string? Body { get; set; }

        /*
        public bool IsMethodValid(string tempMethod)
        {
            bool ifValid = false;
            string[] validMethods = { "create", "read", "update", "delete", "echo" };
            foreach (string method in validMethods)
            {
                if (string.Equals(tempMethod, method)) { ifValid = true; break; }
            }
            return ifValid;
        }

        public bool IsCidValid(string path, List<Category> allCids)
        {
            bool validCid = false;
            string pCid = path.Remove(0, 15);
            foreach (var cid in allCids)
            {
                if (string.Equals(cid.Cid, pCid))
                {
                    validCid = true;
                    break;
                }

            }
            return validCid;
        }

        static bool IsPathValid(string path, List<Category> allCids)
        {
            bool ifValid = false;
            bool isPathMissing = string.IsNullOrEmpty(path);

            //check if cid is valid
            bool validCid = false;
            string pCid = path.Remove(0, 15);
            foreach (var cid in allCids)
            {
                if (string.Equals(cid.Cid, pCid))
                {
                    validCid = true;
                    break;
                }
            }

            //checks if path is valid
            if (!isPathMissing)
            {
                if (string.Equals(path, "/api/categories")
                    || path.StartsWith("testing")
                    || path.StartsWith("/api/categories/") && validCid
                   )
                { ifValid = true; }
            }
            return ifValid;
        }
        */



    }

    

}
