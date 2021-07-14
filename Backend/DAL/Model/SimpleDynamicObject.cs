using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace com.timmons.cognitive.API.Model
{
    public class SimpleDynamicObject { 
    
        public int id { get; set; }
        public bool isSpatial { get; set; }
        //HAX
        public int objectId {
            get
            {
                return id;
            }
            set
            {
                this.id = value;
            }
        }
        public DynamicObjectType type { get; set; }
        public string typeKey {
            get
            {
                if (this.type != null)
                {
                    return type.key;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (this.type == null)
                {
                    this.type = new DynamicObjectType();
                }
                this.type.key = value;
            }
        }
        public string name { get; set; }
        public string objectName {
            get
            {
                return name;
            }
            set
            {
                this.name = value;
            }
        }
    }
}
