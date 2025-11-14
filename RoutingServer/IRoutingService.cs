using RoutingServer;
using System.ServiceModel;
using System.Threading.Tasks;

[ServiceContract]
public interface IRoutingService
{
    [OperationContract]
    Task<ItineraryResponse> GetItinerary(ItineraryRequest request);
} 