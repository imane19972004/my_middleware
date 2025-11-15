using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace RoutingServer
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var baseAddress = new Uri(Constants.BaseAddress);

                using (var host = new ServiceHost(typeof(RoutingService), baseAddress))
                {
                    // ✅ Configuration du binding avec CORS
                    var binding = new CustomBinding();

                    var textEncoding = new TextMessageEncodingBindingElement
                    {
                        MessageVersion = MessageVersion.Soap11
                    };

                    var httpTransport = new HttpTransportBindingElement
                    {
                        MaxReceivedMessageSize = 2147483647,
                        MaxBufferSize = 2147483647,
                        TransferMode = TransferMode.Buffered
                    };

                    binding.Elements.Add(textEncoding);
                    binding.Elements.Add(httpTransport);

                    host.AddServiceEndpoint(typeof(IRoutingService), binding, "");

                    // ✅ Ajouter le behavior CORS
                    foreach (ServiceEndpoint endpoint in host.Description.Endpoints)
                    {
                        endpoint.EndpointBehaviors.Add(new CorsEnablingBehavior());
                    }

                    // ✅ Exposer le WSDL
                    var smb = new ServiceMetadataBehavior
                    {
                        HttpGetEnabled = true,
                        MetadataExporter = { PolicyVersion = PolicyVersion.Policy15 }
                    };
                    host.Description.Behaviors.Add(smb);

                    // ✅ Démarrage
                    host.Open();

                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine("╔════════════════════════════════════════════╗");
                    Console.WriteLine("║      RoutingServer - DÉMARRÉ ✅           ║");
                    Console.WriteLine("║           CORS ACTIVÉ 🌐                  ║");
                    Console.WriteLine("╚════════════════════════════════════════════╝");
                    Console.ResetColor();
                    Console.WriteLine($"\n📍 URL:  {Constants.BaseAddress}");
                    Console.WriteLine($"📋 WSDL: {Constants.BaseAddress}?wsdl");
                    Console.WriteLine("\n🚴 Prêt à calculer des itinéraires vélo...");
                    Console.WriteLine("   Appuyez sur ENTRÉE pour arrêter.\n");

                    Console.ReadLine();
                    host.Close();
                    Console.WriteLine("🛑 RoutingServer arrêté.");
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n❌ ERREUR FATALE:");
                Console.WriteLine($"   Message: {ex.Message}");
                Console.WriteLine($"   Stack:   {ex.StackTrace}");
                Console.ResetColor();
                Console.WriteLine("\nAppuyez sur une touche pour quitter...");
                Console.ReadKey();
            }
        }
    }

    // ✅ CLASSE POUR ACTIVER CORS
    public class CorsEnablingBehavior : IEndpointBehavior
    {
        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters) { }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime) { }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            endpointDispatcher.DispatchRuntime.MessageInspectors.Add(new CorsMessageInspector());
        }

        public void Validate(ServiceEndpoint endpoint) { }
    }

    public class CorsMessageInspector : IDispatchMessageInspector
    {
        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            // Gérer les requêtes OPTIONS (preflight)
            var httpRequest = request.Properties[HttpRequestMessageProperty.Name] as HttpRequestMessageProperty;
            if (httpRequest != null && httpRequest.Method.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase))
            {
                return "OPTIONS";
            }
            return null;
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            // Ajouter les headers CORS à toutes les réponses
            if (reply.Properties.ContainsKey(HttpResponseMessageProperty.Name))
            {
                var httpResponse = reply.Properties[HttpResponseMessageProperty.Name] as HttpResponseMessageProperty;
                if (httpResponse != null)
                {
                    httpResponse.Headers.Add("Access-Control-Allow-Origin", "*");
                    httpResponse.Headers.Add("Access-Control-Allow-Methods", "POST, GET, OPTIONS");
                    httpResponse.Headers.Add("Access-Control-Allow-Headers", "Content-Type, SOAPAction");
                    httpResponse.Headers.Add("Access-Control-Max-Age", "86400");

                    // Si c'était une requête OPTIONS, renvoyer 200 OK
                    if (correlationState != null && correlationState.ToString() == "OPTIONS")
                    {
                        httpResponse.StatusCode = System.Net.HttpStatusCode.OK;
                    }
                }
            }
            else
            {
                var httpResponse = new HttpResponseMessageProperty();
                httpResponse.Headers.Add("Access-Control-Allow-Origin", "*");
                httpResponse.Headers.Add("Access-Control-Allow-Methods", "POST, GET, OPTIONS");
                httpResponse.Headers.Add("Access-Control-Allow-Headers", "Content-Type, SOAPAction");
                httpResponse.Headers.Add("Access-Control-Max-Age", "86400");
                reply.Properties.Add(HttpResponseMessageProperty.Name, httpResponse);
            }
        }
    }
}