using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace com.timmons.cognitive.API.Model
{
    public class ShortDynamicObject
    {
        public int id { get; set; }
        public string name { get; set; }
        public Guid createdBy { get; set; }
        public long created { get; set; }
    }

    public class DynamicObject 
    { 
    
        public static DynamicObject NewFromType(DynamicObjectType t, User by)
        {
            return new DynamicObject { type = t, createdBy = by, lastModifiedBy = by };
        }

        public int id { get; set; }
        public string name { get; set; }

        [JsonIgnore]
        public DynamicObjectType type { get; set; }

        public User createdBy { get; set; }
        public User lastModifiedBy { get; set; }

        [JsonIgnore]
        public bool deleted { get; set; }
        public JObject fields { get; set; }
        public long created { get; set; }
        public long modified { get; set; }
    }
}
