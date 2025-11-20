//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net.Http;
//using System.Threading.Tasks;
//using Newtonsoft.Json.Linq;

//namespace RoutingServer
//{
//    public class OpenRouteService
//    {
//        private readonly HttpClient _httpClient;
//        private const string NOMINATIM_API = "https://nominatim.openstreetmap.org/search";
//        private const string OPENROUTE_API = "https://api.openrouteservice.org/v2/directions";
//        private const string OPENROUTE_KEY = "5b3ce3597851110001cf6248a1e8e2a8d14746b39f1a8f64e5b0f6c5"; // Clé publique de demo

//        public OpenRouteService()
//        {
//            _httpClient = new HttpClient();
//            _httpClient.DefaultRequestHeaders.Add("User-Agent", "LetsGoBiking/1.0");
//        }

//        /// <summary>
//        /// Géocode une adresse via Nominatim - TOUJOURS utilisé maintenant
//        /// </summary>
//        public async Task<Position> GeocodeAddress(string address)
//        {
//            if (string.IsNullOrWhiteSpace(address))
//                return null;

//            try
//            {
//                Console.WriteLine($"   🌍 Géocodage: {address}");

//                var url = $"{NOMINATIM_API}?q={Uri.EscapeDataString(address)}&format=json&limit=1&countrycodes=fr&addressdetails=1";

//                var response = await _httpClient.GetStringAsync(url);
//                var results = JArray.Parse(response);

//                if (results.Count == 0)
//                {
//                    Console.WriteLine($"   ❌ Adresse non trouvée: {address}");
//                    return null;
//                }

//                var firstResult = results[0];
//                var position = new Position
//                {
//                    lat = double.Parse(firstResult["lat"].ToString(), System.Globalization.CultureInfo.InvariantCulture),
//                    lng = double.Parse(firstResult["lon"].ToString(), System.Globalization.CultureInfo.InvariantCulture)
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
//                var url = $"{OPENROUTE_API}/{profile}?api_key={OPENROUTE_KEY}&start={start.lng},{start.lat}&end={end.lng},{end.lat}";

//                Console.WriteLine($"   🗺️ Calcul itinéraire {profile}...");

//                var response = await _httpClient.GetStringAsync(url);
//                var json = JObject.Parse(response);

//                var features = json["features"];
//                if (features == null || !features.Any())
//                {
//                    Console.WriteLine($"   ⚠️ Pas d'itinéraire trouvé, utilisation distance directe");
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

//                Console.WriteLine($"   ✅ Itinéraire: {distance:F0}m, {duration:F0}s, {waypoints.Count} points");

//                return new RouteSegment
//                {
//                    Distance = Math.Round(distance, 1),
//                    Duration = Math.Round(duration, 1),
//                    Waypoints = waypoints
//                };
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"   ⚠️ OpenRouteService échoué: {ex.Message}, utilisation distance directe");
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

//            return new RouteSegment
//            {
//                Distance = Math.Round(distance, 1),
//                Duration = Math.Round(duration, 1),
//                Waypoints = new List<Position> { start, end } // Juste ligne droite
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


//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net.Http;
//using System.Threading.Tasks;
//using Newtonsoft.Json.Linq;

//namespace RoutingServer
//{
//    public class OpenRouteService
//    {
//        private readonly HttpClient _httpClient;
//        private const string NOMINATIM_API = "https://nominatim.openstreetmap.org/search";

//        // ✅ OSRM est 100% gratuit, sans clé API nécessaire
//        private const string OSRM_FOOT_API = "https://routing.openstreetmap.de/routed-foot/route/v1";
//        private const string OSRM_BIKE_API = "https://routing.openstreetmap.de/routed-bike/route/v1";

//        public OpenRouteService()
//        {
//            _httpClient = new HttpClient();
//            _httpClient.DefaultRequestHeaders.Add("User-Agent", "LetsGoBiking/1.0");
//            Console.WriteLine("✅ Utilisation OSRM (gratuit, sans clé API)");
//        }

//        /// <summary>
//        /// Géocode une adresse via Nominatim
//        /// </summary>
//        public async Task<Position> GeocodeAddress(string address)
//        {
//            if (string.IsNullOrWhiteSpace(address))
//                return null;

//            try
//            {
//                Console.WriteLine($"   🌍 Géocodage: {address}");

//                var url = $"{NOMINATIM_API}?q={Uri.EscapeDataString(address)}&format=json&limit=1&countrycodes=fr&addressdetails=1";

//                var response = await _httpClient.GetStringAsync(url);
//                var results = JArray.Parse(response);

//                if (results.Count == 0)
//                {
//                    Console.WriteLine($"   ❌ Adresse non trouvée: {address}");
//                    return null;
//                }

//                var firstResult = results[0];
//                var position = new Position
//                {
//                    lat = double.Parse(firstResult["lat"].ToString(), System.Globalization.CultureInfo.InvariantCulture),
//                    lng = double.Parse(firstResult["lon"].ToString(), System.Globalization.CultureInfo.InvariantCulture)
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
//        /// Récupère un itinéraire piéton via OSRM
//        /// </summary>
//        public async Task<RouteSegment> GetWalkingRoute(Position start, Position end)
//        {
//            return await GetOSRMRoute(start, end, OSRM_FOOT_API, "foot");
//        }

//        /// <summary>
//        /// Récupère un itinéraire vélo via OSRM
//        /// </summary>
//        public async Task<RouteSegment> GetCyclingRoute(Position start, Position end)
//        {
//            return await GetOSRMRoute(start, end, OSRM_BIKE_API, "bike");
//        }

