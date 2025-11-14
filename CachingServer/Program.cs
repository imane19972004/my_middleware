using System;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace CachingServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var baseAddress = new Uri(Constants.BaseAddress);

            using (var host = new ServiceHost(typeof(CachingService), baseAddress))
            {
                // ✅ Configuration du binding BasicHttpBinding
                var binding = new BasicHttpBinding
                {
                    MaxReceivedMessageSize = 2147483647,
                    MaxBufferSize = 2147483647,
                    SendTimeout = TimeSpan.FromMinutes(10),
                    ReceiveTimeout = TimeSpan.FromMinutes(10)
                };

                host.AddServiceEndpoint(typeof(ICachingService), binding, "");

                // ✅ Exposer le WSDL
                var smb = new ServiceMetadataBehavior
                {
                    HttpGetEnabled = true,
                    MetadataExporter = { PolicyVersion = PolicyVersion.Policy15 }
                };
                host.Description.Behaviors.Add(smb);

                // ✅ Démarrage
                host.Open();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("╔════════════════════════════════════════════╗");
                Console.WriteLine("║       CachingServer - DÉMARRÉ ✅          ║");
                Console.WriteLine("╚════════════════════════════════════════════╝");
                Console.ResetColor();
                Console.WriteLine($"\n📍 URL:  {Constants.BaseAddress}");
                Console.WriteLine($"📋 WSDL: {Constants.BaseAddress}?wsdl");
                Console.WriteLine("\n💾 Cache système prêt...");
                Console.WriteLine("   Appuyez sur ENTRÉE pour arrêter.\n");

                Console.ReadLine();
                host.Close();
                Console.WriteLine("🛑 CachingServer arrêté.");
            }
        }
    }
}

//Lance un serveur SOAP qui écoute sur mon URL déjà défini 
//Attend qu'on appuie sur ENTREE pour s'arrêter
//Program.cs ="J'ouvre un restau qui serve CachingServer , les clients peuvent consulter mon menu (?wsdl) et passer commande tant que je suis ouvert"
