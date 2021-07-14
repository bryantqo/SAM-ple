using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using com.timmons.cognitive.API.DAL.Event;
using com.timmons.cognitive.API.Model.Event;
using com.timmons.PAM.API.Model;
using com.timmons.Stitch.Shared;
using Dapper;
using Newtonsoft.Json.Linq;

namespace com.timmons.cognitive.API.DAL
{
    public class SpatialRepo
    {
        public static void Delete(ConnectionHelper<DbConnection> con, int id, List<int> spatialFields, string typeKey)
        {
            String sql = "DELETE FROM pam_2_0_0.shapes_$_$_KEY_$_$ WHERE objectid = @objectid and fieldid = @fieldid";
            sql = sql.Replace("$_$_KEY_$_$", typeKey);
            //Delete from spatial where object id and field id match
            using (var conn = con.GetConnection())
                foreach (var field in spatialFields)
                    conn.Execute(sql, new { objectid = id, fieldid = field });
        }

        public static async Task<Spat> Put(ConnectionHelper<DbConnection> con, Guid who, int objectid, int objecttypeid, int fieldid, JToken geojson, string typeKey)
        {

            JObject metadata = null;
            JObject geometry = null;

            if (geojson.Type == JTokenType.Object)
            {
                var asO = geojson as JObject;
                if (asO.ContainsKey("geometry"))
                    geojson = asO["geometry"];
                if (asO.ContainsKey("properties"))
                    metadata = asO["properties"] as JObject;

                geometry = geojson as JObject;
            }
            // Events!
            // We are doing this outside here now
            // This should continue to be the destination driven by the event though

            /*var addedEvent = new ShapeAdded
            {
                fieldId = fieldid,
                geometry = geometry,
                objectId = objectid,
                objectTypeId = objecttypeid,
                properties = metadata
            };

            var eventRes = EventDAL.write(con, addedEvent, who);*/

            //Below is the PAM way of doing things, we are moving to the event way above
            String sql = @"
INSERT INTO pam_2_0_0.shapes_$_$_KEY_$_$ 
    ( objectid
    , fieldid
    , shape
    , metadata
    ) 
VALUES
    ( @objectid
    , @fieldid
    , ST_SetSRID(ST_MakeValid(ST_GeomFromGeoJSON(@geojson)), 3857)
    , @metadata::jsonb
    ) 
RETURNING id, ST_AsGeoJSON(ST_Envelope(shape))::character varying as geojson";
            sql = sql.Replace("$_$_KEY_$_$", typeKey);

            var res = (await con.GetConnection().QueryAsync<SpatDTO>(sql, new { objectid, fieldid, geojson, metadata }
            )).First();

            var parsed = JObject.Parse(res.geojson);

            return new Spat(res.id, parsed);
        }

        public static Spatial Add(ConnectionHelper<DbConnection> con, int id1, int id2, Spatial spatialValue, string typeKey)
        {
            String sql = @"
INSERT INTO pam_2_0_0.shapes_$_$_KEY_$_$ 
    ( objectid
    , fieldid
    , shape
    , acres
    ) 
VALUES
    ( @objectid
    , @fieldid
    , ST_SetSRID(ST_MakeValid(@shape), 3857)
    , @acres
    ) 
RETURNING id, ST_AsText(ST_Envelope(shape))::character varying as wkt, acres, 'bbox' as ""type""";
            sql = sql.Replace("$_$_KEY_$_$", typeKey);

            //This should return a bounding box once intersted so ST_Envelope
            return con.GetConnection().QuerySingle<Spatial>(sql, new { objectid = id1, fieldid = id2, shape = spatialValue.wkt, acres = spatialValue.acres });
        }

        public static IEnumerable<Spatial> GetByObjectId(ConnectionHelper<DbConnection> con, int id, int field, string typeKey)
        {
            String sql = "SELECT id, ST_AsText(shape)::character varying as wkt, acres, 'exact' as \"type\" FROM pam_2_0_0.shapes_$_$_KEY_$_$ WHERE objectid = @objectid and fieldid = @fieldid";
            sql = sql.Replace("$_$_KEY_$_$", typeKey);

            return con.GetConnection().Query<Spatial>(sql, new { objectid = id, fieldid = field });
        }
        public static IEnumerable<Spatial> GetById(ConnectionHelper<DbConnection> con, int id, string typeKey)
        {
            String sql = "SELECT id, ST_AsText(shape)::character varying as wkt, acres, 'exact' as \"type\" FROM pam_2_0_0.shapes_$_$_KEY_$_$ WHERE id = @id";
            sql = sql.Replace("$_$_KEY_$_$", typeKey);

            return con.GetConnection().Query<Spatial>(sql, new { id });
        }

        public static IEnumerable<JObject> GetGeoJSONByObjectId(ConnectionHelper<DbConnection> con, int id, int field, string typeKey)
        {
            String sql = "SELECT ST_AsGeoJSON(shape) FROM pam_2_0_0.shapes_$_$_KEY_$_$ WHERE objectid = @id";
            sql = sql.Replace("$_$_KEY_$_$", typeKey);

            return con.GetConnection().Query<JObject>(sql, new { id });
        }
    }

    public enum SpatialFormat
    {
        WKT,
        GeoJSON
    }
}
