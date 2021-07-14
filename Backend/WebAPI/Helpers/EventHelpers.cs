using com.timmons.Stitch.Shared;
using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Helpers
{
    public class EventHelpers
    {
        public static Guid getStreamForID(IConnection con, int id, int modelid)
        {
            Guid? res = GetCorrelatedStream(con, id, modelid);

            if (res == null)
            {
                res = CorrelateStreamToObject(con, id, modelid, Guid.NewGuid());
            }

            return res.GetValueOrDefault();
        }

        public static Guid? GetCorrelatedStream(IConnection con, int objectid, int modelid)
        {
            var sql = @"SELECT streamid
FROM pam_3_0_0.streamid_objectid_correlation
WHERE objectid = @objectid
AND modelid = @modelid
LIMIT 1;";
            var res = con.Connection.Query<Guid?>(sql, new { objectid, modelid });
            return res.FirstOrDefault();
        }

        public static Guid CorrelateStreamToObject(IConnection con, int objectid, int modelid, Guid newId)
        {
            var sql = @"INSERT INTO pam_3_0_0.streamid_objectid_correlation
    (
        streamid,
        objectid,
        modelid
    )
VALUES
    (
        @streamid,
        @objectid,
        @modelid
    )
";
            con.Connection.Execute(sql, new { objectid, modelid, streamid = newId });

            return newId;
        }
    }
}
