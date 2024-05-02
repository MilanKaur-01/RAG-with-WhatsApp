using System.Text;
using Microsoft.AspNetCore.Mvc;
using Azure.Messaging.EventGrid;
using Azure.Messaging.EventGrid.SystemEvents;
using System.Text.Json;
using CpmDemoApp.Models;

namespace viewer.Controllers
{
    [Route("webhook")]
    public class WebhookController : Controller
    {
        private bool EventTypeSubcriptionValidation
            => HttpContext.Request.Headers["aeg-event-type"].FirstOrDefault() ==
               "SubscriptionValidation";

        private bool EventTypeNotification
            => HttpContext.Request.Headers["aeg-event-type"].FirstOrDefault() ==
               "Notification";

        private JsonSerializerOptions _options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        [HttpOptions]
        public async Task<IActionResult> Options()
        {
            using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                var webhookRequestOrigin = HttpContext.Request.Headers["WebHook-Request-Origin"].FirstOrDefault();
                var webhookRequestCallback = HttpContext.Request.Headers["WebHook-Request-Callback"];
                var webhookRequestRate = HttpContext.Request.Headers["WebHook-Request-Rate"];
                HttpContext.Response.Headers.Add("WebHook-Allowed-Rate", "*");
                HttpContext.Response.Headers.Add("WebHook-Allowed-Origin", webhookRequestOrigin);
            }

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> Post()
        {
            using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                var jsonContent = await reader.ReadToEndAsync();

                // Check the event type.
                // Return the validation code if it's 
                // a subscription validation request. 
                if (EventTypeSubcriptionValidation)
                {
                    return await HandleValidation(jsonContent);
                }
                else if (EventTypeNotification)
                {
                    return await HandleGridEvents(jsonContent);
                }

                return BadRequest();
            }
        }

        private async Task<JsonResult> HandleValidation(string jsonContent)
        {
            var eventGridEvent = JsonSerializer.Deserialize<EventGridEvent[]>(jsonContent, _options).First();
            var eventData = JsonSerializer.Deserialize<SubscriptionValidationEventData>(eventGridEvent.Data.ToString(), _options);
            var responseData = new SubscriptionValidationResponse
            {
                ValidationResponse = eventData.ValidationCode
            };
            return new JsonResult(responseData);
        }

        private async Task<IActionResult> HandleGridEvents(string jsonContent)
        {
            var eventGridEvents = JsonSerializer.Deserialize<EventGridEvent[]>(jsonContent, _options);
            foreach (var eventGridEvent in eventGridEvents)
            {
                switch (eventGridEvent.EventType.ToLower())
                {
                    case "microsoft.communication.advancedmessagereceived":
                        var messageReceivedEventData = JsonSerializer.Deserialize<CrossPlatformMessageReceivedEventData>(eventGridEvent.Data.ToString(), _options);

                        Messages.MessagesListStatic.Add(new Message
                        {
                            Text = $"Received message from \"{messageReceivedEventData.From}\": \"{messageReceivedEventData.Content}\""
                        });
                        break;
                    case "microsoft.communication.advancedmessageanalysiscompleted":
                        var messageAnalysisEventData = JsonSerializer.Deserialize<AdvancedMessageAnalysisCompletedEventData>(eventGridEvent.Data.ToString(), _options);
                        Messages.MessagesListStatic.Add(new Message
                        {
                            Analysis = messageAnalysisEventData
                        });
                        break;
                    default:
                        break;
                }
            }

            return Ok();
        }
    }
}