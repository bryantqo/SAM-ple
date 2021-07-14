using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Middleware.Model.DTOs.Layout
{
    public class LayoutSectionDTO
    {
        public int id { get; set; }
        public bool enabled { get; set; }
        public int categoryid { get; set; }
        public int order { get; set; }
        public string name { get; set; }
        public bool editable { get; set; }
        public JArray layout { get; set; }
    }
}
