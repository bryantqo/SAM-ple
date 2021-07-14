using Model = com.timmons.cognitive.API.Model.Event;
using com.timmons.Stitch.Shared;
using Dapper;
using System;
using System.Data.Common;
using System.Threading.Tasks;

namespace com.timmons.cognitive.API.DAL.Event
{
    public class EventDAL
    {
        public static async Task<Model.Event.EventEntry> write(ConnectionHelper<DbConnection> Connection, Model.Event.Event evt, Guid who)
        {
            var sql = @"
INSERT INTO pam_3_0_0.events
    ( who, streamid, type, data )
VALUES
    ( @who, @streamid, @type, @data::jsonb )
returning 
    id,
    ""date"",
    who, 
    type, 
    streamid,
    data as ""dataRaw""
                   "; 
            using(var con = Connection.GetConnection())
            {
                var ret = await con.QuerySingleAsync<Model.Event.EventEntry>(sql, new { who, type = evt.getType(), streamid = evt.getStreamID(), data = Newtonsoft.Json.Linq.JObject.FromObject(evt) });
                return ret;
            }
        }

        public static async Task<Model.Event.EventEntry> read<E>(ConnectionHelper<DbConnection> Connection, int id)
        {
            var sql = @"
SELECT
    id,
    ""date"",
    who, 
    type, 
    data as ""dataRaw""
FROM pam_3_0_0.events
WHERE id = @id";

            using (var con = Connection.GetConnection())
            {
                var ret = await con.QuerySingleAsync<Model.Event.EventEntry>(sql, new { id });
                return ret;
            }
        }
    }
}
