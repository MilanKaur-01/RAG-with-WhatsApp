using OpenAI.Chat;

namespace CpmDemoApp.Models
{
     public class Messages
    {
        public static IList<Message> MessagesListStatic { get; set; } = new List<Message>();
        public static IList<ChatMessage> OpenAIConversationHistory { get; set; } = new List<ChatMessage>();
    }

    public class Message
    {
        public string Text { get; set; } = null;
    }
}