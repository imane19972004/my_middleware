using System;
using System.Net.Http;
using System.ServiceModel;
using System.Threading.Tasks;

namespace ProxyServer
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ProxyService : IProxyService
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly CachingServer.CachingServer _cache = new CachingServer.CachingServer();

        // Votre clé API JCDecaux (à mettre dans un fichier .env plus tard)
        private const string JCDECAUX_API_KEY = "VOTRE_CLE_API_ICI";

        public async Task<string> GetStationsAsync(string contractName)
        {
            var cacheKey = $"stations_{contractName}";

            // On vérifie d'abord le cache
            var result = await _cache.GetOrSet(
                cacheKey,
                async () =>
                {
                    var url = $"https://api.jcdecaux.com/vls/v1/stations?contract={contractName}&apiKey={JCDECAUX_API_KEY}";
                    var response = await _httpClient.GetStringAsync(url);
                    return response;
                },
                TimeSpan.FromMinutes(5) // Cache pendant 5 minutes
            );

            return result;
        }

        public async Task<string> GetStationByNumberAsync(string contractName, int stationNumber)
        {
            var cacheKey = $"station_{contractName}_{stationNumber}";

            var result = await _cache.GetOrSet(
                cacheKey,
                async () =>
                {
                    var url = $"https://api.jcdecaux.com/vls/v1/stations/{stationNumber}?contract={contractName}&apiKey={JCDECAUX_API_KEY}";
                    var response = await _httpClient.GetStringAsync(url);
                    return response;
                },
                TimeSpan.FromMinutes(5)
            );

            return result;
        }
    }
}



/*
 Pourquoi ?

GetOrSet du cache évite de surcharger l'API JCDecaux
Le cache expire après 5 minutes pour avoir des données fraîches
On retourne du JSON brut (le client le parsera)
 */