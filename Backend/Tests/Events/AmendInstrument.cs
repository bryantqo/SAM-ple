using API.Middleware.Helpers;
using com.timmons.cognitive.API.Model;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Tests.Mocks;
using PAM_Model = com.timmons.cognitive.API.Model;
using Model = API.Middleware.Model;
using System.Threading.Tasks;
using com.timmons.cognitive.API.Model.Event;
using API.Middleware.Events.Instrument;
using API.Middleware.Events.Amendment;

namespace Tests.Events
{
    public class AmendInstrument
    {
        private LookyLooMock instruments;
        private LookyLooMock amendments;

        [SetUp]
        public void Setup()
        {
            instruments = new LookyLooMock();
            amendments = new LookyLooMock();

                //.WithMockObject(new PAM_Model.DynamicObject
                //{
                //    id = 100,
                //    name = "Test Fake Object 1",
                //    fields = null,
                //    created = 1234,
                //    modified = 1234,
                //    createdBy = new PAM_Model.User
                //    {
                //        id = Guid.NewGuid()
                //    },
                //    lastModifiedBy = new PAM_Model.User
                //    {
                //        id = Guid.NewGuid()
                //    }
                //})
                //.WithMockObject(new PAM_Model.DynamicObject
                //{
                //    id = 200,
                //    name = "Test Fake Object 2",
                //    fields = null,
                //    created = 1234,
                //    modified = 1234,
                //    createdBy = new PAM_Model.User
                //    {
                //        id = Guid.NewGuid()
                //    },
                //    lastModifiedBy = new PAM_Model.User
                //    {
                //        id = Guid.NewGuid()
                //    }
                //});
        }

        [Test]
        public async Task TestAmendingInstrument()
        {
            // This task should use a fake instrument and apply an AmedInstrumentEvent
            // The result should be that a new Amendment is created and added to the instrument

            // I think these are all the events that need to happen to get to this point ...
            // We will need to import old instruments at some point ...

            List<Event> queue = new List<Event>();
            var instrumentEvent = new InstrumentCreatedEvent(1, 1, 1, Guid.NewGuid());
            queue.Add(instrumentEvent);

            queue.Add(new InstrumentUpdatedEvent(instrumentEvent.getStreamID(), JObject.Parse("{}")));

            var amendmentEvent = new AmendmentCreatedEvent(1);

            queue.Add(amendmentEvent);
            queue.Add(new InstrumentAmendedEvent(instrumentEvent.getStreamID(), amendmentEvent.getStreamID()));

            queue.Add(new AmendmentUpdatedEvent(amendmentEvent.getStreamID(), JObject.Parse("{}")));
        }
    }
}