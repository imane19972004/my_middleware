using System.ServiceModel;
using System.Threading.Tasks;

namespace ProxyServer
{
    [ServiceContract]
    public interface IProxyService
    {
        /// <summary>
        /// Récupère toutes les stations d'un contrat JCDecaux
        /// </summary>
        [OperationContract]
        Task<string> GetStationsAsync(string contractName);

        /// <summary>
        /// Récupère une station spécifique par son numéro
        /// </summary>
        [OperationContract]
        Task<string> GetStationByNumberAsync(string contractName, int stationNumber);
    }
}





// Cette interface définit le contrat SOAP que votre service doit respecter.