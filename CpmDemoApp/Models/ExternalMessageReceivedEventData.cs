namespace CpmDemoApp.Models
{
    public class ExternalMessageReceivedEventData
    {
        public string From { get; set; }
        public string To { get; set; }
        public string Message { get; set; }
        public string ChannelType { get; set; }
        public string Type { get; set; }
        public string ReceivedTimeStamp { get; set; }
    }
}
