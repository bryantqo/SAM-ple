using System.Collections.Generic;

using PAM_Model = com.timmons.cognitive.API.Model;

namespace API.Middleware.Model.Layout
{
    public class LayoutField
    {
        public bool? readOnly { get; set; }
        public bool? hidden { get; set; }
        public bool? required { get; set; }
        public string label { get; set; }
        public string path { get; set; }
        public PAM_Model.Field model { get; set; }
        public PAM_Model.FieldType type { get; set; }
        public List<string> help { get; set; }
        public Newtonsoft.Json.Linq.JObject viewMap { get; set; }
        public List<Newtonsoft.Json.Linq.JObject> conditions { get; set; }
        public string noValue { get; set; }
    }
}