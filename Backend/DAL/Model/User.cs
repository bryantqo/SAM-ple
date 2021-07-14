using System;
using System.Collections.Generic;
using System.Text;

namespace com.timmons.cognitive.API.Model
{
    public class User
    {
        public Guid id { get; set; }
        public string name { get {
                var r = "";
                
                if (firstName != null)
                    r += firstName + " ";

                if (lastName != null)
                    r += lastName;

                if (firstName == null && lastName == null && email != null)
                    r = email;

                return r.Trim();
            } }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }

    }
}
