using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.timmons.cognitive.API.Model;
using com.timmons.cognitive.API.Util;
using com.timmons.Stitch.Shared;
using Dapper;
using Newtonsoft.Json.Linq;

namespace com.timmons.cognitive.API.DAL
{
    public class DynamicObjectRepo
    {

        public static IEnumerable<ObjectReferenceField> getRelationalFields(IEnumerable<Tuple<FieldType, Field>> fields)
        {
            var r =  from fieldTuple in fields where fieldTuple.Item1 == FieldType.ObjectReference select fieldTuple.Item2 as ObjectReferenceField;
            return from i in r where i != null select i;
        }


        public static async Task<DynamicObject> GetSingle(ConnectionHelper<DbConnection> Connection, int id, DynamicObjectModelNew model, string schema = "pam", bool withSpatial = false)
        {

            var sql =
                @"SELECT 
	                o.id as id, 
	                o.name as name,
	                o.deleted as deleted, 
	                o.fields || ( case 
		                WHEN s.shape IS NOT NULL THEN
			                jsonb_build_object(CONCAT(s.""field_key""), ST_AsGeoJSON(s.shape)::jsonb)
                         ELSE
                            '{}'::jsonb
                         END
	                )
                    as fields,
	                CAST(EXTRACT(EPOCH FROM o.created) AS bigint) as created,
	                CAST(EXTRACT(EPOCH FROM o.modified) AS bigint) as modified,
	                o.""createdBy"" as id, 
                    o.""lastModifiedBy"" as id
                FROM $_$_SCHEMA_$_$.objects_$_$_KEY_$_$ as o
                LEFT JOIN
                    (
                        SELECT
                            DISTINCT ON (objectid, fieldid) id, objectid, fieldid, ""key"" as ""field_key"", shape
                         FROM $_$_SCHEMA_$_$.shapes_$_$_KEY_$_$ 
						LEFT JOIN
                        (SELECT ""key"",""value"" FROM

                            jsonb_each(
                                (
                                    SELECT model

                                    FROM pam.model

                                    WHERE ""key"" = '$_$_KEY_$_$'
                                )
							) 
						) as m

                        on fieldid = (m.""value""->> 'id')::integer

                        ORDER BY objectid ASC, fieldid ASC, id DESC
                    ) as s
                ON o.id = s.objectid
                WHERE o.id = @id
                    AND o.deleted = false 
                LIMIT 1;";

            if (!withSpatial)
                sql =
                @"SELECT 
	                o.id as id, 
	                o.name as name,
	                o.deleted as deleted, 
	                o.fields as fields,
	                CAST(EXTRACT(EPOCH FROM o.created) AS bigint ) as created,
	                CAST(EXTRACT(EPOCH FROM o.modified) AS bigint ) as modified,
	                o.""createdBy"" as id, 

                    o.""lastModifiedBy"" as id
                FROM $_$_SCHEMA_$_$.objects_$_$_KEY_$_$ as o
                WHERE o.id = @id
                    AND o.deleted = false
                LIMIT 1";





            sql = sql.Replace("$_$_SCHEMA_$_$", schema);
            sql = sql.Replace("$_$_KEY_$_$", model.key);

            
            return await GetSingleFrom(Connection, sql, new { id }, model);

        }



        public static async IAsyncEnumerable<DynamicObject> Get(ConnectionHelper<DbConnection> Connection, List<int> ids, DynamicObjectType objectType, bool withSpatial = false)
        {
            var model = await DynamicObjectModelRepo.Get(Connection, objectType.key);

            await foreach(var p in Get(Connection, ids, model, "pam", withSpatial))
            {
                yield return p;
            }

            yield break;
        }

        public static async IAsyncEnumerable<DynamicObject> Get(ConnectionHelper<DbConnection> Connection, List<string> ids, DynamicObjectType objectType)
        {
            var model = await DynamicObjectModelRepo.Get(Connection, objectType.key);

            await foreach (var p in Get(Connection, ids, model))
            {
                yield return p;
            }

            yield break;
        }

