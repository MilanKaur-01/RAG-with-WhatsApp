using System.Text;
using Microsoft.AspNetCore.Mvc;
using Azure.Messaging.EventGrid;
using Azure.Messaging.EventGrid.SystemEvents;
using System.Text.Json;
using CpmDemoApp.Models;
using Azure.AI.OpenAI;
using Azure.Communication.Messages;
using static System.Net.WebRequestMethods;
using OpenAI.Chat;
using System.ClientModel;

namespace viewer.Controllers
{
    [Route("webhook")]
    public class WebhookController : Controller
    {
        private static string SystemPrompt => "You are an AI customer service assistant who helps resolve queries of customers." +
                    " You ask them the error code on the screen and use the below context to help them resolve the issue." +
                    "You maintain a professional and friendly tone." +
                    "If you do not find answer in the context below, you do not search the web. Instead you say 'I do not know how to fix this one. Please call customer service. Thank you'" +
                    "Context for answering questions" +
                    "Code: 1000\r\nErrorName: Math ERROR\r\nCause: Calculation or input exceeds range or involves an illegal operation.\r\nAction: Adjust input values and press “Clear” to retry.\r\n\r\n" +
                    "Code: 1001\r\nErrorName: Stack ERROR\r\nCause: Stack capacity exceeded.\r\nAction: Simplify your expression and press “Enter” to try again.\r\n\r\n" +
                    "Code: 1002\r\nErrorName: Syntax ERROR\r\nCause: Calculation format issue.\r\nAction: Correct the format, then press “Clear” and re-enter the calculation.\r\n\r\n" +
                    "Code: 1003\r\nErrorName: Dimension ERROR (MATRIX and VECTOR)\r\nCause: Matrix/vector dimensions not specified or incompatible.\r\nAction: Verify dimensions, then press “Clear” and re-enter the matrix/vector.\r\n\r\n" +
                    "Code: 1004\r\nErrorName: Variable ERROR (SOLVE)\r\nCause: Missing or incorrect solution variable.\r\nAction: Include the variable in your equation, then press “Solve” again.\r\n\r\n" +
                    "Code: 1005\r\nErrorName: Can't Solve Error (SOLVE)\r\nCause: Solution could not be obtained.\r\nAction: Check your equation for errors, then adjust your input and press “Solve” again.";

        private bool EventTypeSubcriptionValidation
            => HttpContext.Request.Headers["aeg-event-type"].FirstOrDefault() ==
               "SubscriptionValidation";

        private bool EventTypeNotification
            => HttpContext.Request.Headers["aeg-event-type"].FirstOrDefault() ==
               "Notification";

        private static NotificationMessagesClient _notificationMessagesClient => new NotificationMessagesClient("endpoint=https://acs-sms-vr-ai-test.unitedstates.communication.azure.com/;accesskey=YmhT+/CguJ6CPChz7vNUeeGyfdT35b1eoU5FutKpOJmWtDR9Jism9nJE2eDFCt4WwwNZI/rtTPiV026/qaoKgw==");

        private JsonSerializerOptions _options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        //Initialize Azure Open AI client
        private AzureOpenAIClient _client => new AzureOpenAIClient(
            new Uri("https://milandemo.openai.azure.com/"),
            new System.ClientModel.ApiKeyCredential("f3207db1b717441a8d94f82cb9ac718f")
            );

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
                        Messages.ConversationHistory.Add(new UserChatMessage(messageReceivedEventData.Content));
                        respondToTheCustomer(messageReceivedEventData.From);
                        break;
                    default:
                        break;
                }
            }

            return Ok();
        }

        private async void respondToTheCustomer(string numberToRespondTo)
        {
            var systemPrompt = new SystemChatMessage(SystemPrompt);
            var conversationHistory = Messages.ConversationHistory;

            var chatMessages = new List<ChatMessage> { systemPrompt };
            chatMessages.AddRange(conversationHistory);

            var response = await _client.GetChatClient("MilanDemoWhatsApp").CompleteChatAsync(chatMessages);

            // Assuming response.Value.ChatResponse contains the text response
            var responseText = response.Value.Content[0].Text;

            await SendWhatsAppMessage(numberToRespondTo, responseText);
            Messages.ConversationHistory.Add(new AssistantChatMessage(responseText));
            Messages.MessagesListStatic.Add(new Message
            {
                Text = $"Assistant : {responseText}"
            });

        }

        private async Task SendWhatsAppMessage(string numberToRespondTo, string message)
        {
            var recipientList = new List<string> { numberToRespondTo };
            var textContent = new TextNotificationContent(
                new Guid("e8363b7a-8618-4d98-9508-d2842661e745"),
                new List<string> { numberToRespondTo },
                message);
            await _notificationMessagesClient.SendAsync(textContent);
        }
    }
}