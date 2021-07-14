using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Middleware.Events
{
    public interface Projection <A,B>
    {
        B Apply(A evt, B source);
    }
}
