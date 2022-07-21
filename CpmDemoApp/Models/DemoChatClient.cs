using Azure.Communication;
using Azure.Communication.Chat;
using Azure.Communication.Identity;
using Azure.Core;
using System.Timers;

namespace CpmDemoApp.Models
{
    public class DemoChatClient
    {
        private static Uri _endpoint = new Uri("https://acsxxplatdemo.int.communication.azure.net");
        private static string _connectionString = "endpoint=https://acsxxplatdemo.int.communication.azure.net/;accesskey=7zIRqTFXRyeBzypL08hu1tJh/QTLNvLChrodkr7FeqyhIfPHx21EPU7Z6BcGbMcNH2xvpcFhM4pi1r8UXgJMgA==";
        private static CommunicationUserIdentifier _userIdentifier = new CommunicationUserIdentifier("8:acs:51538f79-1c6e-48fb-b379-9ed2457bde90_00000012-7ebe-83c7-5896-094822000d7b");
        private static CommunicationIdentityClient _communicationIdentityClient;
        private static IEnumerable<CommunicationTokenScope> _scopes;
        private static AccessToken _token;
        private static ChatClient _chatClient { get; set; }

        static DemoChatClient()
        {
            _communicationIdentityClient = new CommunicationIdentityClient(_connectionString);
            _scopes = new[] { CommunicationTokenScope.Chat };
            refreshToken();
        }

        public static ChatClient ChatClient {
            get
            {
                if (_token.ExpiresOn < DateTimeOffset.UtcNow)
                    refreshToken();
                return _chatClient;
            } 
            set {
                _chatClient = value;
            } 
        }

        public static void refreshToken()
        {
            _token = _communicationIdentityClient.GetToken(_userIdentifier, _scopes);
            CommunicationTokenCredential communicationTokenCredential = new CommunicationTokenCredential(_token.Token);
            _chatClient = new ChatClient(_endpoint, communicationTokenCredential);
            System.Diagnostics.Debug.WriteLine("Refreshed token");
        }
    }
}