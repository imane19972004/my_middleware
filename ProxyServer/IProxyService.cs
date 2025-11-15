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
        Task<string> GetStationsAsync(string contractName);//Lister toutes les stations d'une ville

        /// <summary>
        /// Récupère une station spécifique par son numéro
        /// </summary>
        [OperationContract]
        Task<string> GetStationByNumberAsync(string contractName, int stationNumber);//Station précise
    }
}





// Cette interface définit le contrat SOAP que votre service doit respecter:ce que les clients veront
