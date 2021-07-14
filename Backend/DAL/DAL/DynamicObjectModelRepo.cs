using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.timmons.cognitive.API.Model;
using com.timmons.cognitive.API.Util;
using com.timmons.Stitch.Shared;
using Dapper;

namespace com.timmons.cognitive.API.DAL
{
    public class DynamicObjectModelRepo
    {
        public static async Task<DynamicObjectModelNew> Get(ConnectionHelper<DbConnection> con, string key)
        {
            var sql = @"
SELECT 
    m.id as id,
    m.key as key,
    m.singular_name,
    m.plural_name,
    m.display,
    m.display_order,
    m.model || COALESCE(e.extensions, '{}'::jsonb) as fields_json
FROM pam.model as m
LEFT JOIN
	( 	SELECT 
			mo.""extensionOf"", 

            jsonb_object_agg((mo.r).key, (mo.r).value) as extensions

        FROM
        (
            SELECT ""extensionOf"", jsonb_each(m.model) as r

            FROM pam.model as m
        ) mo
        GROUP BY mo.""extensionOf""
 	) as e
ON m.id = e.""extensionOf""
WHERE m.key = @key
";
            var res = await con.GetConnection().QuerySingleAsync<DynamicObjectModelDBDTO>(sql, new { key = key });

            populateFieldsFromJson(res, res.fields_json);

            return res;
        }

        public static void populateFieldsFromJson(DynamicObjectModelNew res, Newtonsoft.Json.Linq.JObject fields_json)
        {
            foreach (var (name, fld) in fields_json)
            {

                FieldDTO asDto = null;

                try
                {
                    asDto = fld.ToObject<FieldDTO>();
                }
                catch { }

                if (asDto != null)
                {
                    switch (asDto.fieldType)
                    {
                        case FieldType.Choice:
                            {
                                //TODO: Refactor this as its sloppy
                                TwoLevelConfigChoiceField asTwoLevelConfigChoiceField = fld.ToObject<TwoLevelConfigChoiceField>();

                                if (asTwoLevelConfigChoiceField != null && asTwoLevelConfigChoiceField.parentField != null)
                                {
                                    res.withChoiceField(asTwoLevelConfigChoiceField);
                                }
                                else
                                {
                                    ConfigChoiceField asConfigChoiceField = fld.ToObject<ConfigChoiceField>();


                                    if (asConfigChoiceField != null)
                                    {
                                        res.withChoiceField(asConfigChoiceField);
                                    }
                                    else
                                    {
                                        ChoiceField asChoiceField = fld.ToObject<ChoiceField>();

                                        if (asChoiceField != null)
                                        {
                                            res.withChoiceField(asChoiceField);
                                        }
                                    }
                                }
                            }; break;
                        case FieldType.ObjectReference:
                            {
                                ByIdObjectReferenceField asById = fld.ToObject<ByIdObjectReferenceField>();

                                if (asById != null && asById.useId)
                                {
                                    res.withObjectReferenceField(asById);
                                }
                                else
                                {
                                    ByNameObjectReferenceField asByName = fld.ToObject<ByNameObjectReferenceField>();

                                    if (asByName != null && asByName.useName)
                                    {
                                        res.withObjectReferenceField(asByName);
                                    }
                                }

                            }; break;
                        case FieldType.Text:
                            {
                               TextField asText = fld.ToObject<TextField>();

                                res.withTextField(asText);

                            }; break;
                        case FieldType.Currency:
                            {
                                CurrencyField asCurrency = fld.ToObject<CurrencyField>();

                                res.withCurrencyField(asCurrency);

                            }; break;
                        case FieldType.LongText:
                            {
                                LongTextField asText = fld.ToObject<LongTextField>();

                                res.withLongTextField(asText);

                            }; break;
                        case FieldType.Float:
                            {
                                var asRaw = fld.ToObject<Field>();

                                res.withField(FieldType.Float, asRaw);

                            }; break;
                        case FieldType.Int:
                            {
                                var asRaw = fld.ToObject<Field>();

                                res.withField(FieldType.Int, asRaw);

                            }; break;
                        case FieldType.Date:
                            {
                                var asRaw = fld.ToObject<Field>();

                                res.withField(FieldType.Date, asRaw);

                            }; break;
                        case FieldType.Flag:
                            {
                                var asRaw = fld.ToObject<Field>();

                                res.withField(FieldType.Flag, asRaw);

                            }; break;
                        case FieldType.Spatial:
                            {
                                var asRaw = fld.ToObject<Field>();

                                res.withField(FieldType.Spatial, asRaw);

                            }; break;
                        case FieldType.ClusterField:
                            {
                                var asClusterDTO = fld.ToObject<ClusterFieldDTO>();

                                DynamicObjectModelNew res_sub = new DynamicObjectModelNew();
                                populateFieldsFromJson(res_sub, asClusterDTO.subfields);

                                var rf = fld.ToObject<Field>();
                                ClusterField cf = new ClusterField
                                {
                                    id = rf.id,
                                    name = rf.name
                                };
                                cf.subfields = res_sub.fields;


                                res.withField(FieldType.ClusterField, cf);

                            }; break;
                        default:
                            {
                                var asLongText = fld.ToObject<RawField>();
                                asLongText.raw = fld;
                                res.withField(asDto.fieldType, asLongText);
                            }; break;
                    }
                }
            }
        }

