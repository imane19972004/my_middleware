////using System;
////using System.Net.Http;
////using System.ServiceModel;
////using System.Text;
////using System.Threading.Tasks;
////using System.Xml;

////namespace RoutingServer
////{
////    /// <summary>
////    /// Client SOAP pour communiquer avec ProxyServer (qui lui-même appelle JCDecaux)
////    /// </summary>
////    public class JCDecauxProxy
////    {
////        private readonly HttpClient _httpClient;

////        public JCDecauxProxy()
////        {
////            _httpClient = new HttpClient
////            {
////                Timeout = TimeSpan.FromSeconds(30)
////            };
////        }

////        public async Task<Station> GetClosestStation(Position position, int minBikes)
////        {
////            try
////            {
////                // Déterminer la ville basée sur les coordonnées
////                string city = DetermineCity(position);

////                Console.WriteLine($"   🌐 Appel ProxyServer pour {city}...");

////                // ✅ Construction de la requête SOAP correcte
////                var soapEnvelope = $@"<?xml version=""1.0"" encoding=""utf-8""?>
////<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/""
////               xmlns:tem=""http://tempuri.org/"">
////  <soap:Body>
////    <tem:GetClosestStationAsync>
////      <tem:coordinates>
////        <tem:Latitude>{position.lat.ToString(System.Globalization.CultureInfo.InvariantCulture)}</tem:Latitude>
////        <tem:Longitude>{position.lng.ToString(System.Globalization.CultureInfo.InvariantCulture)}</tem:Longitude>
////      </tem:coordinates>
////      <tem:city>{city}</tem:city>
////      <tem:minBikes>{minBikes}</tem:minBikes>
////    </tem:GetClosestStationAsync>
////  </soap:Body>
////</soap:Envelope>";

////                var content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml");
////                content.Headers.Clear();
////                content.Headers.Add("Content-Type", "text/xml; charset=utf-8");
////                content.Headers.Add("SOAPAction", "http://tempuri.org/IJCDService/GetClosestStationAsync");

////                var response = await _httpClient.PostAsync(Constants.ProxyServiceUrl, content);

////                if (!response.IsSuccessStatusCode)
////                {
////                    var errorContent = await response.Content.ReadAsStringAsync();
////                    Console.WriteLine($"   ❌ Erreur HTTP {response.StatusCode}: {errorContent}");
////                    return null;
////                }

////                var responseXml = await response.Content.ReadAsStringAsync();
////                return ParseStationFromSoapResponse(responseXml);
////            }
////            catch (Exception ex)
////            {
////                Console.WriteLine($"   ❌ Erreur JCDecauxProxy: {ex.Message}");
////                return null;
////            }
////        }

////        private Station ParseStationFromSoapResponse(string soapResponse)
////        {
////            try
////            {
////                var doc = new XmlDocument();
////                doc.LoadXml(soapResponse);

////                // Namespaces SOAP
////                var nsmgr = new XmlNamespaceManager(doc.NameTable);
////                nsmgr.AddNamespace("s", "http://schemas.xmlsoap.org/soap/envelope/");
////                nsmgr.AddNamespace("a", "http://tempuri.org/");

////                // Extraire les données
////                var stationNode = doc.SelectSingleNode("//a:GetClosestStationAsyncResponse/a:GetClosestStationAsyncResult", nsmgr);

////                if (stationNode == null)
////                {
////                    Console.WriteLine("   ⚠️ Aucune station trouvée dans la réponse SOAP");
////                    return null;
////                }

////                var station = new Station
////                {
////                    number = int.Parse(GetNodeValue(stationNode, "a:number", nsmgr) ?? "0"),
////                    name = GetNodeValue(stationNode, "a:name", nsmgr),
////                    address = GetNodeValue(stationNode, "a:address", nsmgr),
////                    available_bikes = int.Parse(GetNodeValue(stationNode, "a:available_bikes", nsmgr) ?? "0"),
////                    available_bike_stands = int.Parse(GetNodeValue(stationNode, "a:available_bike_stands", nsmgr) ?? "0"),
////                    status = GetNodeValue(stationNode, "a:status", nsmgr),
////                    position = new Position
////                    {
////                        lat = double.Parse(GetNodeValue(stationNode, "a:position/a:lat", nsmgr) ?? "0",
////                                          System.Globalization.CultureInfo.InvariantCulture),
////                        lng = double.Parse(GetNodeValue(stationNode, "a:position/a:lng", nsmgr) ?? "0",
////                                          System.Globalization.CultureInfo.InvariantCulture)
////                    }
////                };

