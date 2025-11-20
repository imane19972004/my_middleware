//using System;
//using System.Collections.Generic;
//using System.Globalization;
//using System.Linq;
//using System.Net.Http;
//using System.Text;
//using System.Threading.Tasks;
//using Newtonsoft.Json.Linq;

//namespace RoutingServer
//{
//    public class OpenRouteService
//    {
//        private readonly HttpClient _httpClient;
//        private const string NOMINATIM_API = "https://nominatim.openstreetmap.org/search";
//        private const string OPENROUTE_API = "https://api.openrouteservice.org/v2/directions";
//        private const string OPENROUTE_KEY = "5b3ce3597851110001cf6248a1e8e2a8d14746b39f1a8f64e5b0f6c5";

//        public OpenRouteService()
//        {
//            _httpClient = new HttpClient();
//            _httpClient.DefaultRequestHeaders.Add("User-Agent", "LetsGoBiking/1.0");
//        }

//        /// <summary>
//        /// Normalise les accents dans une chaîne (Aéroport → Aeroport)
//        /// </summary>
//        private string RemoveAccents(string text)
//        {
//            if (string.IsNullOrWhiteSpace(text))
//                return text;

//            // Méthode plus robuste pour enlever les accents
//            var normalizedString = text.Normalize(NormalizationForm.FormD);
//            var stringBuilder = new StringBuilder(capacity: normalizedString.Length);

//            for (int i = 0; i < normalizedString.Length; i++)
//            {
//                char c = normalizedString[i];
//                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
//                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
//                {
//                    stringBuilder.Append(c);
//                }
//            }

//            return stringBuilder
//                .ToString()
//                .Normalize(NormalizationForm.FormC);
//        }

//        /// <summary>
//        /// Géocode une adresse via Nominatim avec support des accents
//        /// </summary>
//        public async Task<Position> GeocodeAddress(string address)
//        {
//            if (string.IsNullOrWhiteSpace(address))
//                return null;

//            try
//            {
//                // Essayer D'ABORD avec l'adresse originale (avec accents)
//                Console.WriteLine($"   🌍 Géocodage: '{address}'");

//                var url = $"{NOMINATIM_API}?q={Uri.EscapeDataString(address)}&format=json&limit=1&countrycodes=fr&addressdetails=1";

//                var response = await _httpClient.GetStringAsync(url);
//                var results = JArray.Parse(response);

//                // Si aucun résultat, réessayer SANS accents
//                if (results.Count == 0)
//                {
//                    var addressWithoutAccents = RemoveAccents(address);
//                    Console.WriteLine($"   🔄 Réessai sans accents: '{addressWithoutAccents}'");

//                    url = $"{NOMINATIM_API}?q={Uri.EscapeDataString(addressWithoutAccents)}&format=json&limit=1&countrycodes=fr&addressdetails=1";
//                    response = await _httpClient.GetStringAsync(url);
//                    results = JArray.Parse(response);
//                }

//                if (results.Count == 0)
//                {
//                    Console.WriteLine($"   ❌ Adresse non trouvée: {address}");
//                    return null;
//                }

//                var firstResult = results[0];
//                var position = new Position
//                {
//                    lat = double.Parse(firstResult["lat"].ToString(), CultureInfo.InvariantCulture),
//                    lng = double.Parse(firstResult["lon"].ToString(), CultureInfo.InvariantCulture)
//                };

//                var displayName = firstResult["display_name"]?.ToString();
//                Console.WriteLine($"   ✅ Trouvé: {displayName}");
//                Console.WriteLine($"      GPS: {position.lat:F6}, {position.lng:F6}");

//                await Task.Delay(1100); // Rate limiting Nominatim
//                return position;
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"   ❌ Erreur géocodage: {ex.Message}");
//                return null;
//            }
//        }

//        /// <summary>
//        /// Récupère un itinéraire piéton RÉEL avec les routes
//        /// </summary>
//        public async Task<RouteSegment> GetWalkingRoute(Position start, Position end)
//        {
//            return await GetRealRoute(start, end, "foot-walking");
//        }