        public static async IAsyncEnumerable<DynamicObject> Get(ConnectionHelper<DbConnection> Connection, List<int> ids, DynamicObjectModelNew model, string schema = "pam", bool withSpatial = false)
        {

            var sql =
                @"SELECT 
	                o.id as id, 
	                o.name as name,
	                o.deleted as deleted, 
	                o.fields || ( case 
		                WHEN s.shape IS NOT NULL THEN
			                jsonb_build_object(CONCAT(s.""field_key""), ST_AsGeoJSON(s.shape)::jsonb)
                         ELSE
                            '{}'::jsonb
                         END
	                )
                    as fields,
	                CAST(EXTRACT(EPOCH FROM o.created) AS bigint) as created,
	                CAST(EXTRACT(EPOCH FROM o.modified) AS bigint) as modified,
	                o.""createdBy"" as id, 
                    o.""lastModifiedBy"" as id
                FROM $_$_SCHEMA_$_$.objects_$_$_KEY_$_$ as o
                LEFT JOIN
                    (
                        SELECT
                            DISTINCT ON (objectid, fieldid) id, objectid, fieldid, ""key"" as ""field_key"", shape
                         FROM $_$_SCHEMA_$_$.shapes_$_$_KEY_$_$ 
						LEFT JOIN
                        (SELECT ""key"",""value"" FROM

                            jsonb_each(
                                (
                                    SELECT model

                                    FROM pam.model

                                    WHERE ""key"" = '$_$_KEY_$_$'
                                )
							) 
						) as m

                        on fieldid = (m.""value""->> 'id')::integer

                        ORDER BY objectid ASC, fieldid ASC, id DESC
                    ) as s
                ON o.id = s.objectid
                WHERE o.id = ANY(@ids)
                    AND o.deleted = false; ";

            if(!withSpatial)
                sql =
                @"SELECT 
	                o.id as id, 
	                o.name as name,
	                o.deleted as deleted, 
	                o.fields as fields,
	                CAST(EXTRACT(EPOCH FROM o.created) AS bigint ) as created,
	                CAST(EXTRACT(EPOCH FROM o.modified) AS bigint ) as modified,
	                o.""createdBy"" as id, 

                    o.""lastModifiedBy"" as id
                FROM $_$_SCHEMA_$_$.objects_$_$_KEY_$_$ as o
                WHERE o.id = ANY(@ids)
                    AND o.deleted = false";





            sql = sql.Replace("$_$_SCHEMA_$_$", schema);
            sql = sql.Replace("$_$_KEY_$_$", model.key);

            await foreach (var o in GetFrom(Connection, sql, new { ids }, model))
            {
                yield return o;
            }
        }

        public static async IAsyncEnumerable<DynamicObject> Get(ConnectionHelper<DbConnection> Connection, List<string> names, DynamicObjectModelNew model, string schema = "pam")
        {

            var sql =
                @"SELECT 
                    o.id as id, 
                    o.name as name,
                    o.deleted as deleted, 
                    o.fields as fields,
                    CAST(EXTRACT(EPOCH FROM o.created) AS bigint ) as created,
                    CAST(EXTRACT(EPOCH FROM o.modified) AS bigint ) as modified,
                    o.""createdBy"" as id, 
                    o.""lastModifiedBy"" as id
                FROM $_$_SCHEMA_$_$.objects_$_$_KEY_$_$ as o
                WHERE o.name = ANY(@names)
                    AND o.deleted = false
                LIMIT 1";

            sql = sql.Replace("$_$_SCHEMA_$_$", schema);
            sql = sql.Replace("$_$_KEY_$_$", model.key);

            await foreach (var o in GetFrom(Connection, sql, new { names }, model))
            {
                yield return o;
            }
        }




