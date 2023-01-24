namespace CpmDemoApp.Models
{
    public class CrossPlatformMessageReceivedEventData
    {
        public string From { get; set; }
        public string To { get; set; }
        public string Content { get; set; }
        public string ChannelType { get; set; }
        public string Type { get; set; }
        public string ReceivedTimeStamp { get; set; }
    }
}
