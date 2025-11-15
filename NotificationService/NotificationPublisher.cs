// NotificationPublisher.cs à créer
using Apache.NMS;
using Apache.NMS.ActiveMQ;
using System;
using System.Threading;

namespace NotificationService
{
    public class NotificationPublisher
    {
        private IConnection _connection;
        private ISession _session;
        private Timer _weatherTimer;
        private Timer _pollutionTimer;
        private Timer _eventTimer;

        public void Start()
        {
            var factory = new ConnectionFactory(Constants.BrokerUri);
            _connection = factory.CreateConnection();
            _connection.Start();
            _session = _connection.CreateSession();

            Console.WriteLine("✅ Connecté à ActiveMQ");

            // Lancer les timers pour publier périodiquement
            _weatherTimer = new Timer(PublishWeather, null, 0, Constants.WeatherInterval);
            _pollutionTimer = new Timer(PublishPollution, null, 5000, Constants.PollutionInterval);
            _eventTimer = new Timer(PublishEvent, null, 10000, Constants.EventInterval);
        }

        private void PublishWeather(object state)
        {
            var messages = new[] {
                "☀️ Météo favorable pour le vélo",
                "🌧️ Risque de pluie dans 30min",
                "💨 Vent fort prévu"
            };
            PublishMessage(Constants.WeatherTopic, messages[new Random().Next(messages.Length)]);
        }

        private void PublishPollution(object state)
        {
            var messages = new[] {
                "✅ Qualité de l'air excellente",
                "⚠️ Pollution modérée détectée",
                "🔴 Alerte pollution importante"
            };
            PublishMessage(Constants.PollutionTopic, messages[new Random().Next(messages.Length)]);
        }

        private void PublishEvent(object state)
        {
            var messages = new[] {
                "🎉 Événement en centre-ville",
                "🚧 Travaux sur l'Avenue principale",
                "🎭 Festival ce weekend"
            };
            PublishMessage(Constants.EventTopic, messages[new Random().Next(messages.Length)]);
        }

        private void PublishMessage(string topicName, string message)
        {
            var topic = _session.GetTopic(topicName);
            var producer = _session.CreateProducer(topic);
            var textMessage = _session.CreateTextMessage(message);
            producer.Send(textMessage);
            Console.WriteLine($"📤 [{topicName}] {message}");
        }
    }
}