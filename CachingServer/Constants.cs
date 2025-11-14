using System;

namespace CachingServer
{
    public static class Constants
    {
        public const string BaseAddress = "http://localhost:8002/CachingServer";
    }

    public static class CacheDefaults
    {
        // TTL par type de données
        public static readonly TimeSpan StationsMetadataTTL = TimeSpan.FromHours(24);
        public static readonly TimeSpan StationStatusTTL = TimeSpan.FromSeconds(15);
        public static readonly TimeSpan ContractTTL = TimeSpan.FromHours(6);
        public static readonly TimeSpan RouteResultTTL = TimeSpan.FromSeconds(20);
    }
}


//Stocker l'adress URL ,où mon serveur sera accessible (const:ne change jamais)
//Permet de changer l'URL facilement sans modifier tous le code
//Ajouter de Cacheefaults pour centraliser la durée de vie des différents types de données
