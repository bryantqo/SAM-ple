using System.Collections.Generic;

namespace API.Middleware.Model.DTOs.Layout
{
    public class LayoutFieldDTO
    {
        public int id { get; set; }
        public bool? required { get; set; }
        public bool? readOnly { get; set; }
        public bool? hidden { get; set; }
        public string label { get; set; }
        public string path { get; set; }
        public List<string>? help { get; set; }
        public Newtonsoft.Json.Linq.JObject viewMap { get; set; }
        public List<Newtonsoft.Json.Linq.JObject>? conditions { get; set; }
        public string noValue { get; set; }
    }
}
