namespace CpmDemoApp.Models
{
    public class Messages
    {
        public IList<string> MessagesList
        {
            get { return _messagesListStatic; }   // get method
        }
        private static IList<string> _messagesListStatic = new List<string>();

        public static IList<string> MessagesListStatic
        {
            get { return _messagesListStatic; }
            set { _messagesListStatic = value; } 
        } 
        
    }
}