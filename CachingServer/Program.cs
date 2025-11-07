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

            using (var host = new ServiceHost(typeof(CachingServer), baseAddress))
            {
                // Configuration pour exposer les métadonnées (WSDL)
                var smb = new ServiceMetadataBehavior
                {
                    HttpGetEnabled = true,
                    MetadataExporter = { PolicyVersion = PolicyVersion.Policy15 }
                };

                host.Description.Behaviors.Add(smb);

                // Démarrage du serveur
                host.Open();

                Console.WriteLine("✅ CachingServer lancé sur " + Constants.BaseAddress);
                Console.WriteLine("📋 WSDL disponible sur " + Constants.BaseAddress + "?wsdl");
                Console.WriteLine("\n⏸️  Appuyez sur ENTRÉE pour arrêter le serveur...\n");
                Console.ReadLine();

                host.Close();
            }
        }
    }
}

//Lance un serveur SOAP qui écoute sur mon URL déjà défini 
//Attend qu'on appuie sur ENTREE pour s'arrêter
