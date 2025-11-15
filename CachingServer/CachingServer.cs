using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.Caching;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;

namespace CachingServer
{
    // ✅ Interface du contrat SOAP
    [ServiceContract]
    public interface ICachingService
    {
        [OperationContract]
        Task<string> GetOrSetStringAsync(string cacheKey, string defaultValue, int durationSeconds);

        [OperationContract]
        void InvalidateCache(string cacheKey);

        [OperationContract]
        void ClearAllCache();
    }

    // ✅ Implémentation du service
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class CachingService : ICachingService
    {
        private static readonly MemoryCache _cache = MemoryCache.Default;
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks
            = new ConcurrentDictionary<string, SemaphoreSlim>();

        // Méthode générique interne (non exposée en SOAP)
        public async Task<T> GetOrSet<T>(string cacheKey, Func<Task<T>> retrieveFunction, TimeSpan duration)
            where T : class
        {
            if (_cache.Contains(cacheKey))
            {
                Console.WriteLine($"✅ Cache HIT: {cacheKey}");
                return _cache.Get(cacheKey) as T;
            }

            Console.WriteLine($"🔴 Cache MISS: {cacheKey}");

            // Protection contre stampede
            var semaphore = _locks.GetOrAdd(cacheKey, k => new SemaphoreSlim(1, 1));

            await semaphore.WaitAsync();
            try
            {
                // Double-check après acquisition du lock
                if (_cache.Contains(cacheKey))
                {
                    return _cache.Get(cacheKey) as T;
                }

                var data = await retrieveFunction();
                if (data != null)
                {
                    _cache.Set(cacheKey, data, DateTimeOffset.Now.Add(duration));
                    Console.WriteLine($"💾 Cache SET: {cacheKey} (expire dans {duration.TotalSeconds}s)");
                }
                return data;
            }
            finally
            {
                semaphore.Release();
            }
        }

        // ✅ Méthode SOAP pour les strings (utilisable par ProxyServer)
        public async Task<string> GetOrSetStringAsync(string cacheKey, string defaultValue, int durationSeconds)
        {
            return await GetOrSet(
                cacheKey,
                async () => { await Task.Delay(1); return defaultValue; },
                TimeSpan.FromSeconds(durationSeconds)
            );
        }

        // ✅ Invalidation d'une clé
        public void InvalidateCache(string cacheKey)
        {
            _cache.Remove(cacheKey);
            Console.WriteLine($"🧹 Cache invalidé: {cacheKey}");
        }

        // ✅ Vider tout le cache
        public void ClearAllCache()
        {
            var cacheItems = _cache.ToList();
            foreach (var item in cacheItems)
                _cache.Remove(item.Key);

            Console.WriteLine("🧹 Cache entièrement vidé !");
        }
    }
}

//Ajouter une protection contre stampede avec ConcurrentDictionary + Lazy<Task>
//Ajout de la méthode Invalidate() pour mettre à jour des données critiques