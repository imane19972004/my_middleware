//using System;
//using System.ServiceModel;
//using System.ServiceModel.Description;
//using dotenv.net;

//namespace ProxyServer
//{
//    class Program
//    {
//        static void Main(string[] args)
//        {
//            DotEnv.Load();

//            var baseAddress = new Uri(Constants.BaseAddress);

//            using (var host = new ServiceHost(typeof(JCDService), baseAddress))
//            {
//                var smb = new ServiceMetadataBehavior
//                {
//                    HttpGetEnabled = true,
//                    MetadataExporter = { PolicyVersion = PolicyVersion.Policy15 }
//                };

//                host.Description.Behaviors.Add(smb);

//                host.Open();

//                Console.WriteLine("✅ ProxyServer lancé sur " + Constants.BaseAddress);
//                Console.WriteLine("📋 WSDL disponible sur " + Constants.BaseAddress + "?wsdl");
//                Console.WriteLine("\n⏸️  Appuyez sur ENTRÉE pour arrêter le serveur...\n");
//                Console.ReadLine();

//                host.Close();
//            }
//        }
//    }
//}




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

            using (var host = new ServiceHost(typeof(ProxyService), baseAddress))
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

                Console.WriteLine("✅ ProxyServer lancé sur " + Constants.BaseAddress);
                Console.WriteLine("📋 WSDL disponible sur " + Constants.BaseAddress + "?wsdl");
                Console.WriteLine("\n⏸️  Appuyez sur ENTRÉE pour arrêter le serveur...\n");
                Console.ReadLine();

                host.Close();
            }
        }
    }
}