        public static async IAsyncEnumerable<DynamicObject> GetLight(ConnectionHelper<DbConnection> Connection, List<int> ids, DynamicObjectModelNew model, string schema = "pam")
        {

            var sql =
                @"SELECT 
                    o.id as id, 
                    o.name as name,
                    o.deleted as deleted, 
                    CAST(EXTRACT(EPOCH FROM o.created) AS bigint ) as created,
                    CAST(EXTRACT(EPOCH FROM o.modified) AS bigint ) as modified,
                    o.""createdBy"" as id, 
                    o.""lastModifiedBy"" as id
                FROM $_$_SCHEMA_$_$.objects_$_$_KEY_$_$ as o
                WHERE o.id = ANY(@ids)
                    AND o.deleted = false
                LIMIT 1";

            sql = sql.Replace("$_$_SCHEMA_$_$", schema);
            sql = sql.Replace("$_$_KEY_$_$", model.key);

            await foreach (var o in GetFrom(Connection, sql, new { ids }, model))
            {
                yield return o;
            }
        }

        public static async IAsyncEnumerable<DynamicObject> GetLight(ConnectionHelper<DbConnection> Connection, List<string> names, DynamicObjectModelNew model, string schema = "pam")
        {

            var sql =
                @"SELECT 
                    o.id as id, 
                    o.name as name,
                    o.deleted as deleted, 
                    CAST(EXTRACT(EPOCH FROM o.created) AS bigint ) as created,
                    CAST(EXTRACT(EPOCH FROM o.modified) AS bigint ) as modified,
                    o.""createdBy"" as id, 
                    o.""lastModifiedBy"" as id
                FROM $_$_SCHEMA_$_$.objects_$_$_KEY_$_$ as o
                WHERE o.name = ANY(@names)
                    AND o.deleted = false
                LIMIT 1";

            sql = sql.Replace("$_$_SCHEMA_$_$", schema);
            sql = sql.Replace("$_$_KEY_$_$", model.key);

            await foreach(var o in GetFrom(Connection, sql, new { names }, model))
            {
                yield return o;
            }
        }


        public static async Task<bool> Exists(ConnectionHelper<DbConnection> Connection, int id, DynamicObjectType model, string schema = "pam")
        {

            var sql =
                @"SELECT 
                    true                    
                FROM $_$_SCHEMA_$_$.objects_$_$_KEY_$_$ as o
                WHERE o.id = @id
                    AND o.deleted = false
                LIMIT 1";

            sql = sql.Replace("$_$_SCHEMA_$_$", schema);
            sql = sql.Replace("$_$_KEY_$_$", model.key);

            var res = await Connection.GetConnection().QueryAsync<bool>(sql, new { id });

            return res.Any();
        }



        public static async Task<bool> Delete(ConnectionHelper<DbConnection> Connection, DynamicObjectType model, int id, Guid by, string schema = "pam")
        {

            var sql =
                @"UPDATE $_$_SCHEMA_$_$.objects_$_$_KEY_$_$
                    SET deleted = true, modified = NOW(), ""lastModifiedBy"" = @uid
                WHERE
                    id = @id";

            sql = sql.Replace("$_$_SCHEMA_$_$", schema);
            sql = sql.Replace("$_$_KEY_$_$", model.key);

            await Connection.GetConnection().ExecuteAsync(sql, new { id, uid = by });

            AuditLog audit = new AuditLog
            {
            };

            RecordAudit(Connection, model, id, audit, by, AuditType.Delete);

            return true;
        }



        public static async IAsyncEnumerable<DynamicObject> GetFrom(ConnectionHelper<DbConnection> Connection, string sql, object param, DynamicObjectType objectType)
        {
            var model = await DynamicObjectModelRepo.Get(Connection, objectType.key);

            await foreach (var p in GetFrom(Connection, sql, param, model))
            {
                yield return p;
            }

            yield break;
        }


        public static async IAsyncEnumerable<DynamicObject> GetFrom(ConnectionHelper<DbConnection> Connection, string sql, object param, DynamicObjectModelNew model, string schema = "pam")
        {
            var collection = await Connection.GetConnection().QueryAsync<DynamicObject, User, User, DynamicObject>(sql, (obj, createdBy, modifiedBy) =>
            {
                obj.createdBy = createdBy;
                obj.lastModifiedBy = modifiedBy;
                return obj;
            }, param);

            if (collection == null)
                yield break;

            foreach (var itm in collection)
            {
                yield return itm;
            }

            yield break;
        }

