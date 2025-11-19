//using RoutingServer;
//using System.ServiceModel;
//using System.Threading.Tasks;

//[ServiceContract]
//public interface IRoutingService
//{
//    [OperationContract]
//    Task<ItineraryResponse> GetItinerary(ItineraryRequest request);
//} 

//using RoutingServer;
//using System.ServiceModel;
//using System.Threading.Tasks;

//[ServiceContract(Namespace = "http://tempuri.org/")]
//public interface IRoutingService
//{
//    [OperationContract(Action = "http://tempuri.org/IRoutingService/GetItinerary")]
//    Task<ItineraryResponse> GetItinerary(ItineraryRequest request);
//}



//using RoutingServer;
//using System.ServiceModel;
//using System.Threading.Tasks;

//[ServiceContract(Namespace = "http://tempuri.org/")]
//public interface IRoutingService
//{
//    [OperationContract(Action = "http://tempuri.org/IRoutingService/GetItinerary")]
//    //Task<ItineraryResponse> GetItinerary(ItineraryRequest request);
//    Task<ItineraryResponse> GetItinerary(string origin , string destination);
//}

//using RoutingServer;
//using System.ServiceModel;
//using System.Threading.Tasks;

//[ServiceContract(Namespace = "http://tempuri.org/")]
//public interface IRoutingService
//{
//    [OperationContract(Action = "http://tempuri.org/IRoutingService/GetItinerary")]
//    Task<ItineraryResponse> GetItinerary(string Origin, string Destination, int MinBikes);
//}



using RoutingServer;
using System.ServiceModel;
using System.Threading.Tasks;

[ServiceContract(Namespace = "http://tempuri.org/")]
public interface IRoutingService
{
    [OperationContract(Action = "http://tempuri.org/IRoutingService/GetItinerary",
                       ReplyAction = "http://tempuri.org/IRoutingService/GetItineraryResponse")]
    Task<ItineraryResponse> GetItinerary(ItineraryRequest request);
}