        public static IEnumerable<Field> fieldsFromJson(Newtonsoft.Json.Linq.JObject fields_json)
        {
            foreach (var (name, fld) in fields_json)
            {
                var asDto = fld.ToObject<FieldDTO>();

                if (asDto != null)
                {
                    switch (asDto.fieldType)
                    {
                        case FieldType.Text:
                            {
                                //TODO: Refactor this as its sloppy
                                TextField asTextField = fld.ToObject<TextField>();

                                yield return asTextField;

                            }; break;
                        case FieldType.LongText:
                            {
                                //TODO: Refactor this as its sloppy
                                LongTextField asTextField = fld.ToObject<LongTextField>();

                                yield return asTextField;

                            }; break;
                        case FieldType.Choice:
                            {
                                //TODO: Refactor this as its sloppy
                                TwoLevelConfigChoiceField asTwoLevelConfigChoiceField = fld.ToObject<TwoLevelConfigChoiceField>();

                                if (asTwoLevelConfigChoiceField != null && asTwoLevelConfigChoiceField.parentField != null)
                                {
                                    yield return asTwoLevelConfigChoiceField;
                                }
                                else
                                {
                                    ConfigChoiceField asConfigChoiceField = fld.ToObject<ConfigChoiceField>();


                                    if (asConfigChoiceField != null)
                                    {
                                        yield return asConfigChoiceField;
                                    }
                                    else
                                    {
                                        ChoiceField asChoiceField = fld.ToObject<ChoiceField>();

                                        if (asChoiceField != null)
                                        {
                                            yield return asChoiceField;
                                        }
                                    }
                                }
                            }; break;
                        case FieldType.ObjectReference:
                            {
                                ByIdObjectReferenceField asById = fld.ToObject<ByIdObjectReferenceField>();

                                if (asById != null && asById.useId)
                                {
                                    yield return asById;
                                }
                                else
                                {
                                    ByNameObjectReferenceField asByName = fld.ToObject<ByNameObjectReferenceField>();

                                    if (asByName != null && asByName.useName)
                                    {
                                        yield return asByName;
                                    }
                                }

                            }; break;
                    }
                }
            }
        }

        public static async Task<DynamicObjectModelNew> Get(ConnectionHelper<DbConnection> con, int id)
        {
            var sql = @"
SELECT 
    m.id as id,
    m.key as key,
    m.singular_name,
    m.plural_name,
    m.display,
    m.display_order,
    m.model || COALESCE(e.extensions, '{}'::jsonb) as fields_json
FROM pam.model as m
LEFT JOIN
	( 	SELECT 
			mo.""extensionOf"", 

            jsonb_object_agg((mo.r).key, (mo.r).value) as extensions

        FROM
        (
            SELECT ""extensionOf"", jsonb_each(m.model) as r

            FROM pam.model as m
        ) mo
        GROUP BY mo.""extensionOf""
 	) as e
ON m.id = e.""extensionOf""
WHERE m.id = @id
LIMIT 1

";
            var res = await con.GetConnection().QuerySingleAsync<DynamicObjectModelDBDTO>(sql, new { id = id });

            populateFieldsFromJson(res, res.fields_json);

            return res;
        }
    }

    internal class FieldDTO
    {
        public int id { get; set; }
        public FieldType fieldType { get; set; }
    }

    internal class ClusterFieldDTO
    {
        public int id { get; set; }
        public FieldType fieldType { get; set; }
        public Newtonsoft.Json.Linq.JObject subfields { get; set; }
    }

    public class DynamicObjectModelDBDTO : DynamicObjectModelNew
    {
        public Newtonsoft.Json.Linq.JObject fields_json { get; set; }
    }
}
