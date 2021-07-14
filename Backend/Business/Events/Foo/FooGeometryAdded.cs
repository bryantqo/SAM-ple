using com.timmons.cognitive.API.Model;
using System;
using com.timmons.cognitive.API.Model.Event;
using Newtonsoft.Json.Linq;

namespace API.Middleware.Events.Foo
{
    public class FooGeometryAddedEvent : Event
    {
        private Guid streamId = Guid.NewGuid();

        public JObject geometry { get; set; }
        public JObject properties { get; set; }
        public string type { get; set; }

        public FooGeometryAddedEvent(JObject geometry, JObject properties, string type)
        {
            this.geometry = geometry;
            this.properties = properties;
            this.type = type;
        }

        public Guid getType()
        {
            return Guid.Parse("1C582DAF-1BC7-49D7-BD45-E83C7832A4B4");
        }
        public Guid getStreamID()
        {
            return streamId;
        }

    }

    public class FooGeometryAddedProjection : Projection<FooGeometryAddedEvent, DynamicObject>
    {
        public DynamicObject Apply(FooGeometryAddedEvent evt, DynamicObject source)
        {
            return source;
        }
    }
}