        public static async Task<DynamicObject> GetSingleFrom(ConnectionHelper<DbConnection> Connection, string sql, object param, DynamicObjectModelNew model, string schema = "pam")
        {
            var collection = await Connection.GetConnection().QueryAsync<DynamicObject, User, User, DynamicObject>(sql, (obj, createdBy, modifiedBy) =>
            {
                obj.createdBy = createdBy;
                obj.lastModifiedBy = modifiedBy;
                return obj;
            }, param);

            return collection.First();
        }


        public static async Task<IEnumerable<DynamicObject>> GetAll(ConnectionHelper<DbConnection> Connection, DynamicObjectType objectType)
        {
            var model = await DynamicObjectModelRepo.Get(Connection, objectType.key);

            return await GetAll(Connection, model);
        }

        public static async Task<IEnumerable<DynamicObject>> GetAll(ConnectionHelper<DbConnection> Connection, DynamicObjectModelNew model, string schema = "pam")
        {

            var sql =
                @"SELECT 
                    o.id as id, 
                    o.name as name,
                    o.deleted as deleted, 
                    o.fields as fields,
                    CAST(EXTRACT(EPOCH FROM o.created) AS bigint ) as created,
                    CAST(EXTRACT(EPOCH FROM o.modified) AS bigint ) as modified,
                    o.""createdBy"" as id, 
                    o.""lastModifiedBy"" as id
                FROM $_$_SCHEMA_$_$.objects_$_$_KEY_$_$ as o
                WHERE o.deleted = false
                LIMIT 1000";

            sql = sql.Replace("$_$_SCHEMA_$_$", schema);
            sql = sql.Replace("$_$_KEY_$_$", model.key);

            var collection = await Connection.GetConnection().QueryAsync<DynamicObject, User, User, DynamicObject>(sql, (obj, createdBy, modifiedBy) =>
            {
                obj.createdBy = createdBy;
                obj.lastModifiedBy = modifiedBy;
                return obj;
            });

            if (collection == null)
                return null;



            IEnumerable<Field> relationalFields = getRelationalFields(model.fields);

            return collection;
            //return TransformRelationalFieldsFromDatabase(Connection, schema, new List<DynamicObject> { collection }, relationalFields.ToList()).FirstOrDefault();
        }

        private static void RecordAudit(ConnectionHelper<DbConnection> Connection, DynamicObjectType type, int objectid, AuditLog audit, Guid by, AuditType auditType = AuditType.Update, string schema = "pam")
        {
            String sql =
                @"INSERT INTO  $_$_SCHEMA_$_$.audit_$_$_KEY_$_$
                    ( objectid
                    , logtype
                    , ""by""
                    , log
                    )
                VALUES
                    ( @objectid
                    , @logtype
                    , @by
                    , CAST(@log as jsonb)
                    )
                ";

            

            sql = sql.Replace("$_$_SCHEMA_$_$", schema);
            sql = sql.Replace("$_$_KEY_$_$", type.key);


            var parm = new 
            { 
                objectid = objectid,
                logtype = auditType,
                by = by,
                log = JObject.FromObject(audit)
            };

            Connection.GetConnection().ExecuteAsync(sql, parm);

        }


