//using System;
//using System.Collections.Generic;
//using System.Threading.Tasks;

//namespace RoutingServer
//{
//    public class OpenRouteService
//    {
//        // Base de données de villes françaises pour le géocodage
//        private readonly Dictionary<string, Position> _cityDatabase = new Dictionary<string, Position>(StringComparer.OrdinalIgnoreCase)
//        {
//            { "Paris", new Position { lat = 48.8566, lng = 2.3522 } },
//            { "Lyon", new Position { lat = 45.7640, lng = 4.8357 } },
//            { "Marseille", new Position { lat = 43.2965, lng = 5.3698 } },
//            { "Toulouse", new Position { lat = 43.6047, lng = 1.4442 } },
//            { "Nice", new Position { lat = 43.7102, lng = 7.2620 } },
//            { "Nantes", new Position { lat = 47.2184, lng = -1.5536 } },
//            { "Strasbourg", new Position { lat = 48.5734, lng = 7.7521 } },
//            { "Montpellier", new Position { lat = 43.6108, lng = 3.8767 } },
//            { "Bordeaux", new Position { lat = 44.8378, lng = -0.5792 } },
//            { "Lille", new Position { lat = 50.6292, lng = 3.0573 } },
//            { "Rennes", new Position { lat = 48.1173, lng = -1.6778 } },
//            { "Reims", new Position { lat = 49.2583, lng = 4.0317 } },
//            { "Le Havre", new Position { lat = 49.4944, lng = 0.1079 } },
//            { "Saint-Étienne", new Position { lat = 45.4397, lng = 4.3872 } },
//            { "Toulon", new Position { lat = 43.1242, lng = 5.9280 } },
//            { "Grenoble", new Position { lat = 45.1885, lng = 5.7245 } },
//            { "Dijon", new Position { lat = 47.3220, lng = 5.0415 } },
//            { "Angers", new Position { lat = 47.4784, lng = -0.5632 } },
//            { "Nîmes", new Position { lat = 43.8367, lng = 4.3601 } },
//            { "Villeurbanne", new Position { lat = 45.7667, lng = 4.8833 } },
//            { "Clermont-Ferrand", new Position { lat = 45.7772, lng = 3.0870 } },
//            { "Le Mans", new Position { lat = 48.0077, lng = 0.1984 } },
//            { "Aix-en-Provence", new Position { lat = 43.5297, lng = 5.4474 } },
//            { "Brest", new Position { lat = 48.3905, lng = -4.4861 } },
//            { "Tours", new Position { lat = 47.3941, lng = 0.6848 } },
//            { "Amiens", new Position { lat = 49.8942, lng = 2.2957 } },
//            { "Limoges", new Position { lat = 45.8336, lng = 1.2611 } },
//            { "Annecy", new Position { lat = 45.8992, lng = 6.1294 } },
//            { "Perpignan", new Position { lat = 42.6887, lng = 2.8948 } },
//            { "Boulogne-Billancourt", new Position { lat = 48.8350, lng = 2.2400 } }
//        };

//        public async Task<Position> GeocodeAddress(string address)
//        {
//            await Task.Delay(50); // Simulation temps API

//            if (string.IsNullOrWhiteSpace(address))
//                return null;

//            string normalized = address.Trim();

//            // Recherche exacte d'abord
//            if (_cityDatabase.TryGetValue(normalized, out var position))
//            {
//                Console.WriteLine($"   📍 Géocodé: {normalized} -> {position.lat:F4}, {position.lng:F4}");
//                return position;
//            }

//            // Recherche partielle (contient le nom de ville)
//            foreach (var kvp in _cityDatabase)
//            {
//                if (normalized.IndexOf(kvp.Key, StringComparison.OrdinalIgnoreCase) >= 0)
//                {
//                    Console.WriteLine($"   📍 Géocodé (partiel): {normalized} -> {kvp.Key}");
//                    return kvp.Value;
//                }
//            }

//            // Par défaut: Paris
//            Console.WriteLine($"   ⚠️ Ville inconnue '{address}', utilisation de Paris par défaut");
//            return _cityDatabase["Paris"];
//        }

//        public async Task<RouteSegment> GetWalkingRoute(Position start, Position end)
//        {
//            await Task.Delay(50);

//            var distance = CalculateDistanceMeters(start, end);
//            distance *= 1.25; // +25% pour les détours réels

//            var walkingSpeed = 1.4; // m/s (5 km/h)
//            var duration = distance / walkingSpeed;

//            return new RouteSegment
//            {
//                Distance = Math.Round(distance, 1),
//                Duration = Math.Round(duration)
//            };
//        }

//        public async Task<RouteSegment> GetCyclingRoute(Position start, Position end)
//        {
//            await Task.Delay(50);

//            var distance = CalculateDistanceMeters(start, end);
//            distance *= 1.15; // +15% pour les détours (moins qu'à pied)

//            var cyclingSpeed = 4.5; // m/s (16 km/h)
//            var duration = distance / cyclingSpeed;

//            return new RouteSegment
//            {
//                Distance = Math.Round(distance, 1),
//                Duration = Math.Round(duration)
//            };
//        }

//        /// <summary>
//        /// Calcule la distance haversine entre deux points GPS
//        /// </summary>
//        public double CalculateDistanceMeters(Position pos1, Position pos2)
//        {
//            const double R = 6371000; // Rayon de la Terre en mètres

