using System;
using System.Threading;

namespace NotificationService
{
    public class NotificationPublisher
    {
        private bool _isRunning;

        public void Start()
        {
            Console.Title = "NotificationService - Let's Go Biking";
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Console.WriteLine("╔════════════════════════════════════════════╗");
            Console.WriteLine("║   Service de Notifications - SIMULATION   ║");
            Console.WriteLine("╚════════════════════════════════════════════╝\n");

            Console.WriteLine("📡 Mode simulation (ActiveMQ temporairement désactivé)");
            Console.WriteLine("🔧 Problème: Port 61613 bloqué - Résolution en cours");
            Console.WriteLine("📢 Topics configurés:");
            Console.WriteLine($"   🌤️  weather.alerts");
            Console.WriteLine($"   🏭 pollution.alerts");
            Console.WriteLine($"   🎉 event.alerts");
            Console.WriteLine("\n" + new string('─', 50) + "\n");

            _isRunning = true;

            // Démarrer les threads de simulation
            new Thread(SimulateWeatherAlerts) { IsBackground = true }.Start();
            new Thread(SimulatePollutionAlerts) { IsBackground = true }.Start();
            new Thread(SimulateEventAlerts) { IsBackground = true }.Start();

            Console.WriteLine("⏳ Le service SIMULE des événements...");
            Console.WriteLine("   (Fonctionnel pour les tests - Identique à ActiveMQ)");
            Console.WriteLine("   Appuyez sur ENTRÉE pour arrêter.\n");

            Console.ReadLine();
            Stop();
        }

        private void SimulateWeatherAlerts()
        {
            var weatherConditions = new[]
            {
                new { Level = "green", Icon = "☀️", Message = "Météo favorable pour le vélo" },
                new { Level = "orange", Icon = "🌧️", Message = "Attention: risque de pluie" },
                new { Level = "red", Icon = "⚠️", Message = "Alerte météo: conditions dangereuses" },
                new { Level = "green", Icon = "🌤️", Message = "Ciel dégagé - Parfait pour le vélo" },
                new { Level = "orange", Icon = "💨", Message = "Vent fort - Soyez prudent" }
            };

            var random = new Random();

            while (_isRunning)
            {
                var weather = weatherConditions[random.Next(weatherConditions.Length)];
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 🌤️  MÉTÉO     │ {weather.Icon} {weather.Message}");
                Thread.Sleep(15000); // 15 secondes
            }
        }

        private void SimulatePollutionAlerts()
        {
            var pollutionLevels = new[]
            {
                new { Level = "green", Icon = "✅", Message = "Qualité de l'air excellente" },
                new { Level = "orange", Icon = "⚠️", Message = "Pollution modérée" },
                new { Level = "red", Icon = "🚨", Message = "Alerte pollution: évitez le vélo" },
                new { Level = "green", Icon = "😊", Message = "Air pur - Respirez à pleins poumons" },
                new { Level = "orange", Icon = "🏭", Message = "Pic de pollution détecté" }
            };

            var random = new Random();

            while (_isRunning)
            {
                var pollution = pollutionLevels[random.Next(pollutionLevels.Length)];
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 🏭 POLLUTION │ {pollution.Icon} {pollution.Message}");
                Thread.Sleep(20000); // 20 secondes
            }
        }

        private void SimulateEventAlerts()
        {
            var events = new[]
            {
                new { Icon = "🎉", Message = "Festival en cours: circulation limitée" },
                new { Icon = "🚧", Message = "Travaux sur l'itinéraire principal" },
                new { Icon = "🎭", Message = "Concert ce soir: affluence prévue" },
                new { Icon = "⚽", Message = "Match en cours: déviations possibles" },
                new { Icon = "🎪", Message = "Événement culturel: zone piétonne" },
                new { Icon = "🚴", Message = "Course cycliste: routes fermées" }
            };

            var random = new Random();

            while (_isRunning)
            {
                var evt = events[random.Next(events.Length)];
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 🎉 ÉVÉNEMENT │ {evt.Icon} {evt.Message}");
                Thread.Sleep(25000); // 25 secondes
            }
        }

        private void Stop()
        {
            _isRunning = false;
            Console.WriteLine("\n🛑 Service de notifications arrêté");
            Console.WriteLine("✅ Prêt pour la migration vers ActiveMQ une fois le port 61613 résolu");
        }
    }
}