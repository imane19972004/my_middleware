using System;
using System.Linq;
using System.Net.Http;
using System.ServiceModel;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ProxyServer
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class JCDService : IJCDService
    {
        private readonly HttpClient _httpClient;
        private readonly CachingServer.CachingService _cache;
        private readonly string _apiKey;

        public JCDService()
        {
            _httpClient = new HttpClient();
            _cache = new CachingServer.CachingService();

            // ✅ Récupérer la clé API depuis les variables d'environnement
            _apiKey = Environment.GetEnvironmentVariable(Constants.EnvJcdecauxApiKey);

            if (string.IsNullOrEmpty(_apiKey))
            {
                Console.WriteLine("⚠️ AVERTISSEMENT: Variable JCDECAUX_API_KEY non trouvée !");
                Console.WriteLine("   Définissez-la avec: setx JCDECAUX_API_KEY \"votre_clé\"");
                _apiKey = "DEMO_KEY"; // Fallback pour tests
            }
        }

        public async Task<string> GetStationsAsync(string contractName)
        {
            var cacheKey = $"stations_{contractName}";

            try
            {
                var result = await _cache.GetOrSet(
                    cacheKey,
                    async () =>
                    {
                        var url = $"{Constants.JcdBaseAddress}?contract={contractName}&apiKey={_apiKey}";
                        Console.WriteLine($"🌐 API JCDecaux: {contractName}");
                        return await _httpClient.GetStringAsync(url);
                    },
                    CachingServer.CacheDefaults.StationStatusTTL
                );

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur GetStationsAsync: {ex.Message}");
                throw new FaultException($"Impossible de récupérer les stations: {ex.Message}");
            }
        }

        public async Task<string> GetStationAsync(string contractName, int stationNumber)
        {
            var cacheKey = $"station_{contractName}_{stationNumber}";

            try
            {
                var result = await _cache.GetOrSet(
                    cacheKey,
                    async () =>
                    {
                        var url = $"{Constants.JcdBaseAddress}/{stationNumber}?contract={contractName}&apiKey={_apiKey}";
                        return await _httpClient.GetStringAsync(url);
                    },
                    CachingServer.CacheDefaults.StationStatusTTL
                );

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur GetStationAsync: {ex.Message}");
                throw new FaultException($"Station {stationNumber} introuvable: {ex.Message}");
            }
        }

        public async Task<Station> GetClosestStationAsync(SimplifiedGeoCoordinate coordinates, string city, int minBikes)
        {
            try
            {
                // 1. Récupérer toutes les stations de la ville
                var jsonStations = await GetStationsAsync(city);
                var stations = JsonConvert.DeserializeObject<Station[]>(jsonStations);

                if (stations == null || stations.Length == 0)
                {
                    Console.WriteLine($"⚠️ Aucune station trouvée pour {city}");
                    return null;
                }

                // 2. Filtrer les stations ouvertes avec assez de vélos
                var availableStations = stations
                    .Where(s => s.status == Station.StatusOpen && s.available_bikes >= minBikes)
                    .ToList();

                if (!availableStations.Any())
                {
                    Console.WriteLine($"⚠️ Aucune station avec {minBikes} vélos disponibles");
                    return null;
                }

                // 3. Trouver la plus proche
                var closest = availableStations
                    .OrderBy(s => CalculateDistance(
                        coordinates.Latitude, coordinates.Longitude,
                        s.position.lat, s.position.lng))
                    .First();

                Console.WriteLine($"✅ Station trouvée: {closest.name} ({closest.available_bikes} vélos)");
                return closest;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur GetClosestStationAsync: {ex.Message}");
                return null;
            }
        }

        public void InvalidateContractCache(string contractName)
        {
            _cache.InvalidateCache($"stations_{contractName}");
            Console.WriteLine($"🧹 Cache invalidé pour {contractName}");
        }

        // ✅ Calcul de distance haversine
        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            var R = 6371000; // Rayon Terre en mètres
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private double ToRadians(double degrees) => degrees * Math.PI / 180;
    }
}


/*
 Pourquoi ?

GetOrSet du cache évite de surcharger l'API JCDecaux
Le cache expire après 5 minutes pour avoir des données fraîches
On retourne du JSON brut (le client le parsera)
 */

//C'est le coeur du proxyServer:Il appelle JCDecaux mais en utilisant le cache