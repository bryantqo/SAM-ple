using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Util
{
    public class ValidationResult<E>
    {
        public bool valid { get; set; }
        public E result { get; set; }
        public List<String> messages { get; set; }
    }
}
