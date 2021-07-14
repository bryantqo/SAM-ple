using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.timmons.cognitive.API.DAL.Event;
using com.timmons.cognitive.API.Model.Event;
using com.timmons.Stitch.Shared;

namespace API.Middleware.Events
{
    public interface IEventStore
    {
        Task<EventEntry> Append(Event evt, Guid who);
    }

    public class PAMEventStore : IEventStore
    {
        private readonly IConnection con;
        public PAMEventStore(IConnection con)
        {
            this.con = con;
        }

        public async Task<EventEntry> Append(Event evt, Guid who)
        {
            var res = await EventDAL.write(con.Wrap(), evt, who);
            return res;
        }
    }
}
