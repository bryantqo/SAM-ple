using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.timmons.cognitive.API.Model.Event
{
    public class ShapeAdded : Event
    {
        public Guid getType()
        {
            return Guid.Parse("3A27C881-94AC-4B26-9297-565C39C9DCA0");
        }
        public Guid getStreamID()
        {
            return streamId;
        }

        public Guid streamId = Guid.NewGuid();
        public int objectId { get; set; }
        public int objectTypeId { get; set; }
        public int fieldId { get; set;  }
        public Newtonsoft.Json.Linq.JObject geometry { get; set; }
        public Newtonsoft.Json.Linq.JObject properties { get; set; }
    }
}
