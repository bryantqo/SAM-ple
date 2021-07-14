using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Helpers
{
    public class NamedObject<E>
    {
        public E id { get; set; }
        public string name { get; set; }
    }
}
