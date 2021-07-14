using API.Middleware.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Mocks
{

    /*
     * This class emulates looking up looky loos from the database
     * In testing cases we dont always want to actually have a database in place so instead we will fake the data a database may contain
     */
    public class LookyLooLookupMock : Lookupable<DynamicObjectLookup, int>
    {

        Dictionary<int, DynamicObjectLookup> MockRepo = new Dictionary<int, DynamicObjectLookup>();

        public LookyLooLookupMock()
        {
            //TODO: Populate the repo
            var lla = new LookyLooMock().WithMockObjects(FakeObjects.FakeObjectsTypeA);
            var llb = new LookyLooMock().WithMockObjects(FakeObjects.FakeObjectsTypeB);

            MockRepo.Add(1, lla);
            MockRepo.Add(2, llb);
        }

        async IAsyncEnumerable<DynamicObjectLookup> Lookupable<DynamicObjectLookup, int>.Get(IEnumerable<int> lookup)
        {
            foreach (var id in lookup)
            {
                if (MockRepo.ContainsKey(id))
                    yield return MockRepo[id];
            }

            yield break;
        }

        async Task<DynamicObjectLookup> Lookupable<DynamicObjectLookup, int>.GetSingle(int lookup)
        {
            if (MockRepo.ContainsKey(lookup))
                return MockRepo[lookup];

            return null;
        }
    }
}