//        /// <summary>
//        /// Appelle OSRM pour obtenir un itinéraire réel
//        /// </summary>
//        private async Task<RouteSegment> GetOSRMRoute(Position start, Position end, string apiBase, string profile)
//        {
//            try
//            {
//                // Format OSRM : {lon},{lat};{lon},{lat}
//                var url = $"{apiBase}/driving/{start.lng},{start.lat};{end.lng},{end.lat}?overview=full&geometries=geojson";

//                Console.WriteLine($"   🗺️ Calcul itinéraire {profile} via OSRM...");

//                var response = await _httpClient.GetStringAsync(url);
//                var json = JObject.Parse(response);

//                var code = json["code"]?.ToString();
//                if (code != "Ok")
//                {
//                    Console.WriteLine($"   ⚠️ OSRM erreur: {code}, utilisation distance directe");
//                    return GetDirectRoute(start, end, profile);
//                }

//                var routes = json["routes"];
//                if (routes == null || !routes.Any())
//                {
//                    Console.WriteLine($"   ⚠️ Pas de route OSRM, utilisation distance directe");
//                    return GetDirectRoute(start, end, profile);
//                }

//                var route = routes[0];
//                var distance = (double)route["distance"]; // mètres
//                var duration = (double)route["duration"]; // secondes
//                var geometry = route["geometry"]["coordinates"] as JArray;

//                // Extraire les waypoints
//                var waypoints = new List<Position>();
//                foreach (var coord in geometry)
//                {
//                    waypoints.Add(new Position
//                    {
//                        lng = (double)coord[0],
//                        lat = (double)coord[1]
//                    });
//                }

//                Console.WriteLine($"   ✅ Itinéraire OSRM: {distance:F0}m, {duration:F0}s, {waypoints.Count} points");

//                return new RouteSegment
//                {
//                    Distance = Math.Round(distance, 1),
//                    Duration = Math.Round(duration, 1),
//                    Waypoints = waypoints
//                };
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"   ⚠️ OSRM échoué: {ex.Message}, utilisation distance directe");
//                return GetDirectRoute(start, end, profile);
//            }
//        }

//        /// <summary>
//        /// Calcul de distance directe (fallback si API échoue)
//        /// </summary>
//        private RouteSegment GetDirectRoute(Position start, Position end, string profile)
//        {
//            var distance = CalculateDistanceMeters(start, end);

//            // Ajouter un facteur de détour
//            if (profile == "foot")
//                distance *= 1.25; // +25% pour piétons
//            else
//                distance *= 1.15; // +15% pour vélo

//            var speed = profile == "foot" ? 1.4 : 4.5; // m/s
//            var duration = distance / speed;

//            Console.WriteLine($"   📏 Distance directe: {distance:F0}m (facteur appliqué)");

//            return new RouteSegment
//            {
//                Distance = Math.Round(distance, 1),
//                Duration = Math.Round(duration, 1),
//                Waypoints = new List<Position> { start, end } // Juste ligne droite
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
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace RoutingServer
{
    public class OpenRouteService
    {
        private readonly HttpClient _httpClient;
        private const string NOMINATIM_API = "https://nominatim.openstreetmap.org/search";
        private const string OPENROUTE_API = "https://api.openrouteservice.org/v2/directions";
        private const string OPENROUTE_KEY = "5b3ce3597851110001cf6248a1e8e2a8d14746b39f1a8f64e5b0f6c5"; // Clé publique de demo

        public OpenRouteService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "LetsGoBiking/1.0");
        }

        /// <summary>
        /// Géocode une adresse via Nominatim - TOUJOURS utilisé maintenant
        /// </summary>
        public async Task<Position> GeocodeAddress(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                return null;

            try
            {
                Console.WriteLine($"   🌍 Géocodage: {address}");

                var url = $"{NOMINATIM_API}?q={Uri.EscapeDataString(address)}&format=json&limit=1&countrycodes=fr&addressdetails=1";

                var response = await _httpClient.GetStringAsync(url);
                var results = JArray.Parse(response);

                if (results.Count == 0)
                {
                    Console.WriteLine($"   ❌ Adresse non trouvée: {address}");
                    return null;
                }

                var firstResult = results[0];
                var position = new Position
                {
                    lat = double.Parse(firstResult["lat"].ToString(), System.Globalization.CultureInfo.InvariantCulture),
                    lng = double.Parse(firstResult["lon"].ToString(), System.Globalization.CultureInfo.InvariantCulture)
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
                var url = $"{OPENROUTE_API}/{profile}?api_key={OPENROUTE_KEY}&start={start.lng},{start.lat}&end={end.lng},{end.lat}";

                Console.WriteLine($"   🗺️ Calcul itinéraire {profile}...");

                var response = await _httpClient.GetStringAsync(url);
                var json = JObject.Parse(response);

                var features = json["features"];
                if (features == null || !features.Any())
                {
                    Console.WriteLine($"   ⚠️ Pas d'itinéraire trouvé, utilisation distance directe");
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

                Console.WriteLine($"   ✅ Itinéraire: {distance:F0}m, {duration:F0}s, {waypoints.Count} points");

                return new RouteSegment
                {
                    Distance = Math.Round(distance, 1),
                    Duration = Math.Round(duration, 1),
                    Waypoints = waypoints
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ⚠️ OpenRouteService échoué: {ex.Message}, utilisation distance directe");
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

            return new RouteSegment
            {
                Distance = Math.Round(distance, 1),
                Duration = Math.Round(duration, 1),
                Waypoints = new List<Position> { start, end } // Juste ligne droite
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