        public static async Task<DynamicObject> Update(ConnectionHelper<DbConnection> Connection, DynamicObject updateObject, 
            Guid who, UpdateMode mode = UpdateMode.Replace, string schema = "pam", bool updateLinks = true)
        {
            //TODO: Start recording update events so the update action can be replayed

            var model = await DynamicObjectModelRepo.Get(Connection, updateObject.type.key);

            String sql = 
                @"UPDATE $_$_SCHEMA_$_$.objects_$_$_KEY_$_$
	            SET name=@name
	                , ""lastModifiedBy""=@lastModifiedById
	                , fields = CAST(@fields as jsonb)
	                , modified = NOW()
                WHERE id = @id
                RETURNING id";

            
            sql = sql.Replace("$_$_KEY_$_$", model.key);
            sql = sql.Replace("$_$_SCHEMA_$_$", schema);


            Model.DynamicObject oldObjectValues = updateObject;

            //Start a new audit record
            UpdateAuditLog audit = new UpdateAuditLog
            {
                changes = new List<UpdateAuditFields>()
            };

            //If we are doing an overlay we need to have a copy of the old data so that we can merge the data
            if (mode == UpdateMode.Overlay)
            {
                oldObjectValues = await GetSingle(Connection, updateObject.id, model);
            }

            Model.DynamicObject transformed = updateObject;

            IEnumerable<ObjectReferenceField> relationalFields = getRelationalFields(model.fields);
            IEnumerable<Field> spatialFields = from fieldTuple in model.fields where fieldTuple.Item1 == FieldType.Spatial select fieldTuple.Item2;

            
            //Transform spatial fields

            foreach (var field in spatialFields)
            {
                await updateSpatial(Connection, who, updateObject, field, model);
            }

            //Get all of the relational fields where we actually need to sync data
            //TODO: we should actually do this in a relation table
            var linktoFields = (from f in relationalFields where f.syncFieldId != null select f).ToList();

            if (updateLinks && linktoFields.Any())
            {
                await updateObjectLinks(Connection, updateObject, oldObjectValues, who, linktoFields);
            }

            if (mode == UpdateMode.Overlay)
            {
                var rec = overlay(transformed, oldObjectValues);

                transformed = rec.Item1;
                if(rec.Item2.changes != null)
                    audit.changes.AddRange(rec.Item2.changes);
            }

            if (mode == UpdateMode.Replace)
            {
                transformed = updateObject;

                foreach (var field in updateObject.fields)
                {
                    var change = new UpdateAuditFields
                    {
                        field = field.Key,
                        newValue = field.Value
                    };

                    audit.changes.Add(change);
                }
            }


            if (transformed == null)
                return null;


            var dyn = new DynamicParameters(transformed);
            dyn.Add("@typeID", transformed.type.id);
            dyn.Add("@lastModifiedByID", who);

            int id = Connection.GetConnection().QuerySingle<int>(sql, dyn);

            

            RecordAudit(Connection, model, updateObject.id, audit, updateObject.lastModifiedBy.id);

            DynamicObject res = await GetSingle(Connection, id, model, schema, true);

            return res;
        }

        private static async Task updateSpatial(ConnectionHelper<DbConnection> Connection, Guid who, DynamicObject updateObject, Field field, DynamicObjectModelNew model)
        {
            if (updateObject.fields.ContainsKey(field.name))
            {
                var geom = updateObject.fields[field.name];
                if (geom.Type == JTokenType.Null)
                    return;

                var tup = await SpatialRepo.Put(Connection, who, updateObject.id, model.id, field.id, geom, model.key);
                updateObject.fields[field.name] = JObject.FromObject(tup);
            }
        }

