namespace CpmDemoApp.Models
{
    public class AdvancedMessageAnalysisCompletedEventData
    {
        public string OriginalMessage { get; set; }

        public LanguageDetection LanguageDetection { get; set; }

        public Sentiment Sentiment { get; set; }

        public string IntentAnalysis { get; set; }

        public List<string> KeyphraseExtraction { get; set; }
    }
}
