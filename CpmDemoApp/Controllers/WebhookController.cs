using Azure.Messaging.EventGrid;
using Azure.Messaging.EventGrid.SystemEvents;
using CpmDemoApp.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;

namespace CpmDemoApp.Controllers
{
    [ApiController]
    [Route("webhook")]
    //[Consumes("application/json")]
    public class WebhookController : Controller
    {
        [HttpPost]
        [HttpGet]
        public JObject Post([FromBody] object request)
        {
            var eventGridEvents = JsonSerializer.Deserialize<EventGridEvent[]>(request.ToString());
            EventGridEvent eventGridEvent = eventGridEvents.FirstOrDefault();
            //BinaryData events = BinaryData.FromStream((Stream)request);
            //EventGridEvent[] eventGridEvents = EventGridEvent.ParseMany(events);
            //var eventGridEvent = eventGridEvents.FirstOrDefault();

            if (eventGridEvent == null) return new JObject(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
            var data = eventGridEvent.Data.ToString();

            if (string.Equals(eventGridEvent.EventType, "Microsoft.EventGrid.SubscriptionValidationEvent", StringComparison.OrdinalIgnoreCase))
            {
                if (eventGridEvent.Data != null)
                {
                    var eventData = JsonSerializer.Deserialize<SubscriptionValidationEventData>(data);
                    var responseData = new SubscriptionValidationResponse
                    {
                        ValidationResponse = eventData.ValidationCode
                    };

                    if (responseData.ValidationResponse != null)
                    {
                        return JObject.FromObject(responseData);
                    }
                }
            }
            else
            {
                if (data == null) return new JObject(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
                var eventData = JsonSerializer.Deserialize<object>(data);
                return JObject.FromObject(eventData);
            }

            return new JObject(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
        }
    }
    
}
