using System;
using System.ServiceModel;
using System.ServiceModel.Description;

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
                    // ✅ Configuration du binding
                    var binding = new BasicHttpBinding
                    {
                        MaxReceivedMessageSize = 2147483647,
                        MaxBufferSize = 2147483647,
                        SendTimeout = TimeSpan.FromMinutes(10),
                        ReceiveTimeout = TimeSpan.FromMinutes(10)
                    };

                    host.AddServiceEndpoint(typeof(IRoutingService), binding, "");

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
}