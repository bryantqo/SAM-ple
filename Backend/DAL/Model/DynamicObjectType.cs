using System;
using System.Collections.Generic;
using System.Text;

namespace com.timmons.cognitive.API.Model
{
    public class DynamicObjectType
    {
        public int id { get; set; }
        public string key { get; set; }
        public string singular_name { get; set; }
        public string plural_name { get; set; }
        public bool display { get; set; }
        public int display_order { get; set; }
        public int? extensionOf { get; set; }
    }
}
