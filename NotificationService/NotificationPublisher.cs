using System;
using System.Threading;
using Apache.NMS;
using Apache.NMS.ActiveMQ;

namespace NotificationService
{
    /// <summary>
    /// Classe responsable de publier des notifications sur les topics ActiveMQ
    /// </summary>
    public class NotificationPublisher
    {
        private IConnectionFactory _factory;
        private IConnection _connection;
        private ISession _session;
        private bool _isRunning;

        /// <summary>
        /// Démarre le service de publication
        /// </summary>
        public void Start()
        {
            Console.WriteLine("╔════════════════════════════════════════════╗");
            Console.WriteLine("║   Service de Notifications - Démarrage    ║");
            Console.WriteLine("╚════════════════════════════════════════════╝\n");

            try
            {
                // Connexion à ActiveMQ
                Console.WriteLine("📡 Connexion à ActiveMQ...");
                _factory = new ConnectionFactory(Constants.BrokerUri);
                _connection = _factory.CreateConnection();
                _connection.Start();
                _session = _connection.CreateSession(AcknowledgementMode.AutoAcknowledge);

                Console.WriteLine("✅ Connecté à ActiveMQ sur " + Constants.BrokerUri);
                Console.WriteLine("\n📢 Topics configurés:");
                Console.WriteLine($"   🌤️  {Constants.WeatherTopic}");
                Console.WriteLine($"   🏭 {Constants.PollutionTopic}");
                Console.WriteLine($"   🎉 {Constants.EventTopic}");
                Console.WriteLine("\n" + new string('─', 50) + "\n");

                _isRunning = true;

                // Lancer les threads de publication
                new Thread(PublishWeatherAlerts) { IsBackground = true }.Start();
                new Thread(PublishPollutionAlerts) { IsBackground = true }.Start();
                new Thread(PublishEventAlerts) { IsBackground = true }.Start();

                Console.WriteLine("⏳ Le service publie des événements...");
                Console.WriteLine("   Appuyez sur ENTRÉE pour arrêter.\n");
                Console.ReadLine();

                Stop();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur lors du démarrage: {ex.Message}");
                Console.WriteLine("\n⚠️  Vérifiez qu'ActiveMQ est bien lancé !");
            }
        }

        /// <summary>
        /// Publie des alertes météo toutes les 15 secondes
        /// </summary>
        private void PublishWeatherAlerts()
        {
            var destination = _session.GetTopic(Constants.WeatherTopic);
            var producer = _session.CreateProducer(destination);

            var weatherConditions = new[]
            {
                new { Level = "green", Icon = "☀️", Message = "Météo favorable pour le vélo" },
                new { Level = "orange", Icon = "🌧️", Message = "Attention: risque de pluie" },
                new { Level = "red", Icon = "⚠️", Message = "Alerte météo: conditions dangereuses" }
            };

            var random = new Random();

            while (_isRunning)
            {
                try
                {
                    var weather = weatherConditions[random.Next(weatherConditions.Length)];
                    var jsonMessage = $"{{\"type\":\"weather\",\"level\":\"{weather.Level}\",\"icon\":\"{weather.Icon}\",\"message\":\"{weather.Message}\",\"timestamp\":\"{DateTime.Now:HH:mm:ss}\"}}";

                    var message = _session.CreateTextMessage(jsonMessage);
                    producer.Send(message);

                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 🌤️  MÉTÉO     │ {weather.Icon} {weather.Message}");

                    Thread.Sleep(Constants.WeatherInterval);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Erreur météo: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Publie des alertes pollution toutes les 20 secondes
        /// </summary>
        private void PublishPollutionAlerts()
        {
            var destination = _session.GetTopic(Constants.PollutionTopic);
            var producer = _session.CreateProducer(destination);

            var pollutionLevels = new[]
            {
                new { Level = "green", Icon = "✅", Message = "Qualité de l'air excellente" },
                new { Level = "orange", Icon = "⚠️", Message = "Pollution modérée" },
                new { Level = "red", Icon = "🚨", Message = "Alerte pollution: évitez le vélo" }
            };

            var random = new Random();

            while (_isRunning)
            {
                try
                {
                    var pollution = pollutionLevels[random.Next(pollutionLevels.Length)];
                    var jsonMessage = $"{{\"type\":\"pollution\",\"level\":\"{pollution.Level}\",\"icon\":\"{pollution.Icon}\",\"message\":\"{pollution.Message}\",\"timestamp\":\"{DateTime.Now:HH:mm:ss}\"}}";

                    var message = _session.CreateTextMessage(jsonMessage);
                    producer.Send(message);

                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 🏭 POLLUTION │ {pollution.Icon} {pollution.Message}");

                    Thread.Sleep(Constants.PollutionInterval);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Erreur pollution: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Publie des événements toutes les 25 secondes
        /// </summary>
        private void PublishEventAlerts()
        {
            var destination = _session.GetTopic(Constants.EventTopic);
            var producer = _session.CreateProducer(destination);

            var events = new[]
            {
                new { Icon = "🎉", Message = "Festival en cours: circulation limitée" },
                new { Icon = "🚧", Message = "Travaux sur l'itinéraire principal" },
                new { Icon = "🎭", Message = "Concert ce soir: affluence prévue" },
                new { Icon = "⚽", Message = "Match en cours: déviations possibles" },
                new { Icon = "🎪", Message = "Événement culturel: zone piétonne" }
            };

            var random = new Random();

            while (_isRunning)
            {
                try
                {
                    var evt = events[random.Next(events.Length)];
                    var jsonMessage = $"{{\"type\":\"event\",\"level\":\"orange\",\"icon\":\"{evt.Icon}\",\"message\":\"{evt.Message}\",\"timestamp\":\"{DateTime.Now:HH:mm:ss}\"}}";

                    var message = _session.CreateTextMessage(jsonMessage);
                    producer.Send(message);

                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 🎉 ÉVÉNEMENT │ {evt.Icon} {evt.Message}");

                    Thread.Sleep(Constants.EventInterval);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Erreur événement: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Arrête le service proprement
        /// </summary>
        private void Stop()
        {
            _isRunning = false;
            Console.WriteLine("\n🛑 Arrêt du service...");

            try
            {
                _session?.Close();
                _connection?.Close();
                Console.WriteLine("✅ Service arrêté proprement");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Erreur lors de l'arrêt: {ex.Message}");
            }
        }
    }
}


//(logique de publication)