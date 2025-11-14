using System;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace ProxyServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var baseAddress = new Uri(Constants.BaseAddress);

            using (var host = new ServiceHost(typeof(JCDService), baseAddress))
            {
                // ✅ Configuration du binding
                var binding = new BasicHttpBinding
                {
                    MaxReceivedMessageSize = 2147483647,
                    MaxBufferSize = 2147483647,
                    SendTimeout = TimeSpan.FromMinutes(10),
                    ReceiveTimeout = TimeSpan.FromMinutes(10)
                };

                host.AddServiceEndpoint(typeof(IJCDService), binding, "");

                // ✅ Exposer le WSDL
                var smb = new ServiceMetadataBehavior
                {
                    HttpGetEnabled = true,
                    MetadataExporter = { PolicyVersion = PolicyVersion.Policy15 }
                };
                host.Description.Behaviors.Add(smb);

                // ✅ Démarrage
                host.Open();

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("╔════════════════════════════════════════════╗");
                Console.WriteLine("║       ProxyServer - DÉMARRÉ ✅            ║");
                Console.WriteLine("╚════════════════════════════════════════════╝");
                Console.ResetColor();
                Console.WriteLine($"\n📍 URL:  {Constants.BaseAddress}");
                Console.WriteLine($"📋 WSDL: {Constants.BaseAddress}?wsdl");
                Console.WriteLine("\n🚲 Proxy JCDecaux avec cache activé...");
                Console.WriteLine("   Appuyez sur ENTRÉE pour arrêter.\n");

                Console.ReadLine();
                host.Close();
                Console.WriteLine("🛑 ProxyServer arrêté.");
            }
        }
    }
}

//1.Crée un hôte WCF pour ton service
//2.Rend mon ProxyServer accessible sur localhost:8001/ProxyServer
//Affiche l'URL du WSDL
//Attend une touche Entrée pour fermer le serveur 