        private static async Task updateObjectLinks(ConnectionHelper<DbConnection> Connection, DynamicObject updateObject, DynamicObject oldObjectValues,
            Guid who, List<ObjectReferenceField> linktoFields)
        {
            var modelCache = new Dictionary<int, DynamicObjectModelNew>();

            foreach (var fld in linktoFields)
            {
                //Does the object contain the relational field?
                if (updateObject.fields.ContainsKey(fld.name))
                {
                    //Does the model cache contain the model related to the relational field
                    if (!modelCache.ContainsKey(fld.objectTypeId))
                    {
                        var relModel = await DynamicObjectModelRepo.Get(Connection, fld.objectTypeId);
                        modelCache[fld.objectTypeId] = relModel;
                    }

                    var mod = modelCache[fld.objectTypeId];


                    var targetVal = updateObject.fields.SelectToken(fld.name);
                    var oldVal = oldObjectValues.fields.SelectToken(fld.name);

                    var linkToIDs = new List<int>();
                    List<int> unlinkIDs = new List<int>();

                    var targetField = (from i in mod.fields where i.Item2.id == fld.syncFieldId select i.Item2 as ObjectReferenceField).FirstOrDefault();
                    if (targetField == null)
                        targetField = new ObjectReferenceField { name = "UNKNOW_FIELD_" + fld.syncFieldId };


                    List<int> linked = new List<int>();
                    List<int> oldLinked = new List<int>();


                    if (oldVal != null && oldVal.Type == JTokenType.Array)
                    {
                        oldLinked.AddRange(from i in oldVal select i.Value<int>("id"));
                    }
                    else if (oldVal != null && oldVal.Type == JTokenType.Object)
                    {
                        oldLinked.Add(oldVal.Value<int>("id"));
                    }

                    if (targetVal.Type == JTokenType.Array)
                    {
                        linkToIDs.AddRange(from i in targetVal select i.Value<int>("id"));
                        linked.AddRange(from i in targetVal select i.Value<int>("id"));
                    }
                    else if (targetVal.Type == JTokenType.Object)
                    {
                        linkToIDs.Add(targetVal.Value<int>("id"));
                        linked.Add(targetVal.Value<int>("id"));
                    }
                    else if (targetVal.Type == JTokenType.Null)
                    {
                        linked = new List<int>();
                    }

                    //Find all the IDs that we were linked to but now are not
                    unlinkIDs = oldLinked.Where(i => !linked.Contains(i)).ToList();

                    foreach (var unlink in unlinkIDs)
                    {
                        await unlinkFrom(Connection, who, updateObject.id, unlink, mod, targetField);
                    }

                    foreach (var targetId in linkToIDs)
                    {
                        await linkTo(Connection, who, updateObject.id, targetId, mod, targetField);
                    }
                }
            }
        }

        private static async Task unlinkFrom(ConnectionHelper<DbConnection> Connection, Guid who, int fromId, int toId, DynamicObjectModelNew toModel, ObjectReferenceField targetField)
        {
            DynamicObject targetObject = await GetSingle(Connection, toId, toModel);
            var newVal = new { id = fromId };

            if (targetObject == null)
                return;


            var oldValue = targetObject.fields.SelectToken(targetField.name);

            if (oldValue != null && oldValue.Type == JTokenType.Array)
            {
                var tArr = (oldValue as JArray);

                //Keep all the old values, just not the one we are unlinking from
                var nArr = JArray.FromObject(from i in tArr where i.Value<int>("id") != fromId select i);

                oldValue = nArr;
            }

            else if (oldValue != null && oldValue.Type == JTokenType.Object && targetField.multiple.GetValueOrDefault(false))
            {
                if(oldValue.Value<int>("id") == fromId)
                {
                    oldValue = null; // JObject.FromObject(null);
                }
            }

            else
            {
                oldValue = null; // JObject.FromObject(null);
            }

            var nf = new Dictionary<string, object>();


            //TODO: Get the actual field name here
            nf[targetField.name] = oldValue;

            targetObject.fields = JObject.FromObject(nf);
            targetObject.type = toModel;

            //Pass in update links = false so we dont get stuck in a loop
            await Update(Connection, targetObject, who, UpdateMode.Overlay, "pam", false);
        }

        private static async Task linkTo(ConnectionHelper<DbConnection> Connection, Guid who, int fromId, int toId, DynamicObjectModelNew toModel, ObjectReferenceField targetField)
        {
            DynamicObject targetObject = await GetSingle(Connection, toId, toModel);
            var newVal = new { id = fromId };

            if (targetObject == null)
                return;

            var oldValue = targetObject.fields.SelectToken(targetField.name);

            var values = new List<int>();

            if (oldValue != null && oldValue.Type == JTokenType.Array)
            {
                values = (from i in oldValue select i.Value<int>("id")).ToList();
            }

            else if (oldValue != null && oldValue.Type == JTokenType.Object) // && targetField.multiple.GetValueOrDefault(false))
            {
                values.Add(oldValue.Value<int>("id"));
            }

            values.Add(fromId);

            var nf = new Dictionary<string, object>();

            if (!targetField.multiple.GetValueOrDefault(false))
                oldValue = JObject.FromObject(new { id = values.Last() });
            else
                oldValue = JArray.FromObject(from i in values select new { id = i });

            //TODO: Get the actual field name here
            nf[targetField.name] = oldValue;

            targetObject.fields = JObject.FromObject(nf);
            targetObject.type = toModel;

            //Pass in update links = false so we dont get stuck in a loop
            await Update(Connection, targetObject, who, UpdateMode.Overlay, "pam", false);
        }

