using System.ServiceModel;
using System.Threading.Tasks;

namespace ProxyServer
{
    /// <summary>
    /// Interface du service JCDecaux avec cache
    /// ✅ CORRECTION: Actions explicites pour WCF
    /// </summary>
    [ServiceContract(Namespace = "http://tempuri.org/")]
    public interface IJCDService
    {
        /// <summary>
        /// Récupère toutes les stations d'un contrat
        /// </summary>
        [OperationContract(Action = "http://tempuri.org/IJCDService/GetStations")]
        Task<string> GetStationsAsync(string contractName);

        /// <summary>
        /// Récupère une station par son numéro
        /// </summary>
        [OperationContract(Action = "http://tempuri.org/IJCDService/GetStation")]
        Task<string> GetStationAsync(string contractName, int stationNumber);

        /// <summary>
        /// Trouve la station la plus proche avec assez de vélos
        /// </summary>
        [OperationContract(Action = "http://tempuri.org/IJCDService/GetClosestStation")]
        Task<Station> GetClosestStationAsync(SimplifiedGeoCoordinate coordinates, string city, int minBikes);

        /// <summary>
        /// Invalide le cache d'un contrat
        /// </summary>
        [OperationContract(Action = "http://tempuri.org/IJCDService/InvalidateCache")]
        void InvalidateContractCache(string contractName);
    }
}