using System;
using System.Collections.Generic;
using System.Text;

namespace com.timmons.PAM.API.Model
{
    public class Spatial
    {

        public int id { get; set; }
        public string wkt { get; set; }
        public float acres { get; set; }
        public string type { get; set; }
    }

    public class Spat
    {
        public Spat(int id, Newtonsoft.Json.Linq.JObject geojson)
        {
            this.id = id;
            this.geojson = geojson;
        }
        public int id { get; set; }
        public Newtonsoft.Json.Linq.JObject geojson { get; set; }
    }

    public class SpatDTO
    {
        public int id { get; set; }
        public string geojson { get; set; }
    }
}
