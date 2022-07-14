using System;

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

    public class ExternalMessageEvent
    {
        public string Id { get; set; }
        public string Topic { get; set; }
        public string Subject { get; set; }
        public BinaryData Data { get; set; }
        public string EventType { get; set; }
        public string DataVersion { get; set; }
        public string MetadataVersion { get; set; }
        public DateTime EventTime { get; set; }
    }


}
