using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Middleware.Model.DTOs.Layout
{
    public class ClusterLayoutFieldDTO : LayoutFieldDTO
    {
        public JToken layout { get; set; }
    }
}
