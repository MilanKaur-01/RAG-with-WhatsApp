using Azure.Communication.Messages;

namespace CpmDemoApp.Models
{
    public class DemoNotificationMessagesClient
    {
        public static NotificationMessagesClient NotificationMessagesClient;
        public static string ChannelRegistrationId;

        static DemoNotificationMessagesClient()
        {
            string connectionString = "";
            NotificationMessagesClient = new NotificationMessagesClient(connectionString); 
            ChannelRegistrationId = "";
        }
    }
}