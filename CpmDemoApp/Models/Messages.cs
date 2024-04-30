namespace CpmDemoApp.Models
{
    public class Messages
    {
        public static IList<Message> MessagesListStatic { get; set; } = new List<Message>();
    }

    public class Message
    {
        public string Text { get; set; } = null;
        public string Image { get; set; } = null;
        public AdvancedMessageAnalysisCompletedEventData? Analysis { get; set; } = null;
    }
}