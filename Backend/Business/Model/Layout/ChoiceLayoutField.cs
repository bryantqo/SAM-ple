using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Middleware.Model.Layout
{
    public class ChoiceLayoutField : LayoutField
    {
        public List<DTOs.ChoiceDTO> possibleChoices { get; set; }
    }
}
