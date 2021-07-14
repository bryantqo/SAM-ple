using System;
using System.Collections.Generic;
using System.Text;

namespace com.timmons.cognitive.API.Model
{
    public class AppUser
    {
        public Guid id { get; set; }
        public string name { get; set; }
        public long created { get; set; }
        public long modified { get; set; }
        public User createdBy { get; set; }
        public User lastModifiedBy { get; set; }
        public DynamicObjectType type { get; set; }
        public AppUserData fields { get; set; }
    }

    public class AppUserData
    {
        public string familyName { get; set; }
        public string givenName { get; set; }
        public string email { get; set; }
        public string created { get; set; }
        public string modified { get; set; }
        public string status { get; set; }
        public List<int> organizations { get; set; }
    }
}
