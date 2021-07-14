using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Middleware.Model.Layout
{
    public class CategoryLayout
    {
        public int categoryId { get; set; }
        public List<CategoryLayoutSection> sections { get; set; }
    }
}
