using System.Text;
using Microsoft.AspNetCore.Mvc;
using Azure.Messaging.EventGrid;
using Azure.Messaging.EventGrid.SystemEvents;
using System.Text.Json;
using CpmDemoApp.Models;
using Azure.AI.OpenAI;
using Azure.Communication.Messages;
using OpenAI.Chat;
using System.ClientModel;
using Microsoft.Extensions.Options;
using Azure;

namespace viewer.Controllers
{
    [Route("webhook")]
    public class WebhookController : Controller
    {
        private static bool _clientsInitialized;
        private static NotificationMessagesClient _notificationMessagesClient;
        private static Guid _channelRegistrationId;
        private static AzureOpenAIClient _azureOpenAIClient;
        private static string _deploymentName;

        private static string SystemPrompt => "You are Contoso Electronics AI customer service assistant who helps resolve queries of customers." +
                    "When a customer sends you the first message, you greet them and ask them if they need help with their calculator." +
                    "You ask them the error code on the screen and use the below context to help them resolve the issue." +
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

        private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

        public WebhookController(
            IOptions<NotificationMessagesClientOptions> notificationOptions, 
            IOptions<OpenAIClientOptions> AIOptions)
        {
            if (!_clientsInitialized)
            {
                _channelRegistrationId = Guid.Parse(notificationOptions.Value.ChannelRegistrationId);
                _deploymentName = AIOptions.Value.DeploymentName;
                _notificationMessagesClient = new NotificationMessagesClient(notificationOptions.Value.ConnectionString);
                _azureOpenAIClient = new AzureOpenAIClient(new Uri(AIOptions.Value.Endpoint), new ApiKeyCredential(AIOptions.Value.Key));
                _clientsInitialized = true;
            }
        }

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
                // Return the validation code if it's a subscription validation request. 
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
            var eventGridEvent = JsonSerializer.Deserialize<EventGridEvent[]>(jsonContent, _jsonOptions).First();
            var eventData = JsonSerializer.Deserialize<SubscriptionValidationEventData>(eventGridEvent.Data.ToString(), _jsonOptions);
            var responseData = new SubscriptionValidationResponse
            {
                ValidationResponse = eventData.ValidationCode
            };
            return new JsonResult(responseData);
        }

        private async Task<IActionResult> HandleGridEvents(string jsonContent)
        {
            var eventGridEvents = JsonSerializer.Deserialize<EventGridEvent[]>(jsonContent, _jsonOptions);
            foreach (var eventGridEvent in eventGridEvents)
            {
                if (eventGridEvent.EventType.Equals("microsoft.communication.advancedmessagereceived", StringComparison.OrdinalIgnoreCase))
                {
                    var messageData = JsonSerializer.Deserialize<AdvancedMessageReceivedEventData>(eventGridEvent.Data.ToString(), _jsonOptions);
                    Messages.MessagesListStatic.Add(new Message
                    {
                        Text = $"Customer({messageData.From}): \"{messageData.Content}\""
                    });
                    Messages.OpenAIConversationHistory.Add(new UserChatMessage(messageData.Content));
                    await RespondToCustomerAsync(messageData.From);
                }
            }

            return Ok();
        }

        private async Task RespondToCustomerAsync(string numberToRespondTo)
        {
            try
            {
                var assistantResponseText = await GenerateAIResponseAsync();
                if (string.IsNullOrWhiteSpace(assistantResponseText))
                {
                    Messages.MessagesListStatic.Add(new Message
                    {
                        Text = "Error: No response generated from Azure OpenAI."
                    });
                    return;
                }

                await SendWhatsAppMessageAsync(numberToRespondTo, assistantResponseText);
                Messages.OpenAIConversationHistory.Add(new AssistantChatMessage(assistantResponseText));
                Messages.MessagesListStatic.Add(new Message
                {
                    Text = $"Assistant: {assistantResponseText}"
                });
            }
            catch (RequestFailedException e)
            {
                Messages.MessagesListStatic.Add(new Message
                {
                    Text = $"Error: Failed to respond to \"{numberToRespondTo}\". Exception: {e.Message}"
                });
            }
        }

        private async Task<string?> GenerateAIResponseAsync()
        {
            var chatMessages = new List<ChatMessage> { new SystemChatMessage(SystemPrompt) };
            chatMessages.AddRange(Messages.OpenAIConversationHistory);
            ChatCompletion response = await _azureOpenAIClient.GetChatClient(_deploymentName).CompleteChatAsync(chatMessages);
            return response?.Content.FirstOrDefault()?.Text;
        }

        private async Task SendWhatsAppMessageAsync(string numberToRespondTo, string message)
        {
            var recipientList = new List<string> { numberToRespondTo };
            var textContent = new TextNotificationContent(_channelRegistrationId, recipientList, message);
            await _notificationMessagesClient.SendAsync(textContent);
        }
    }
}