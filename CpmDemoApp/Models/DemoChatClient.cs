using Azure.Communication;
using Azure.Communication.Chat;
using Azure.Communication.Identity;
using Azure.Core;
using System.Timers;

namespace CpmDemoApp.Models
{
    public class DemoChatClient
    { 
        static Uri endpoint = new Uri("https://acsxxplatdemo.int.communication.azure.net");
        static string connectionString = "endpoint=https://acsxxplatdemo.int.communication.azure.net/;accesskey=7zIRqTFXRyeBzypL08hu1tJh/QTLNvLChrodkr7FeqyhIfPHx21EPU7Z6BcGbMcNH2xvpcFhM4pi1r8UXgJMgA==";
        static CommunicationUserIdentifier userIdentifier = new CommunicationUserIdentifier("8:acs:51538f79-1c6e-48fb-b379-9ed2457bde90_00000012-7ebe-83c7-5896-094822000d7b");
        static CommunicationIdentityClient communicationIdentityClient;
        static IEnumerable<CommunicationTokenScope> scopes;
        static AccessToken token;

        static DemoChatClient()
        {
            communicationIdentityClient = new CommunicationIdentityClient(connectionString);
            scopes = new[] { CommunicationTokenScope.Chat };

            refreshTokenEveryDay();
            var startTimeSpan = TimeSpan.Zero;
            var periodTimeSpan = TimeSpan.FromHours(24); // refresh the token every 24 hours

            var timer = new System.Threading.Timer((e) =>
            {
                refreshTokenEveryDay();
            }, null, startTimeSpan, periodTimeSpan);
        }

        public static ChatClient ChatClient { get; set; }

        static void refreshTokenEveryDay()
        {
            token = communicationIdentityClient.GetToken(userIdentifier, scopes);
            CommunicationTokenCredential communicationTokenCredential = new CommunicationTokenCredential(token.Token);
            ChatClient = new ChatClient(endpoint, communicationTokenCredential);
        }
    }
}