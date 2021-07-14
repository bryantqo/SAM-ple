using Dapper;
using Newtonsoft.Json.Linq;
using System.Data;

namespace com.timmons.Stitch.Shared
{
    public class Init
    {
        public static void init()
        {
            SqlMapper.AddTypeHandler(new JObjectHandler());
            SqlMapper.AddTypeHandler(new JArrayHandler());
        }
    }

    public class JObjectHandler : SqlMapper.TypeHandler<Newtonsoft.Json.Linq.JObject>
    {
        public override JObject Parse(object value)
        {
            string json = value.ToString();
            return JObject.Parse(json);
        }

        public override void SetValue(IDbDataParameter parameter, JObject value)
        {
            parameter.Value = value.ToString();
        }
    }

    public class JArrayHandler : SqlMapper.TypeHandler<Newtonsoft.Json.Linq.JArray>
    {
        public override JArray Parse(object value)
        {
            string json = value.ToString();
            return JArray.Parse(json);
        }

        public override void SetValue(IDbDataParameter parameter, JArray value)
        {
            parameter.Value = value.ToString();
        }
    }
}
