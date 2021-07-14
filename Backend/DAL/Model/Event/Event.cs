using System;

namespace com.timmons.cognitive.API.Model.Event
{
    public class EventEntry
    {
        public int id { get; set; }
        public Guid streamid { get; set; }
        public DateTime date { get; set; }
        public Guid who { get; set; }
        public Guid type { get; set; }
        public Newtonsoft.Json.Linq.JObject dataRaw { get; set; }
        public Event data { get; set; }

        public void serialize()
        {
            this.dataRaw = Newtonsoft.Json.Linq.JObject.FromObject(data);
            this.type = data.getType();
        }

        public void deserialize<E>() where E : Event 
        {
            this.data = this.dataRaw.ToObject<E>();
        }
    }

    public interface Event
    {
        Guid getStreamID();
        Guid getType();
    }

}
