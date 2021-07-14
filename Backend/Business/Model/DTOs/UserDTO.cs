using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Middleware.Model.DTOs
{
    public class UserDTO
    {
        public string id { get; set; }
        public string last { get; set; }
        public string first { get; set; }
        public string email { get; set; }
        //public List<string> groups { get; set; }
        public bool enabled { get; set; }

        public override bool Equals(object obj)
        {
            if(obj is UserDTO)
                return id.ToLower() == ((UserDTO)obj).id.ToLower();
            return false;
        }

        public override int GetHashCode()
        {
            return id.GetHashCode();
        }
    }
}
