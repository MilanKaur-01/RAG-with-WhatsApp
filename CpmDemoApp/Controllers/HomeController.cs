using Azure.Communication.Chat;
using CpmDemoApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

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
                ViewData["Gloria"] = "changed";
                return View();
            }

            SendExternalMessageResult result;
            if (Image != null)
            {
                var options = new SendExternalMessageOptions(Phone_Number, new Uri(Image));
                result = await DemoChatClient.ChatClient.SendExternalMessageAsync(options);
            }
            else
            {
                var options = new SendExternalMessageOptions(Phone_Number, Message);
                result = await DemoChatClient.ChatClient.SendExternalMessageAsync(options);
            }

            if (result.Status == ExternalMessageStatus.Enqueued)
            {
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
                   
                ModelState.Clear();
            }
            else
            {
                Messages.MessagesListStatic.Add(new Message
                {
                    Text = $"Message \"{Message}\" to \"{Phone_Number}\" failed."
                });
            }
            
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
    }
}