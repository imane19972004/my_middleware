////using System.ServiceModel;
////using System.Threading.Tasks;

////namespace ProxyServer
////{
////    /// <summary>
////    /// Interface du service JCDecaux avec cache
////    /// ✅ CORRECTION: Actions explicites pour WCF
////    /// </summary>
////    [ServiceContract(Namespace = "http://tempuri.org/")]
////    public interface IJCDService
////    {
////        /// <summary>
////        /// Récupère toutes les stations d'un contrat
////        /// </summary>
////        [OperationContract(Action = "http://tempuri.org/IJCDService/GetStations")]
////        Task<string> GetStationsAsync(string contractName);

////        /// <summary>
////        /// Récupère une station par son numéro
////        /// </summary>
////        [OperationContract(Action = "http://tempuri.org/IJCDService/GetStation")]
////        Task<string> GetStationAsync(string contractName, int stationNumber);

////        /// <summary>
////        /// Trouve la station la plus proche avec assez de vélos
////        /// </summary>
////        [OperationContract(Action = "http://tempuri.org/IJCDService/GetClosestStation")]
////        Task<Station> GetClosestStationAsync(SimplifiedGeoCoordinate coordinates, string city, int minBikes);

////        /// <summary>
////        /// Invalide le cache d'un contrat
////        /// </summary>
////        [OperationContract(Action = "http://tempuri.org/IJCDService/InvalidateCache")]
////        void InvalidateContractCache(string contractName);
////    }
////}


//using System.ServiceModel;
//using System.Threading.Tasks;

//namespace ProxyServer
//{
//    [ServiceContract(Namespace = "http://tempuri.org/")]
//    public interface IJCDService
//    {
//        [OperationContract(Action = "http://tempuri.org/IJCDService/GetStations",
//                          ReplyAction = "http://tempuri.org/IJCDService/GetStationsResponse")]
//        Task<string> GetStationsAsync(string contractName);

//        [OperationContract(Action = "http://tempuri.org/IJCDService/GetStation",
//                          ReplyAction = "http://tempuri.org/IJCDService/GetStationResponse")]
//        Task<string> GetStationAsync(string contractName, int stationNumber);

//        [OperationContract(Action = "http://tempuri.org/IJCDService/GetClosestStationAsync",
//                          ReplyAction = "http://tempuri.org/IJCDService/GetClosestStationAsyncResponse")]
//        Task<Station> GetClosestStationAsync(SimplifiedGeoCoordinate coordinates, string city, int minBikes);

//        [OperationContract(Action = "http://tempuri.org/IJCDService/InvalidateCache",
//                          ReplyAction = "http://tempuri.org/IJCDService/InvalidateCacheResponse")]
//        void InvalidateContractCache(string contractName);
//    }
//}


using System.ServiceModel;
using System.Threading.Tasks;

namespace ProxyServer
{
    [ServiceContract(Namespace = "http://tempuri.org/")]
    public interface IJCDService
    {
        [OperationContract(Action = "http://tempuri.org/IJCDService/GetStations",
                          ReplyAction = "http://tempuri.org/IJCDService/GetStationsResponse")]
        Task<string> GetStationsAsync(string contractName);

        [OperationContract(Action = "http://tempuri.org/IJCDService/GetStation",
                          ReplyAction = "http://tempuri.org/IJCDService/GetStationResponse")]
        Task<string> GetStationAsync(string contractName, int stationNumber);

        // ✅ CRITIQUE: Le nom de l'opération doit correspondre à ce que RoutingServer envoie
        [OperationContract(Action = "http://tempuri.org/IJCDService/GetClosestStationAsync",
                          ReplyAction = "http://tempuri.org/IJCDService/GetClosestStationAsyncResponse",
                          Name = "GetClosestStationAsync")]  // ← AJOUTÉ
        Task<Station> GetClosestStationAsync(SimplifiedGeoCoordinate coordinates, string city, int minBikes);

        [OperationContract(Action = "http://tempuri.org/IJCDService/InvalidateCache",
                          ReplyAction = "http://tempuri.org/IJCDService/InvalidateCacheResponse")]
        void InvalidateContractCache(string contractName);
    }
}