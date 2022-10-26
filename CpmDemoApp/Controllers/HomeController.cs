using Azure.Communication.Messages;
using CpmDemoApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Azure;

namespace CpmDemoApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string Phone_Number, string Message, string Image)
        {
            if (string.IsNullOrWhiteSpace(Phone_Number) 
                || (string.IsNullOrWhiteSpace(Message) && string.IsNullOrWhiteSpace(Image)))
            {
                Messages.MessagesListStatic.Add(new Message
                {
                    Text = "Please make sure you have put down phone number and either a text message or an image url.",
                });
                return View();
            }

            var recipientList = new List<string> { Phone_Number };


            try
            {
                if (Image != null)
                {
                    var options = new SendMessageOptions(DemoNotificationMessagesClient.ChannelRegistrationId, recipientList, new Uri(Image));
                    await DemoNotificationMessagesClient.NotificationMessagesClient.SendMessageAsync(options);
                }
                else
                {
                    var options = new SendMessageOptions(DemoNotificationMessagesClient.ChannelRegistrationId, recipientList, Message);
                    await DemoNotificationMessagesClient.NotificationMessagesClient.SendMessageAsync(options);
                }

                if (Image != null)
                {
                    Messages.MessagesListStatic.Add(new Message
                    {
                        Text = $"Sent a image to \"{Phone_Number}\": ",
                        Image = Image
                    });
                }
                else
                {
                    Messages.MessagesListStatic.Add(new Message { 
                    Text = $"Sent a message to \"{Phone_Number}\": \"{Message}\"" 
                    });
                }
            }
            catch (RequestFailedException e)
            {
                Messages.MessagesListStatic.Add(new Message
                {
                    Text = $"Message \"{Message}\" to \"{Phone_Number}\" failed. Exception: {e.Message}"
                });
            }
                   
            ModelState.Clear();
            
            return View();
        }

        public IActionResult MessagesList()
        {
            return View();
        }

        [HttpPost]
        public IActionResult MessagesList(string nothing)
        {
            return PartialView();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public IActionResult ClearHistory()
        {
            Messages.MessagesListStatic = new List<Message>();
            return RedirectToAction("Index");
        }
    }
}