////                return station;
////            }
////            catch (Exception ex)
////            {
////                Console.WriteLine($"   ❌ Erreur parsing SOAP: {ex.Message}");
////                return null;
////            }
////        }

////        private string GetNodeValue(XmlNode parent, string xpath, XmlNamespaceManager nsmgr)
////        {
////            var node = parent.SelectSingleNode(xpath, nsmgr);
////            return node?.InnerText;
////        }

////        /// <summary>
////        /// Détermine approximativement la ville d'après les coordonnées GPS
////        /// </summary>
////        private string DetermineCity(Position position)
////        {
////            // Mapping simplifié coordonnées -> ville JCDecaux
////            // Paris et région parisienne
////            if (Math.Abs(position.lat - 48.8566) < 0.5 && Math.Abs(position.lng - 2.3522) < 0.5)
////                return "Paris";

////            // Lyon
////            if (Math.Abs(position.lat - 45.7640) < 0.5 && Math.Abs(position.lng - 4.8357) < 0.5)
////                return "Lyon";

////            // Marseille
////            if (Math.Abs(position.lat - 43.2965) < 0.5 && Math.Abs(position.lng - 5.3698) < 0.5)
////                return "Marseille";

////            // Toulouse
////            if (Math.Abs(position.lat - 43.6047) < 0.5 && Math.Abs(position.lng - 1.4442) < 0.5)
////                return "Toulouse";

////            // Par défaut
////            return "Paris";
////        }
////    }
////}



//using System;
//using System.Net.Http;
//using System.ServiceModel;
//using System.Text;
//using System.Threading.Tasks;
//using System.Xml;

//namespace RoutingServer
//{
//    public class JCDecauxProxy
//    {
//        private readonly HttpClient _httpClient;

//        public JCDecauxProxy()
//        {
//            _httpClient = new HttpClient
//            {
//                Timeout = TimeSpan.FromSeconds(30)
//            };
//        }

//        public async Task<Station> GetClosestStation(Position position, int minBikes)
//        {
//            try
//            {
//                // ✅ CORRECTION: Meilleure détection de ville
//                string city = DetermineCity(position);

//                Console.WriteLine($"   🌐 Appel ProxyServer pour {city} (GPS: {position.lat:F4}, {position.lng:F4})...");

//                var soapEnvelope = $@"<?xml version=""1.0"" encoding=""utf-8""?>
//<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/""
//               xmlns:tem=""http://tempuri.org/"">
//  <soap:Body>
//    <tem:GetClosestStationAsync>
//      <tem:coordinates>
//        <tem:Latitude>{position.lat.ToString(System.Globalization.CultureInfo.InvariantCulture)}</tem:Latitude>
//        <tem:Longitude>{position.lng.ToString(System.Globalization.CultureInfo.InvariantCulture)}</tem:Longitude>
//      </tem:coordinates>
//      <tem:city>{city}</tem:city>
//      <tem:minBikes>{minBikes}</tem:minBikes>
//    </tem:GetClosestStationAsync>
//  </soap:Body>
//</soap:Envelope>";

//                var content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml");
//                content.Headers.Clear();
//                content.Headers.Add("Content-Type", "text/xml; charset=utf-8");
//                content.Headers.Add("SOAPAction", "http://tempuri.org/IJCDService/GetClosestStationAsync");

//                var response = await _httpClient.PostAsync(Constants.ProxyServiceUrl, content);

//                if (!response.IsSuccessStatusCode)
//                {
//                    var errorContent = await response.Content.ReadAsStringAsync();
//                    Console.WriteLine($"   ❌ Erreur HTTP {response.StatusCode}: {errorContent}");
//                    return null;
//                }

//                var responseXml = await response.Content.ReadAsStringAsync();
//                return ParseStationFromSoapResponse(responseXml);
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"   ❌ Erreur JCDecauxProxy: {ex.Message}");
//                return null;
//            }
//        }

//        private Station ParseStationFromSoapResponse(string soapResponse)
//        {
//            try
//            {
//                var doc = new XmlDocument();
//                doc.LoadXml(soapResponse);

//                var nsmgr = new XmlNamespaceManager(doc.NameTable);
//                nsmgr.AddNamespace("s", "http://schemas.xmlsoap.org/soap/envelope/");
//                nsmgr.AddNamespace("a", "http://tempuri.org/");