//        /// <summary>
//        /// Récupère un itinéraire vélo RÉEL avec les routes
//        /// </summary>
//        public async Task<RouteSegment> GetCyclingRoute(Position start, Position end)
//        {
//            return await GetRealRoute(start, end, "cycling-regular");
//        }

//        /// <summary>
//        /// Appelle OpenRouteService pour obtenir un itinéraire réel
//        /// </summary>
//        private async Task<RouteSegment> GetRealRoute(Position start, Position end, string profile)
//        {
//            try
//            {
//                var url = $"{OPENROUTE_API}/{profile}?api_key={OPENROUTE_KEY}&start={start.lng.ToString(CultureInfo.InvariantCulture)},{start.lat.ToString(CultureInfo.InvariantCulture)}&end={end.lng.ToString(CultureInfo.InvariantCulture)},{end.lat.ToString(CultureInfo.InvariantCulture)}";

//                Console.WriteLine($"   🗺️ Calcul itinéraire {profile}...");
//                Console.WriteLine($"      De: {start.lat:F6}, {start.lng:F6}");
//                Console.WriteLine($"      À:  {end.lat:F6}, {end.lng:F6}");

//                var response = await _httpClient.GetStringAsync(url);
//                var json = JObject.Parse(response);

//                var features = json["features"];
//                if (features == null || !features.Any())
//                {
//                    Console.WriteLine($"   ⚠️ Pas d'itinéraire trouvé dans l'API, utilisation distance directe");
//                    return GetDirectRoute(start, end, profile);
//                }

//                var geometry = features[0]["geometry"]["coordinates"] as JArray;
//                var properties = features[0]["properties"]["segments"][0];

//                var distance = (double)properties["distance"];
//                var duration = (double)properties["duration"];

//                // Extraire les coordonnées du chemin
//                var waypoints = new List<Position>();
//                foreach (var coord in geometry)
//                {
//                    waypoints.Add(new Position
//                    {
//                        lng = (double)coord[0],
//                        lat = (double)coord[1]
//                    });
//                }

//                Console.WriteLine($"   ✅ Itinéraire OpenRouteService: {distance:F0}m, {duration:F0}s");
//                Console.WriteLine($"      Nombre de waypoints: {waypoints.Count}");
//                Console.WriteLine($"      Premier waypoint: {waypoints.First().lat:F6}, {waypoints.First().lng:F6}");
//                Console.WriteLine($"      Dernier waypoint: {waypoints.Last().lat:F6}, {waypoints.Last().lng:F6}");

//                if (waypoints.Count < 2)
//                {
//                    Console.WriteLine($"   ⚠️ Pas assez de waypoints, utilisation distance directe");
//                    return GetDirectRoute(start, end, profile);
//                }

//                return new RouteSegment
//                {
//                    Distance = Math.Round(distance, 1),
//                    Duration = Math.Round(duration, 1),
//                    Waypoints = waypoints
//                };
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"   ⚠️ OpenRouteService échoué: {ex.Message}");
//                Console.WriteLine($"      Utilisation distance directe comme fallback");
//                return GetDirectRoute(start, end, profile);
//            }
//        }

//        /// <summary>
//        /// Calcul de distance directe (fallback si API échoue)
//        /// </summary>
//        private RouteSegment GetDirectRoute(Position start, Position end, string profile)
//        {
//            var distance = CalculateDistanceMeters(start, end);

//            // Ajouter un facteur de détour selon le profil
//            if (profile.Contains("foot"))
//                distance *= 1.25; // +25% pour piétons (détours)
//            else
//                distance *= 1.15; // +15% pour vélo

//            var speed = profile.Contains("foot") ? 1.4 : 4.5; // m/s
//            var duration = distance / speed;

//            Console.WriteLine($"   📏 Distance directe calculée: {distance:F0}m (facteur appliqué)");
//            Console.WriteLine($"      ⚠️ ATTENTION: Ligne droite, pas de vraie route!");

