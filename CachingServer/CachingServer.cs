using System;
using System.Linq;
using System.Runtime.Caching;
using System.ServiceModel;
using System.Threading.Tasks;

namespace CachingServer
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class CachingServer
    {
        private static readonly MemoryCache _cache = MemoryCache.Default;

        public async Task<T> GetOrSet<T>(string cacheKey, Func<Task<T>> retrieveFunction, TimeSpan duration)
            where T : class
        {
            if (!_cache.Contains(cacheKey))
            {
                Console.WriteLine("🔴 Cache MISS pour : " + cacheKey);
                var data = await retrieveFunction();
                if (data != null)
                {
                    _cache.Add(cacheKey, data, new CacheItemPolicy
                    {
                        AbsoluteExpiration = DateTimeOffset.Now.Add(duration)
                    });
                }
                return data;
            }
            else
            {
                Console.WriteLine("✅ Cache HIT pour : " + cacheKey);
                return _cache.Get(cacheKey) as T;
            }
        }

        public void ClearCache()
        {
            var cacheItems = _cache.ToList();
            foreach (var item in cacheItems)
                _cache.Remove(item.Key);
            Console.WriteLine("🧹 Cache vidé !");
        }
    }
}




//GetOrSet<T> => vérifie si une donnée est dans le cache .Si oui,la retourne .Sinon , l'ajoute
//ClearCache() => Vide tout le cache