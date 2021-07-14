using PAM_Model = com.timmons.cognitive.API.Model;
using PAM_API = com.timmons.cognitive.API.DAL;

using com.timmons.Stitch.Shared;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;


namespace API.Middleware.Helpers
{
    /*
     * This class provides a lookup bridge for PAM dynamic object 
     * It provides a lookup by id or name
     */
    public class LookyLoo : DynamicObjectLookup
    {
        private readonly int objectType;
        private readonly PAM_Model.DynamicObjectModelNew model;
        private readonly ConnectionHelper<DbConnection> con;

        public LookyLoo(ConnectionHelper<DbConnection> con, int objectType)
        {
            this.con = con;
            this.objectType = objectType;

            var res = PAM_API.DynamicObjectModelRepo.Get(con, objectType);
            res.Wait();

            model = res.Result;

        }

        /*
         * This will provide a lookyloo based on the passed in object type ID
         * Any lookups on the returned lookyloo will be bound to that object type
         */
        public static LookyLoo tryGetFor(ConnectionHelper<DbConnection> con, int objectType)
        {
            return new LookyLoo(con, objectType);
        }





        /*
         * This will return all the object references that match a the requested lookup ids
         */
        public IAsyncEnumerable<PAM_Model.DynamicObject> Get(IEnumerable<int> lookup)
        {
            return PAM_API.DynamicObjectRepo.GetLight(con, lookup.ToList(), this.model);
        }

        /*
         * This will return all the object references that match a the requested lookup names
         */
        public IAsyncEnumerable<PAM_Model.DynamicObject> Get(IEnumerable<string> lookup)
        {
            return PAM_API.DynamicObjectRepo.GetLight(con, lookup.ToList(), this.model);
        }

        /*
         * This will return a single object reference that matchs the requested lookup id
         */
        public async Task<PAM_Model.DynamicObject> GetSingle(int lookup)
        {
            await foreach (var o in PAM_API.DynamicObjectRepo.GetLight(con, new List<int> { lookup }, this.model))
            {
                if (!o.deleted)
                    return o;

            }

            return null;
        }

        /*
         * This will return a single object reference that matchs the requested lookup name
         */
        public async Task<PAM_Model.DynamicObject> GetSingle(string lookup)
        {
            await foreach (var o in PAM_API.DynamicObjectRepo.GetLight(con, new List<string> { lookup }, this.model))
            {
                if (!o.deleted)
                    return o;
            }

            return null;
        }



        /*
         * The below are an overload of the above lookups but return objects for when we dont care about the data we are getting back
         */

        IAsyncEnumerable<object> Lookupable<object, int>.Get(IEnumerable<int> lookup)
        {
            return Get(lookup);
        }

        IAsyncEnumerable<object> Lookupable<object, string>.Get(IEnumerable<string> lookup)
        {
            return Get(lookup);
        }

        async Task<object> Lookupable<object, int>.GetSingle(int lookup)
        {
            return await GetSingle(lookup);
        }

        async Task<object> Lookupable<object, string>.GetSingle(string lookup)
        {
            return await GetSingle(lookup);
        }
    }


    // TODO: Our abstraction should be an interface that combines the 
    // Lookupable<PAM_Model.DynamicObject, int>, Lookupable<PAM_Model.DynamicObject, String>, Lookupable<object, int>, Lookupable<object, String>
    // implementations

    public interface DynamicObjectLookup : Lookupable<PAM_Model.DynamicObject, int>, Lookupable<PAM_Model.DynamicObject, String>, Lookupable<object, int>, Lookupable<object, String>
    { }

    public class LookyLooLookup : Lookupable<DynamicObjectLookup, int>
    {
        private readonly ConnectionHelper<DbConnection> con;


        /*
         * This class is kinda an abstraction hell but we will get through it soon
         * So if a LookyLoo Looks Up Objects By ID then the Looky Loo Lookup Looks Up Looky Loos By the object Type
         * Yea like I said kinda abstraction hell
         * Think of it this way
         * We dont know the object we are trying to find so we have the above LookyLoo class that handles that for us, cool
         * But we dont know the lookup that we need for every type either
         * So therefore we need a lookup for the lookup because the types are also dynamic
         */
        public LookyLooLookup(ConnectionHelper<DbConnection> con)
        {
            this.con = con;
        }

        async IAsyncEnumerable<DynamicObjectLookup> Lookupable<DynamicObjectLookup, int>.Get(IEnumerable<int> lookup)
        {
            foreach (var li in lookup)
                yield return LookyLoo.tryGetFor(con, li);

            yield break;
        }

        async Task<DynamicObjectLookup> Lookupable<DynamicObjectLookup, int>.GetSingle(int lookup)
        {
            return LookyLoo.tryGetFor(con, lookup);
        }
    }
}
