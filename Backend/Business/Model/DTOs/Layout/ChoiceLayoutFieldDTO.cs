using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Middleware.Model.DTOs.Layout
{
    public class ChoiceLayoutFieldDTO : LayoutFieldDTO
    {
        public List<int> possibleChoices { get; set; }
    }
}
