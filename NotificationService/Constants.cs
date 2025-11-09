namespace NotificationService
{
    /// <summary>
    /// Constantes pour le service de notifications
    /// </summary>
    public static class Constants
    {
        // URL du broker ActiveMQ
        public const string BrokerUri = "tcp://localhost:61616";

        // Noms des topics (IMPORTANT: ce sont des TOPICS, pas des QUEUES)
        public const string WeatherTopic = "weather.alerts";
        public const string PollutionTopic = "pollution.alerts";
        public const string EventTopic = "event.alerts";

        // Intervalles de publication (en millisecondes)
        public const int WeatherInterval = 15000;  // 15 secondes
        public const int PollutionInterval = 20000; // 20 secondes
        public const int EventInterval = 25000;     // 25 secondes
    }
}


//(pour les URLs et topics)