//                var stationNode = doc.SelectSingleNode("//a:GetClosestStationAsyncResponse/a:GetClosestStationAsyncResult", nsmgr);

//                if (stationNode == null)
//                {
//                    Console.WriteLine("   ⚠️ Aucune station trouvée dans la réponse SOAP");
//                    return null;
//                }

//                var station = new Station
//                {
//                    number = int.Parse(GetNodeValue(stationNode, "a:number", nsmgr) ?? "0"),
//                    name = GetNodeValue(stationNode, "a:name", nsmgr),
//                    address = GetNodeValue(stationNode, "a:address", nsmgr),
//                    available_bikes = int.Parse(GetNodeValue(stationNode, "a:available_bikes", nsmgr) ?? "0"),
//                    available_bike_stands = int.Parse(GetNodeValue(stationNode, "a:available_bike_stands", nsmgr) ?? "0"),
//                    status = GetNodeValue(stationNode, "a:status", nsmgr),
//                    position = new Position
//                    {
//                        lat = double.Parse(GetNodeValue(stationNode, "a:position/a:lat", nsmgr) ?? "0",
//                                          System.Globalization.CultureInfo.InvariantCulture),
//                        lng = double.Parse(GetNodeValue(stationNode, "a:position/a:lng", nsmgr) ?? "0",
//                                          System.Globalization.CultureInfo.InvariantCulture)
//                    }
//                };

//                return station;
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"   ❌ Erreur parsing SOAP: {ex.Message}");
//                return null;
//            }
//        }

//        private string GetNodeValue(XmlNode parent, string xpath, XmlNamespaceManager nsmgr)
//        {
//            var node = parent.SelectSingleNode(xpath, nsmgr);
//            return node?.InnerText;
//        }

//        /// <summary>
//        /// ✅ CORRECTION: Détection précise des villes avec JCDecaux
//        /// </summary>
//        private string DetermineCity(Position position)
//        {
//            // Nice et Côte d'Azur (7.0 à 7.3 longitude, 43.5 à 43.75 latitude)
//            if (position.lat >= 43.5 && position.lat <= 43.75 &&
//                position.lng >= 6.9 && position.lng <= 7.35)
//            {
//                Console.WriteLine($"   📍 Ville détectée: Nice (basé sur GPS)");
//                return "Nice";
//            }

//            // Lyon (4.7 à 4.95 longitude, 45.7 à 45.8 latitude)
//            if (position.lat >= 45.7 && position.lat <= 45.85 &&
//                position.lng >= 4.7 && position.lng <= 4.95)
//            {
//                Console.WriteLine($"   📍 Ville détectée: Lyon");
//                return "Lyon";
//            }

//            // Paris et région (2.2 à 2.45 longitude, 48.8 à 49.0 latitude)
//            if (position.lat >= 48.8 && position.lat <= 49.0 &&
//                position.lng >= 2.2 && position.lng <= 2.45)
//            {
//                Console.WriteLine($"   📍 Ville détectée: Paris");
//                return "Paris";
//            }

//            // Marseille (5.3 à 5.45 longitude, 43.25 à 43.35 latitude)
//            if (position.lat >= 43.25 && position.lat <= 43.35 &&
//                position.lng >= 5.3 && position.lng <= 5.45)
//            {
//                Console.WriteLine($"   📍 Ville détectée: Marseille");
//                return "Marseille";
//            }

//            // Toulouse (1.4 à 1.5 longitude, 43.55 à 43.65 latitude)
//            if (position.lat >= 43.55 && position.lat <= 43.65 &&
//                position.lng >= 1.4 && position.lng <= 1.5)
//            {
//                Console.WriteLine($"   📍 Ville détectée: Toulouse");
//                return "Toulouse";
//            }

//            // Par défaut, utiliser Lyon (ville avec beaucoup de stations)
//            Console.WriteLine($"   ⚠️ Ville non reconnue (GPS: {position.lat:F4}, {position.lng:F4}), utilisation de Lyon par défaut");
//            return "Lyon";
//        }
//    }
//}