        private static Tuple<DynamicObject, UpdateAuditLog> overlay(DynamicObject target, DynamicObject onto)
        {
            var transformed = onto;
            var newObject = target;
            var oldObject = onto;

            var audit = new UpdateAuditLog();

            transformed.type = newObject.type;

            foreach (var field in newObject.fields)
            {
                var oldValue = oldObject.fields[field.Key];
                var newValue = field.Value;

                if (oldValue == null && newValue == null)
                    continue;

                if (oldValue != null && newValue != null && JToken.DeepEquals(oldValue, newValue))
                    continue;


                if (oldValue != null && oldValue.Type == JTokenType.Object && newValue.Type == JTokenType.Object)
                {
                    var ovo = (oldValue as JObject);
                    var nvo = (newValue as JObject);

                    var diff = nvo.Diff(ovo, new List<string> { field.Key });
                    oldValue = JValue.FromObject(from i in diff select i.ToString());

                }

                if (oldValue != null && oldValue.Type == JTokenType.Array && newValue.Type == JTokenType.Array)
                {

                    var ovo = (oldValue as JArray);
                    var nvo = (newValue as JArray);

                    var diff = nvo.Diff(ovo, new List<string> { field.Key });
                    oldValue = JValue.FromObject(from i in diff select i.ToString());

                }

                var change = new UpdateAuditFields
                {
                    field = field.Key,
                    oldValue = oldValue,
                    newValue = newValue
                };

                audit.changes.Add(change);

                transformed.fields[field.Key] = field.Value;
            }

            if (target.name != null)
                transformed.name = target.name;

            return new Tuple<DynamicObject, UpdateAuditLog>(transformed, audit);
        }

        public static async Task<int> Create(ConnectionHelper<DbConnection> Connection, string name, Guid createBy, DynamicObjectType newObjectType, string schema = "pam_2_0_0")
        {
            var sql =
                @"INSERT INTO $_$_SCHEMA_$_$.objects_$_$_KEY_$_$
                    ( name
                    , ""createdBy""
                    , ""lastModifiedBy""
                    )
                VALUES
                    ( @name
                    , @created
                    , @created
                    )
                RETURNING id;
                ";



            sql = sql.Replace("$_$_SCHEMA_$_$", schema);

            var model = await DynamicObjectModelRepo.Get(Connection, newObjectType.key);

            if (model == null || model.key == null)
                return -1;


            sql = sql.Replace("$_$_KEY_$_$", model.key);


            var id = await Connection.GetConnection().QuerySingleAsync<int>(sql, new { name = name, created = createBy });


            CreateAuditLog audit = new CreateAuditLog
            {
            };

            RecordAudit(Connection, model, id, audit, createBy, AuditType.Create);

            return id;
        }
    }



    internal class idable
    {
        public int id { get; set; }
    }


    internal class AuditLog
    {
        
    }

    internal class CreateAuditLog : AuditLog
    {
    }

    internal class UpdateAuditLog : AuditLog
    {
        public List<UpdateAuditFields> changes = new List<UpdateAuditFields>();
    }

    internal class UpdateAuditFields
    {
        public string field { get; set; }
        public JToken oldValue { get; set; }
        public JToken newValue { get; set; }

    }

    internal class CreateAuditFields
    {
        public string field { get; set; }
    }

    public enum AuditType
    {
        Create = 0,
        Update = 10,
        Delete = 20
    }
}
