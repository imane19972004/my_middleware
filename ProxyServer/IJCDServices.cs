using System.ServiceModel;
using System.Threading.Tasks;

namespace ProxyServer
{
    /// <summary>
    /// Interface du service JCDecaux avec cache
    /// </summary>
    [ServiceContract]
    public interface IJCDService
    {
        /// <summary>
        /// Récupère toutes les stations d'un contrat
        /// </summary>
        [OperationContract]
        Task<string> GetStationsAsync(string contractName);

        /// <summary>
        /// Récupère une station par son numéro
        /// </summary>
        [OperationContract]
        Task<string> GetStationAsync(string contractName, int stationNumber);

        /// <summary>
        /// Trouve la station la plus proche avec assez de vélos
        /// </summary>
        [OperationContract]
        Task<Station> GetClosestStationAsync(SimplifiedGeoCoordinate coordinates, string city, int minBikes);

        /// <summary>
        /// Invalide le cache d'un contrat
        /// </summary>
        [OperationContract]
        void InvalidateContractCache(string contractName);
    }
}