using System;
using System.Collections.Generic;
using System.Net.Http;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace RoutingServer
{
    public class JCDecauxProxy
    {
        private readonly HttpClient _httpClient;

        public JCDecauxProxy()
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30)
            };
        }

        public async Task<Station> GetClosestStation(Position position, int minBikes)
        {
            try
            {
                // ✅ CORRECTION: Meilleure détection de ville
                string city = DetermineCity(position);

                Console.WriteLine($"   🌐 Appel ProxyServer pour {city} (GPS: {position.lat:F4}, {position.lng:F4})...");

                var soapEnvelope = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/""
               xmlns:tem=""http://tempuri.org/"">
  <soap:Body>
    <tem:GetClosestStationAsync>
      <tem:coordinates>
        <tem:Latitude>{position.lat.ToString(System.Globalization.CultureInfo.InvariantCulture)}</tem:Latitude>
        <tem:Longitude>{position.lng.ToString(System.Globalization.CultureInfo.InvariantCulture)}</tem:Longitude>
      </tem:coordinates>
      <tem:city>{city}</tem:city>
      <tem:minBikes>{minBikes}</tem:minBikes>
    </tem:GetClosestStationAsync>
  </soap:Body>
</soap:Envelope>";

                var content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml");
                content.Headers.Clear();
                content.Headers.Add("Content-Type", "text/xml; charset=utf-8");
                content.Headers.Add("SOAPAction", "http://tempuri.org/IJCDService/GetClosestStationAsync");

                var response = await _httpClient.PostAsync(Constants.ProxyServiceUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"   ❌ Erreur HTTP {response.StatusCode}: {errorContent}");
                    return null;
                }

                var responseXml = await response.Content.ReadAsStringAsync();
                return ParseStationFromSoapResponse(responseXml);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ❌ Erreur JCDecauxProxy: {ex.Message}");
                return null;
            }
        }


        public async Task<List<Station>> GetStationsAsync(string city)
        {
            try
            {
                Console.WriteLine($"   🌐 Appel ProxyServer pour récupérer toutes les stations de {city}...");

                var soapEnvelope = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/""
               xmlns:tem=""http://tempuri.org/"">
  <soap:Body>
    <tem:GetStationsByCityAsync>
      <tem:city>{city}</tem:city>
    </tem:GetStationsByCityAsync>
  </soap:Body>
</soap:Envelope>";

                var content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml");
                content.Headers.Clear();
                content.Headers.Add("Content-Type", "text/xml; charset=utf-8");
                content.Headers.Add("SOAPAction", "http://tempuri.org/IJCDService/GetStationsByCityAsync");

                var response = await _httpClient.PostAsync(Constants.ProxyServiceUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"   ❌ Erreur HTTP {response.StatusCode}: {errorContent}");
                    return null;
                }

                var responseXml = await response.Content.ReadAsStringAsync();
                return ParseStationsListFromSoapResponse(responseXml);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ❌ Erreur JCDecauxProxy.GetStationsAsync: {ex.Message}");
                return null;
            }
        }

        private List<Station> ParseStationsListFromSoapResponse(string soapResponse)
        {
            var stations = new List<Station>();

            try
            {
                var doc = new XmlDocument();
                doc.LoadXml(soapResponse);

                var nsmgr = new XmlNamespaceManager(doc.NameTable);
                nsmgr.AddNamespace("s", "http://schemas.xmlsoap.org/soap/envelope/");
                nsmgr.AddNamespace("a", "http://tempuri.org/");

                var stationNodes = doc.SelectNodes("//a:GetStationsByCityAsyncResponse/a:GetStationsByCityAsyncResult/a:Station", nsmgr);

                if (stationNodes == null || stationNodes.Count == 0)
                {
                    Console.WriteLine("   ⚠️ Aucune station trouvée dans la réponse SOAP");
                    return stations;
                }

                foreach (XmlNode node in stationNodes)
                {
                    var st = new Station
                    {
                        number = int.Parse(GetNodeValue(node, "a:number", nsmgr) ?? "0"),
                        name = GetNodeValue(node, "a:name", nsmgr),
                        address = GetNodeValue(node, "a:address", nsmgr),
                        available_bikes = int.Parse(GetNodeValue(node, "a:available_bikes", nsmgr) ?? "0"),
                        available_bike_stands = int.Parse(GetNodeValue(node, "a:available_bike_stands", nsmgr) ?? "0"),
                        status = GetNodeValue(node, "a:status", nsmgr),
                        position = new Position
                        {
                            lat = double.Parse(GetNodeValue(node, "a:position/a:lat", nsmgr) ?? "0",
                                              System.Globalization.CultureInfo.InvariantCulture),
                            lng = double.Parse(GetNodeValue(node, "a:position/a:lng", nsmgr) ?? "0",
                                              System.Globalization.CultureInfo.InvariantCulture)
                        }
                    };

                    stations.Add(st);
                }

                return stations;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ❌ Erreur parsing liste SOAP: {ex.Message}");
                return stations;
            }
        }



        private Station ParseStationFromSoapResponse(string soapResponse)
        {
            try
            {
                var doc = new XmlDocument();
                doc.LoadXml(soapResponse);

                var nsmgr = new XmlNamespaceManager(doc.NameTable);
                nsmgr.AddNamespace("s", "http://schemas.xmlsoap.org/soap/envelope/");
                nsmgr.AddNamespace("a", "http://tempuri.org/");

                var stationNode = doc.SelectSingleNode("//a:GetClosestStationAsyncResponse/a:GetClosestStationAsyncResult", nsmgr);

                if (stationNode == null)
                {
                    Console.WriteLine("   ⚠️ Aucune station trouvée dans la réponse SOAP");
                    return null;
                }

                var station = new Station
                {
                    number = int.Parse(GetNodeValue(stationNode, "a:number", nsmgr) ?? "0"),
                    name = GetNodeValue(stationNode, "a:name", nsmgr),
                    address = GetNodeValue(stationNode, "a:address", nsmgr),
                    available_bikes = int.Parse(GetNodeValue(stationNode, "a:available_bikes", nsmgr) ?? "0"),
                    available_bike_stands = int.Parse(GetNodeValue(stationNode, "a:available_bike_stands", nsmgr) ?? "0"),
                    status = GetNodeValue(stationNode, "a:status", nsmgr),
                    position = new Position
                    {
                        lat = double.Parse(GetNodeValue(stationNode, "a:position/a:lat", nsmgr) ?? "0",
                                          System.Globalization.CultureInfo.InvariantCulture),
                        lng = double.Parse(GetNodeValue(stationNode, "a:position/a:lng", nsmgr) ?? "0",
                                          System.Globalization.CultureInfo.InvariantCulture)
                    }
                };

                return station;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ❌ Erreur parsing SOAP: {ex.Message}");
                return null;
            }
        }

        private string GetNodeValue(XmlNode parent, string xpath, XmlNamespaceManager nsmgr)
        {
            var node = parent.SelectSingleNode(xpath, nsmgr);
            return node?.InnerText;
        }

        /// <summary>
        /// ✅ CORRECTION: Détection précise des villes avec JCDecaux
        /// </summary>
        private string DetermineCity(Position position)
        {
            // Nice et Côte d'Azur (7.0 à 7.3 longitude, 43.5 à 43.75 latitude)
            if (position.lat >= 43.5 && position.lat <= 43.75 &&
                position.lng >= 6.9 && position.lng <= 7.35)
            {
                Console.WriteLine($"   📍 Ville détectée: Nice (basé sur GPS)");
                return "Nice";
            }

            // Lyon (4.7 à 4.95 longitude, 45.7 à 45.8 latitude)
            if (position.lat >= 45.7 && position.lat <= 45.85 &&
                position.lng >= 4.7 && position.lng <= 4.95)
            {
                Console.WriteLine($"   📍 Ville détectée: Lyon");
                return "Lyon";
            }

            // Paris et région (2.2 à 2.45 longitude, 48.8 à 49.0 latitude)
            if (position.lat >= 48.8 && position.lat <= 49.0 &&
                position.lng >= 2.2 && position.lng <= 2.45)
            {
                Console.WriteLine($"   📍 Ville détectée: Paris");
                return "Paris";
            }

            // Marseille (5.3 à 5.45 longitude, 43.25 à 43.35 latitude)
            if (position.lat >= 43.25 && position.lat <= 43.35 &&
                position.lng >= 5.3 && position.lng <= 5.45)
            {
                Console.WriteLine($"   📍 Ville détectée: Marseille");
                return "Marseille";
            }

            // Toulouse (1.4 à 1.5 longitude, 43.55 à 43.65 latitude)
            if (position.lat >= 43.55 && position.lat <= 43.65 &&
                position.lng >= 1.4 && position.lng <= 1.5)
            {
                Console.WriteLine($"   📍 Ville détectée: Toulouse");
                return "Toulouse";
            }

            // Par défaut, utiliser Lyon (ville avec beaucoup de stations)
            Console.WriteLine($"   ⚠️ Ville non reconnue (GPS: {position.lat:F4}, {position.lng:F4}), utilisation de Lyon par défaut");
            return "Lyon";
        }
    }
}