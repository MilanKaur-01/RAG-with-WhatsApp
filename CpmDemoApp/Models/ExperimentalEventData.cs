namespace CpmDemoApp.Models
{
    public class ExperimentalEventData
    {
        public ExperimentalEventPayload ExperimentalEventPayload { get; set; }
    }

    public class ExperimentalEventPayload
    {
        public string Message { get; set; }
        public string Type { get; set; }
        public string ChannelId { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string ExternalMessageId { get; set; }
        public string ReceivedTimeStamp { get; set; }
    }
}
