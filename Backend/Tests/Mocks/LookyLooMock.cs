using API.Middleware.Helpers;
using com.timmons.cognitive.API.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Model = com.timmons.cognitive.API.Model;

namespace Tests.Mocks
{
    public class LookyLooMock : DynamicObjectLookup
    {
        private List<Model.DynamicObject> mockObjects = new List<DynamicObject>();


        public async IAsyncEnumerable<Model.DynamicObject> Get(IEnumerable<string> lookup)
        {
            foreach (var itm in (from i in mockObjects where lookup.Contains(i.name) select i))
            {
                yield return itm;
            }
        }

        public async IAsyncEnumerable<Model.DynamicObject> Get(IEnumerable<int> lookup)
        {
            foreach(var itm in (from i in mockObjects where lookup.Contains(i.id) select i))
            {
                yield return itm;
            }
        }

        public async Task<Model.DynamicObject> GetSingle(string lookup)
        {
            return (from i in mockObjects where i.name == lookup select i).FirstOrDefault();
        }

        public async Task<Model.DynamicObject> GetSingle(int lookup)
        {
            return (from i in mockObjects where i.id == lookup select i).FirstOrDefault();
        }

        internal void registerMockObject(DynamicObject mockObject)
        {
            mockObjects.Add(mockObject);
        }

        async IAsyncEnumerable<DynamicObject> Lookupable<DynamicObject, int>.Get(IEnumerable<int> lookup)
        {
            foreach (var itm in (from i in mockObjects where lookup.Contains(i.id) select i))
            {
                yield return itm;
            }
        }

        async IAsyncEnumerable<DynamicObject> Lookupable<DynamicObject, string>.Get(IEnumerable<string> lookup)
        {
            foreach (var itm in (from i in mockObjects where lookup.Contains(i.name) select i))
            {
                yield return itm;
            }
        }

        async IAsyncEnumerable<object> Lookupable<object, int>.Get(IEnumerable<int> lookup)
        {
            foreach (var itm in (from i in mockObjects where lookup.Contains(i.id) select i))
            {
                yield return itm;
            }
        }

        async IAsyncEnumerable<object> Lookupable<object, string>.Get(IEnumerable<string> lookup)
        {
            foreach (var itm in (from i in mockObjects where lookup.Contains(i.name) select i))
            {
                yield return itm;
            }
        }

        async Task<DynamicObject> Lookupable<DynamicObject, int>.GetSingle(int lookup)
        {
            return (from i in mockObjects where i.id == lookup select i).FirstOrDefault();
        }

        async Task<DynamicObject> Lookupable<DynamicObject, string>.GetSingle(string lookup)
        {
            return (from i in mockObjects where i.name == lookup select i).FirstOrDefault();
        }

        async Task<object> Lookupable<object, int>.GetSingle(int lookup)
        {
            return (from i in mockObjects where i.id == lookup select i).FirstOrDefault();
        }

        async Task<object> Lookupable<object, string>.GetSingle(string lookup)
        {
            return (from i in mockObjects where i.name == lookup select i).FirstOrDefault();
        }
    }

    public static class LookyLooMockExt
    {

        public static LookyLooMock WithMockObject(this LookyLooMock me, Model.DynamicObject mockObject)
        {
            me.registerMockObject(mockObject);
            return me;
        }

        public static LookyLooMock WithMockObjects(this LookyLooMock me, IEnumerable<Model.DynamicObject> mockObjects)
        {
            foreach(var mockObject in mockObjects)
                me.registerMockObject(mockObject);

            return me;
        }

    }
}
