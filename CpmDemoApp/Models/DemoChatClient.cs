using Azure.Communication;
using Azure.Communication.Chat;
using Azure.Communication.Identity;
using Azure.Core;

namespace CpmDemoApp.Models
{
    public class DemoChatClient
    {
        static DemoChatClient()
        {
            // Your unique Azure Communication service endpoint
            Uri endpoint = new Uri("https://acsxxplatdemo.int.communication.azure.net");
            string connectionString = "endpoint=https://acsxxplatdemo.int.communication.azure.net/;accesskey=7zIRqTFXRyeBzypL08hu1tJh/QTLNvLChrodkr7FeqyhIfPHx21EPU7Z6BcGbMcNH2xvpcFhM4pi1r8UXgJMgA==";
            CommunicationUserIdentifier userIdentifier = new CommunicationUserIdentifier("8:acs:51538f79-1c6e-48fb-b379-9ed2457bde90_00000012-7ebe-83c7-5896-094822000d7b");

            CommunicationIdentityClient communicationIdentityClient = new CommunicationIdentityClient(connectionString);
            IEnumerable<CommunicationTokenScope> scopes = new[] { CommunicationTokenScope.Chat };
            AccessToken token = communicationIdentityClient.GetToken(userIdentifier, scopes);
            CommunicationTokenCredential communicationTokenCredential = new CommunicationTokenCredential(token.Token);
            ChatClient = new ChatClient(endpoint, communicationTokenCredential);
        }

        public static ChatClient ChatClient { get; }
    }
}