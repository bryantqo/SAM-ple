using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Middleware.Helpers
{
    public interface Lookupable<E, X>
    {
        IAsyncEnumerable<E> Get(IEnumerable<X> lookup);
        Task<E> GetSingle(X lookup);

    }
}