//            return new RouteSegment
//            {
//                Distance = Math.Round(distance, 1),
//                Duration = Math.Round(duration, 1),
//                Waypoints = new List<Position> { start, end } // ⚠️ Juste ligne droite
//            };
//        }

//        /// <summary>
//        /// Calcule la distance haversine entre deux points
//        /// </summary>
//        public double CalculateDistanceMeters(Position pos1, Position pos2)
//        {
//            const double R = 6371000;

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
//        public List<Position> Waypoints { get; set; } // Points du tracé
//    }
//}



using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace RoutingServer
{
    public class OpenRouteService
    {
        private readonly HttpClient _httpClient;
        private const string NOMINATIM_API = "https://nominatim.openstreetmap.org/search";
        private const string OPENROUTE_API = "https://api.openrouteservice.org/v2/directions";
        private const string OPENROUTE_KEY = "5b3ce3597851110001cf6248a1e8e2a8d14746b39f1a8f64e5b0f6c5";

        public OpenRouteService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "LetsGoBiking/1.0");
        }

        /// <summary>
        /// Normalise les accents dans une chaîne (Aéroport → Aeroport)
        /// </summary>
        private string RemoveAccents(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            // Méthode plus robuste pour enlever les accents
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder(capacity: normalizedString.Length);

            for (int i = 0; i < normalizedString.Length; i++)
            {
                char c = normalizedString[i];
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder
                .ToString()
                .Normalize(NormalizationForm.FormC);
        }

        /// <summary>
        /// Géocode une adresse via Nominatim avec support des accents
        /// </summary>
        public async Task<Position> GeocodeAddress(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                return null;

            try
            {
                // Essayer D'ABORD avec l'adresse originale (avec accents)
                Console.WriteLine($"   🌍 Géocodage: '{address}'");

                var url = $"{NOMINATIM_API}?q={Uri.EscapeDataString(address)}&format=json&limit=1&countrycodes=fr&addressdetails=1";

                var response = await _httpClient.GetStringAsync(url);
                var results = JArray.Parse(response);

                // Si aucun résultat, réessayer SANS accents
                if (results.Count == 0)
                {
                    var addressWithoutAccents = RemoveAccents(address);
                    Console.WriteLine($"   🔄 Réessai sans accents: '{addressWithoutAccents}'");

                    url = $"{NOMINATIM_API}?q={Uri.EscapeDataString(addressWithoutAccents)}&format=json&limit=1&countrycodes=fr&addressdetails=1";
                    response = await _httpClient.GetStringAsync(url);
                    results = JArray.Parse(response);
                }

                if (results.Count == 0)
                {
                    Console.WriteLine($"   ❌ Adresse non trouvée: {address}");
                    return null;
                }

                var firstResult = results[0];
                var position = new Position
                {
                    lat = double.Parse(firstResult["lat"].ToString(), CultureInfo.InvariantCulture),
                    lng = double.Parse(firstResult["lon"].ToString(), CultureInfo.InvariantCulture)
                };

                var displayName = firstResult["display_name"]?.ToString();
                Console.WriteLine($"   ✅ Trouvé: {displayName}");
                Console.WriteLine($"      GPS: {position.lat:F6}, {position.lng:F6}");

                await Task.Delay(1100); // Rate limiting Nominatim
                return position;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ❌ Erreur géocodage: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Récupère un itinéraire piéton RÉEL avec les routes
        /// </summary>
        public async Task<RouteSegment> GetWalkingRoute(Position start, Position end)
        {
            return await GetRealRoute(start, end, "foot-walking");
        }

        /// <summary>
        /// Récupère un itinéraire vélo RÉEL avec les routes
        /// </summary>
        public async Task<RouteSegment> GetCyclingRoute(Position start, Position end)
        {
            return await GetRealRoute(start, end, "cycling-regular");
        }

        /// <summary>
        /// Appelle OpenRouteService pour obtenir un itinéraire réel
        /// </summary>
        private async Task<RouteSegment> GetRealRoute(Position start, Position end, string profile)
        {
            try
            {
                var url = $"{OPENROUTE_API}/{profile}?api_key={OPENROUTE_KEY}&start={start.lng.ToString(CultureInfo.InvariantCulture)},{start.lat.ToString(CultureInfo.InvariantCulture)}&end={end.lng.ToString(CultureInfo.InvariantCulture)},{end.lat.ToString(CultureInfo.InvariantCulture)}";

                Console.WriteLine($"   🗺️ Calcul itinéraire {profile}...");
                Console.WriteLine($"      De: {start.lat:F6}, {start.lng:F6}");
                Console.WriteLine($"      À:  {end.lat:F6}, {end.lng:F6}");

                var response = await _httpClient.GetStringAsync(url);
                var json = JObject.Parse(response);

                var features = json["features"];
                if (features == null || !features.Any())
                {
                    Console.WriteLine($"   ⚠️ Pas d'itinéraire trouvé dans l'API, utilisation distance directe");
                    return GetDirectRoute(start, end, profile);
                }

                var geometry = features[0]["geometry"]["coordinates"] as JArray;
                var properties = features[0]["properties"]["segments"][0];

                var distance = (double)properties["distance"];
                var duration = (double)properties["duration"];

                // Extraire les coordonnées du chemin
                var waypoints = new List<Position>();
                foreach (var coord in geometry)
                {
                    waypoints.Add(new Position
                    {
                        lng = (double)coord[0],
                        lat = (double)coord[1]
                    });
                }

                Console.WriteLine($"   ✅ Itinéraire OpenRouteService: {distance:F0}m, {duration:F0}s");
                Console.WriteLine($"      Nombre de waypoints: {waypoints.Count}");
                Console.WriteLine($"      Premier waypoint: {waypoints.First().lat:F6}, {waypoints.First().lng:F6}");
                Console.WriteLine($"      Dernier waypoint: {waypoints.Last().lat:F6}, {waypoints.Last().lng:F6}");

                if (waypoints.Count < 2)
                {
                    Console.WriteLine($"   ⚠️ Pas assez de waypoints, utilisation distance directe");
                    return GetDirectRoute(start, end, profile);
                }

                return new RouteSegment
                {
                    Distance = Math.Round(distance, 1),
                    Duration = Math.Round(duration, 1),
                    Waypoints = waypoints
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ⚠️ OpenRouteService échoué: {ex.Message}");
                Console.WriteLine($"      Utilisation distance directe comme fallback");
                return GetDirectRoute(start, end, profile);
            }
        }

        /// <summary>
        /// Calcul de distance directe (fallback si API échoue)
        /// </summary>
        private RouteSegment GetDirectRoute(Position start, Position end, string profile)
        {
            var distance = CalculateDistanceMeters(start, end);

            // Ajouter un facteur de détour selon le profil
            if (profile.Contains("foot"))
                distance *= 1.25; // +25% pour piétons (détours)
            else
                distance *= 1.15; // +15% pour vélo

            var speed = profile.Contains("foot") ? 1.4 : 4.5; // m/s
            var duration = distance / speed;

            Console.WriteLine($"   📏 Distance directe calculée: {distance:F0}m (facteur appliqué)");
            Console.WriteLine($"      ⚠️ ATTENTION: Ligne droite, pas de vraie route!");

            return new RouteSegment
            {
                Distance = Math.Round(distance, 1),
                Duration = Math.Round(duration, 1),
                Waypoints = new List<Position> { start, end } // ⚠️ Juste ligne droite
            };
        }

        /// <summary>
        /// Calcule la distance haversine entre deux points
        /// </summary>
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

    public class RouteSegment
    {
        public double Distance { get; set; } // mètres
        public double Duration { get; set; } // secondes
        public List<Position> Waypoints { get; set; } // Points du tracé
    }
}