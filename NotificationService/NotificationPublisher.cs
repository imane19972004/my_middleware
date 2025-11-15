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
        private readonly Random _random = new Random();

        public void Start()
        {
            try
            {
                Console.WriteLine("🔄 Connexion à ActiveMQ...");

                var factory = new ConnectionFactory(Constants.BrokerUri);
                _connection = factory.CreateConnection();
                _connection.Start();
                _session = _connection.CreateSession();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✅ Connecté à ActiveMQ sur " + Constants.BrokerUri);
                Console.ResetColor();

                // Lancer les timers pour publier périodiquement
                _weatherTimer = new Timer(PublishWeather, null, 0, Constants.WeatherInterval);
                _pollutionTimer = new Timer(PublishPollution, null, 5000, Constants.PollutionInterval);
                _eventTimer = new Timer(PublishEvent, null, 10000, Constants.EventInterval);

                Console.WriteLine("\n📤 Publication de notifications démarrée:");
                Console.WriteLine($"   - Météo: toutes les {Constants.WeatherInterval / 1000}s");
                Console.WriteLine($"   - Pollution: toutes les {Constants.PollutionInterval / 1000}s");
                Console.WriteLine($"   - Événements: toutes les {Constants.EventInterval / 1000}s\n");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ ERREUR de connexion à ActiveMQ:");
                Console.WriteLine($"   Message: {ex.Message}");
                Console.WriteLine($"   Type: {ex.GetType().Name}");
                Console.ResetColor();
                Console.WriteLine("\n💡 Vérifiez qu'ActiveMQ est lancé:");
                Console.WriteLine("   C:\\activemq\\bin\\activemq.bat start");
                throw;
            }
        }

        private void PublishWeather(object state)
        {
            var messages = new[] {
                "☀️ Météo favorable pour le vélo",
                "🌧️ Risque de pluie dans 30min",
                "💨 Vent fort prévu",
                "⛈️ Alerte orage dans la région"
            };
            PublishMessage(Constants.WeatherTopic, messages[_random.Next(messages.Length)]);
        }

        private void PublishPollution(object state)
        {
            var messages = new[] {
                "✅ Qualité de l'air excellente",
                "⚠️ Pollution modérée détectée",
                "🔴 Alerte pollution importante",
                "🟢 Air pur aujourd'hui"
            };
            PublishMessage(Constants.PollutionTopic, messages[_random.Next(messages.Length)]);
        }

        private void PublishEvent(object state)
        {
            var messages = new[] {
                "🎉 Événement en centre-ville",
                "🚧 Travaux sur l'Avenue principale",
                "🎭 Festival ce weekend",
                "🚴 Course cycliste samedi"
            };
            PublishMessage(Constants.EventTopic, messages[_random.Next(messages.Length)]);
        }

        private void PublishMessage(string topicName, string message)
        {
            try
            {
                var topic = _session.GetTopic(topicName);
                using (var producer = _session.CreateProducer(topic))
                {
                    var textMessage = _session.CreateTextMessage(message);
                    producer.Send(textMessage);

                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"📤 [{topicName}] {message}");
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Erreur publication: {ex.Message}");
                Console.ResetColor();
            }
        }

        public void Stop()
        {
            _weatherTimer?.Dispose();
            _pollutionTimer?.Dispose();
            _eventTimer?.Dispose();
            _session?.Close();
            _connection?.Close();
            Console.WriteLine("🛑 Service de notifications arrêté.");
        }
    }
}