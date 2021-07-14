using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Middleware.Model.Layout
{
    public class CategoryLayoutSection
    {
        public bool enabled { get; set; }
        public int order { get; set; }
        public string name { get; set; }
        public bool editable { get; set; }
        public List<LayoutField> layout { get; set; }
    }
}
