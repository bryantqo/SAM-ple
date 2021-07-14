using com.timmons.Stitch.Shared;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using PAM_API = com.timmons.cognitive.API.DAL;

using API.Middleware.Events;
using API.Middleware.Events.Foo;

namespace API.Controllers.api
{
    [Route("api/Foo/[action]")]
    [Route("Foo/[action]")]
    [ApiController]
    public class InstrumentController : ControllerBase
    {
        private readonly IConnection con;

        private readonly IEventStore eventStore;

        public InstrumentController(IConnection con, IEventStore eventStore)
        {
            this.con = con;
            this.eventStore = eventStore;

        }


        public async Task<int> AddSpatial([FromBody] JObject req)
        {
            //Get your actual user id or whatever
            var uid = Guid.Empty;

            var evt = new FooGeometryAddedEvent(req["geometry"] as JObject, req["properties"] as JObject, req.Value<string>("type"));
            var e = await eventStore.Append(evt, uid);

            var ret = await PAM_API.SpatialRepo.Put(this.con.Wrap(), uid, 1, 1, 1, req, "foo");
            
            return ret.id;
        }
    }
}