//            var dLat = ToRadians(pos2.lat - pos1.lat);
//            var dLon = ToRadians(pos2.lng - pos1.lng);

//            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
//                    Math.Cos(ToRadians(pos1.lat)) * Math.Cos(ToRadians(pos2.lat)) *
//                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

//            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

//            return R * c;
//        }

//        private double ToRadians(double degrees)
//        {
//            return degrees * Math.PI / 180.0;
//        }
//    }

//    public class RouteSegment
//    {
//        public double Distance { get; set; } // mètres
//        public double Duration { get; set; } // secondes
//    }
//}


using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RoutingServer
{
    public class OpenRouteService
    {
        // Base de données avec les VRAIS noms de contrats JCDecaux
        private readonly Dictionary<string, CityInfo> _cityDatabase = new Dictionary<string, CityInfo>(StringComparer.OrdinalIgnoreCase)
        {
            // ✅ CONTRATS JCDECAUX VALIDES
            { "Lyon", new CityInfo { Position = new Position { lat = 45.7640, lng = 4.8357 }, ContractName = "Lyon" } },
            { "Toulouse", new CityInfo { Position = new Position { lat = 43.6047, lng = 1.4442 }, ContractName = "Toulouse" } },
            { "Marseille", new CityInfo { Position = new Position { lat = 43.2965, lng = 5.3698 }, ContractName = "Marseille" } },
            { "Nantes", new CityInfo { Position = new Position { lat = 47.2184, lng = -1.5536 }, ContractName = "Nantes" } },
            { "Montpellier", new CityInfo { Position = new Position { lat = 43.6108, lng = 3.8767 }, ContractName = "Montpellier" } },
            { "Strasbourg", new CityInfo { Position = new Position { lat = 48.5734, lng = 7.7521 }, ContractName = "Strasbourg" } },
            { "Bordeaux", new CityInfo { Position = new Position { lat = 44.8378, lng = -0.5792 }, ContractName = "Bordeaux" } },
            { "Lille", new CityInfo { Position = new Position { lat = 50.6292, lng = 3.0573 }, ContractName = "Lille" } },
            { "Rennes", new CityInfo { Position = new Position { lat = 48.1173, lng = -1.6778 }, ContractName = "lekotcha" } },
            { "Nice", new CityInfo { Position = new Position { lat = 43.7102, lng = 7.2620 }, ContractName = "Nice" } },
            
            // Paris utilise un nom de contrat différent
            { "Paris", new CityInfo { Position = new Position { lat = 48.8566, lng = 2.3522 }, ContractName = "Paris" } },
            
            // Autres villes sans vélos JCDecaux (pour tests)
            { "Grenoble", new CityInfo { Position = new Position { lat = 45.1885, lng = 5.7245 }, ContractName = null } },
            { "Dijon", new CityInfo { Position = new Position { lat = 47.3220, lng = 5.0415 }, ContractName = null } },
        };

        public async Task<Position> GeocodeAddress(string address)
        {
            await Task.Delay(50);

            if (string.IsNullOrWhiteSpace(address))
                return null;

            string normalized = address.Trim();

            // Recherche exacte
            if (_cityDatabase.TryGetValue(normalized, out var cityInfo))
            {
                Console.WriteLine($"   📍 Géocodé: {normalized} -> {cityInfo.Position.lat:F4}, {cityInfo.Position.lng:F4}");
                return cityInfo.Position;
            }

            // Recherche partielle
            foreach (var kvp in _cityDatabase)
            {
                if (normalized.IndexOf(kvp.Key, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    Console.WriteLine($"   📍 Géocodé (partiel): {normalized} -> {kvp.Key}");
                    return kvp.Value.Position;
                }
            }

            // Par défaut: Lyon (on sait qu'il marche !)
            Console.WriteLine($"   ⚠️ Ville inconnue '{address}', utilisation de Lyon par défaut");
            return _cityDatabase["Lyon"].Position;
        }

        public async Task<RouteSegment> GetWalkingRoute(Position start, Position end)
        {
            await Task.Delay(50);

            var distance = CalculateDistanceMeters(start, end);
            distance *= 1.25;

            var walkingSpeed = 1.4;
            var duration = distance / walkingSpeed;

            return new RouteSegment
            {
                Distance = Math.Round(distance, 1),
                Duration = Math.Round(duration)
            };
        }

        public async Task<RouteSegment> GetCyclingRoute(Position start, Position end)
        {
            await Task.Delay(50);

            var distance = CalculateDistanceMeters(start, end);
            distance *= 1.15;

            var cyclingSpeed = 4.5;
            var duration = distance / cyclingSpeed;

            return new RouteSegment
            {
                Distance = Math.Round(distance, 1),
                Duration = Math.Round(duration)
            };
        }

        public double CalculateDistanceMeters(Position pos1, Position pos2)
        {
            const double R = 6371000;

            var dLat = ToRadians(pos2.lat - pos1.lat);
            var dLon = ToRadians(pos2.lng - pos1.lng);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(pos1.lat)) * Math.Cos(ToRadians(pos2.lat)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c;
        }

        private double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }
    }

    public class CityInfo
    {
        public Position Position { get; set; }
        public string ContractName { get; set; }
    }

    public class RouteSegment
    {
        public double Distance { get; set; }
        public double Duration { get; set; }
    }
}