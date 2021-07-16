using com.timmons.Stitch.Shared;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using PAM_API = com.timmons.cognitive.API.DAL;

using API.Middleware.Events;
using API.Middleware.Events.Foo;
using System.Collections.Generic;

namespace API.Controllers.api
{
    [Route("api/Foo/[action]")]
    [Route("Foo/[action]")]
    [ApiController]
    public class InstrumentController : ControllerBase
    {
        static List<JObject> allItems = new List<JObject>();

        public InstrumentController()
        {

        }

        [ActionName("GetAll")]
        public List<JObject> GetAll()
        {
            return allItems;
        }

        [ActionName("AddSpatial")]
        public async Task<int> AddSpatial([FromBody] JObject req)
        {
            //Get your actual user id or whatever
            var uid = Guid.Empty;

            var evt = new FooGeometryAddedEvent(req["geometry"] as JObject, req["properties"] as JObject, req.Value<string>("type"));
            allItems.Add(req);
            //var e = await eventStore.Append(evt, uid);

            //var ret = await PAM_API.SpatialRepo.Put(this.con.Wrap(), uid, 1, 1, 1, req, "foo");

            return 1; // ret.id;
        }
    }
}
