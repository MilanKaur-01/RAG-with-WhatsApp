using System;

namespace CpmDemoApp.Models
{
    public class Data
    {
        public ExperimentalEventPayload experimentalEventPayload { get; set; }
    }

    public class ExperimentalEventPayload
    {
        public string message { get; set; }
        public string type { get; set; }
        public string channelId { get; set; }
        public string from { get; set; }
        public string to { get; set; }
        public string externalMessageId { get; set; }
        public string receivedTimeStamp { get; set; }
    }

    public class ExternalMessageEvent
    {
        public string id { get; set; }
        public string topic { get; set; }
        public string subject { get; set; }
        public Data data { get; set; }
        public string eventType { get; set; }
        public string dataVersion { get; set; }
        public string metadataVersion { get; set; }
        public DateTime eventTime { get; set; }
    }


}
