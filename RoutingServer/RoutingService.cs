////using System;
////using System.Collections.Generic;
////using System.Linq;
////using System.ServiceModel;
////using System.Threading.Tasks;

////namespace RoutingServer
////{
////    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
////    public class RoutingService : IRoutingService
////    {
////        private readonly OpenRouteService _routeService;
////        private readonly JCDecauxProxy _jcdProxy;

////        public RoutingService()
////        {
////            _routeService = new OpenRouteService();
////            _jcdProxy = new JCDecauxProxy();
////            Console.WriteLine("✅ RoutingService initialisé avec nouvelle logique de temps");
////        }

////        public async Task<ItineraryResponse> GetItinerary(ItineraryRequest request)
////        {
////            if (request == null)
////            {
////                Console.WriteLine("❌ REQUEST EST NULL!");
////                return CreateErrorResponse("❌ Requête invalide (null)");
////            }

////            Console.WriteLine($"\n╔════════════════════════════════════════════╗");
////            Console.WriteLine($"║  🚴 NOUVELLE REQUÊTE D'ITINÉRAIRE         ║");
////            Console.WriteLine($"╚════════════════════════════════════════════╝");
////            Console.WriteLine($"   📍 Origine: '{request.Origin ?? "NULL"}'");
////            Console.WriteLine($"   🎯 Destination: '{request.Destination ?? "NULL"}'");
////            Console.WriteLine($"   🚲 Vélos min: {request.MinBikes}");

////            if (string.IsNullOrWhiteSpace(request.Origin) || string.IsNullOrWhiteSpace(request.Destination))
////            {
////                return CreateErrorResponse("❌ Origine ou destination manquante");
////            }

////            try
////            {
////                // 1️⃣ Géocoder origine et destination
////                Console.WriteLine("\n📍 ÉTAPE 1: Géocodage des adresses");
////                var originPos = await _routeService.GeocodeAddress(request.Origin);
////                var destPos = await _routeService.GeocodeAddress(request.Destination);

////                if (originPos == null || destPos == null)
////                {
////                    return CreateErrorResponse("❌ Impossible de localiser l'origine ou la destination");
////                }

////                // 2️⃣ Calculer distance directe
////                var directDistance = _routeService.CalculateDistanceMeters(originPos, destPos);
////                Console.WriteLine($"\n📏 ÉTAPE 2: Distance directe = {(directDistance / 1000):F2} km");

////                // 3️⃣ Vérifier distance minimale absolue (200m)
////                if (directDistance < 200)
////                {
////                    Console.WriteLine($"   ⚠️ Distance trop courte ({directDistance:F0}m < 200m) → MARCHE UNIQUEMENT");
////                    return await CreateWalkingOnlyItinerary(originPos, destPos, "distance trop courte");
////                }

////                // 4️⃣ Chercher des stations vélo
////                Console.WriteLine($"\n🚲 ÉTAPE 3: Recherche de stations JCDecaux");
////                var originStation = await _jcdProxy.GetClosestStation(originPos, request.MinBikes > 0 ? request.MinBikes : 1);
////                var destStation = await _jcdProxy.GetClosestStation(destPos, 1);

////                if (originStation == null || destStation == null)
////                {
////                    Console.WriteLine("   ⚠️ Aucune station disponible → MARCHE UNIQUEMENT");
////                    return await CreateWalkingOnlyItinerary(originPos, destPos, "pas de stations disponibles");
////                }

////                Console.WriteLine($"   ✅ Station départ: {originStation.name} ({originStation.available_bikes} vélos)");
////                Console.WriteLine($"   ✅ Station arrivée: {destStation.name} ({destStation.available_bike_stands} places)");

////                // 5️⃣ NOUVELLE LOGIQUE : Comparer le TEMPS total
////                Console.WriteLine($"\n⏱️ ÉTAPE 4: Comparaison des temps de trajet");

////                // Calculer temps MARCHE SEULE
////                Console.WriteLine("   🚶 Calcul temps marche seule...");
////                var walkOnlyRoute = await _routeService.GetWalkingRoute(originPos, destPos);
////                var walkOnlyTime = walkOnlyRoute.Duration; // secondes

////                // Calculer temps AVEC VÉLO
////                Console.WriteLine("   🚴 Calcul temps avec vélo...");
////                var walkToStationRoute = await _routeService.GetWalkingRoute(originPos, originStation.position);
////                var bikeRoute = await _routeService.GetCyclingRoute(originStation.position, destStation.position);
////                var walkFromStationRoute = await _routeService.GetWalkingRoute(destStation.position, destPos);

////                var bikeTime = walkToStationRoute.Duration + 30 + bikeRoute.Duration + 30 + walkFromStationRoute.Duration;
////                // 30 secondes pour prendre/déposer le vélo

////                var timeSaved = walkOnlyTime - bikeTime;

////                Console.WriteLine($"\n   📊 RÉSULTATS:");
////                Console.WriteLine($"      Marche seule: {(walkOnlyTime / 60):F1} min");
////                Console.WriteLine($"      Avec vélo:    {(bikeTime / 60):F1} min");
////                Console.WriteLine($"      Gain:         {(timeSaved / 60):F1} min");

////                // 6️⃣ Décision : utiliser le vélo SI gain de temps >= 2 minutes
////                const int MIN_TIME_SAVING = 120; // 2 minutes en secondes

////                if (timeSaved >= MIN_TIME_SAVING)
////                {
////                    Console.WriteLine($"\n   ✅ DÉCISION: VÉLO (gain de {(timeSaved / 60):F1} min >= 2 min)");
////                    return await CreateBikeItinerary(originPos, destPos, originStation, destStation,
////                                                     walkToStationRoute, bikeRoute, walkFromStationRoute);
////                }
////                else
////                {
////                    Console.WriteLine($"\n   ⚠️ DÉCISION: MARCHE SEULE (gain de {(timeSaved / 60):F1} min < 2 min)");
////                    return await CreateWalkingOnlyItinerary(originPos, destPos, $"gain de temps insuffisant ({(timeSaved / 60):F1} min)");
////                }
////            }
////            catch (Exception ex)
////            {
////                Console.WriteLine($"\n❌ ERREUR FATALE: {ex.Message}");
////                Console.WriteLine($"   Stack: {ex.StackTrace}");
////                return CreateErrorResponse($"❌ Erreur lors du calcul: {ex.Message}");
////            }
////        }

////        private async Task<ItineraryResponse> CreateWalkingOnlyItinerary(Position origin, Position dest, string reason)
////        {
////            var route = await _routeService.GetWalkingRoute(origin, dest);

////            Console.WriteLine($"\n✅ ITINÉRAIRE MARCHE SEULE:");
////            Console.WriteLine($"   Raison: {reason}");
////            Console.WriteLine($"   Distance: {(route.Distance / 1000):F2} km");
////            Console.WriteLine($"   Durée: {(route.Duration / 60):F1} min");
////            Console.WriteLine($"   Waypoints: {route.Waypoints?.Count ?? 0}");

////            return new ItineraryResponse
////            {
////                Instructions = $"✅ Itinéraire à pied uniquement ({reason})",
////                TotalDistance = route.Distance,
////                TotalDuration = route.Duration,
////                Steps = new List<Step>
////                {
////                    new Step
////                    {
////                        Instruction = $"Marcher jusqu'à la destination ({(route.Distance/1000):F2} km)",
////                        Distance = route.Distance,
////                        Duration = route.Duration,
////                        Type = "walk",
////                        Waypoints = route.Waypoints ?? new List<Position> { origin, dest }
////                    }
////                }
////            };
////        }

////        private async Task<ItineraryResponse> CreateBikeItinerary(
////            Position origin, Position dest,
////            Station originStation, Station destStation,
////            RouteSegment walkToStation, RouteSegment bikeRoute, RouteSegment walkFromStation)
////        {
////            var steps = new List<Step>();
////            double totalDistance = 0;
////            double totalDuration = 0;

////            Console.WriteLine($"\n✅ ITINÉRAIRE AVEC VÉLO:");

////            // Étape 1: Marcher jusqu'à la station de départ
////            steps.Add(new Step
////            {
////                Instruction = $"🚶 Marcher jusqu'à la station '{originStation.name}' ({(walkToStation.Distance / 1000):F2} km)",
////                Distance = walkToStation.Distance,
////                Duration = walkToStation.Duration,
////                Type = "walk",
////                Waypoints = walkToStation.Waypoints ?? new List<Position> { origin, originStation.position }
////            });
////            totalDistance += walkToStation.Distance;
////            totalDuration += walkToStation.Duration;
////            Console.WriteLine($"   1. Marche vers station: {(walkToStation.Distance / 1000):F2} km, {(walkToStation.Duration / 60):F1} min, {walkToStation.Waypoints?.Count ?? 0} pts");

////            // Étape 2: Prendre un vélo
////            steps.Add(new Step
////            {
////                Instruction = $"🚲 Prendre un vélo à '{originStation.name}' ({originStation.available_bikes} disponibles)",
////                Distance = 0,
////                Duration = 30,
////                Type = "bike",
////                Waypoints = new List<Position> { originStation.position }
////            });
////            totalDuration += 30;
////            Console.WriteLine($"   2. Prendre vélo: 30s");

////            // Étape 3: Faire du vélo
////            steps.Add(new Step
////            {
////                Instruction = $"🚴 Rouler jusqu'à la station '{destStation.name}' ({(bikeRoute.Distance / 1000):F2} km)",
////                Distance = bikeRoute.Distance,
////                Duration = bikeRoute.Duration,
////                Type = "bike",
////                Waypoints = bikeRoute.Waypoints ?? new List<Position> { originStation.position, destStation.position }
////            });
////            totalDistance += bikeRoute.Distance;
////            totalDuration += bikeRoute.Duration;
////            Console.WriteLine($"   3. Vélo: {(bikeRoute.Distance / 1000):F2} km, {(bikeRoute.Duration / 60):F1} min, {bikeRoute.Waypoints?.Count ?? 0} pts");

////            // Étape 4: Déposer le vélo
////            steps.Add(new Step
////            {
////                Instruction = $"🅿️ Déposer le vélo à '{destStation.name}' ({destStation.available_bike_stands} places libres)",
////                Distance = 0,
////                Duration = 30,
////                Type = "bike",
////                Waypoints = new List<Position> { destStation.position }
////            });
////            totalDuration += 30;
////            Console.WriteLine($"   4. Déposer vélo: 30s");

////            // Étape 5: Marcher jusqu'à la destination
////            steps.Add(new Step
////            {
////                Instruction = $"🚶 Marcher jusqu'à la destination ({(walkFromStation.Distance / 1000):F2} km)",
////                Distance = walkFromStation.Distance,
////                Duration = walkFromStation.Duration,
////                Type = "walk",
////                Waypoints = walkFromStation.Waypoints ?? new List<Position> { destStation.position, dest }
////            });
////            totalDistance += walkFromStation.Distance;
////            totalDuration += walkFromStation.Duration;
////            Console.WriteLine($"   5. Marche finale: {(walkFromStation.Distance / 1000):F2} km, {(walkFromStation.Duration / 60):F1} min, {walkFromStation.Waypoints?.Count ?? 0} pts");

////            Console.WriteLine($"\n   📊 TOTAL: {(totalDistance / 1000):F2} km, {(totalDuration / 60):F1} min");

////            return new ItineraryResponse
////            {
////                Instructions = $"✅ Itinéraire avec vélo calculé ! Distance: {(totalDistance / 1000):F2} km - Durée: {(totalDuration / 60):F0} min",
////                TotalDistance = totalDistance,
////                TotalDuration = totalDuration,
////                Steps = steps
////            };
////        }

////        private ItineraryResponse CreateErrorResponse(string message)
////        {
////            return new ItineraryResponse
////            {
////                Instructions = message,
////                TotalDistance = 0,
////                TotalDuration = 0,
////                Steps = new List<Step>()
////            };
////        }
////    }
////}




//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.ServiceModel;
//using System.Threading.Tasks;

//namespace RoutingServer
//{
//    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
//    public class RoutingService : IRoutingService
//    {
//        private readonly OpenRouteService _routeService;
//        private readonly JCDecauxProxy _jcdProxy;

//        public RoutingService()
//        {
//            _routeService = new OpenRouteService();
//            _jcdProxy = new JCDecauxProxy();
//            Console.WriteLine("✅ RoutingService initialisé avec nouvelle logique de temps");
//        }

//        public async Task<ItineraryResponse> GetItinerary(ItineraryRequest request)
//        {
//            if (request == null)
//            {
//                Console.WriteLine("❌ REQUEST EST NULL!");
//                return CreateErrorResponse("❌ Requête invalide (null)");
//            }

//            Console.WriteLine($"\n╔════════════════════════════════════════════╗");
//            Console.WriteLine($"║  🚴 NOUVELLE REQUÊTE D'ITINÉRAIRE         ║");
//            Console.WriteLine($"╚════════════════════════════════════════════╝");
//            Console.WriteLine($"   📍 Origine: '{request.Origin ?? "NULL"}'");
//            Console.WriteLine($"   🎯 Destination: '{request.Destination ?? "NULL"}'");
//            Console.WriteLine($"   🚲 Vélos min: {request.MinBikes}");

//            if (string.IsNullOrWhiteSpace(request.Origin) || string.IsNullOrWhiteSpace(request.Destination))
//            {
//                return CreateErrorResponse("❌ Origine ou destination manquante");
//            }

//            try
//            {
//                // 1️⃣ Géocoder origine et destination
//                Console.WriteLine("\n📍 ÉTAPE 1: Géocodage des adresses");
//                var originPos = await _routeService.GeocodeAddress(request.Origin);
//                var destPos = await _routeService.GeocodeAddress(request.Destination);

//                if (originPos == null || destPos == null)
//                {
//                    return CreateErrorResponse("❌ Impossible de localiser l'origine ou la destination");
//                }

//                // 2️⃣ Calculer distance directe
//                var directDistance = _routeService.CalculateDistanceMeters(originPos, destPos);
//                Console.WriteLine($"\n📏 ÉTAPE 2: Distance directe = {(directDistance / 1000):F2} km");

//                // 3️⃣ Vérifier distance minimale absolue (200m)
//                if (directDistance < 200)
//                {
//                    Console.WriteLine($"   ⚠️ Distance trop courte ({directDistance:F0}m < 200m) → MARCHE UNIQUEMENT");
//                    return await CreateWalkingOnlyItinerary(originPos, destPos, "distance trop courte");
//                }

//                // 4️⃣ Chercher des stations vélo
//                Console.WriteLine($"\n🚲 ÉTAPE 3: Recherche de stations JCDecaux");
//                var originStation = await _jcdProxy.GetClosestStation(originPos, request.MinBikes > 0 ? request.MinBikes : 1);
//                var destStation = await _jcdProxy.GetClosestStation(destPos, 1);

//                if (originStation == null || destStation == null)
//                {
//                    Console.WriteLine("   ⚠️ Aucune station disponible → MARCHE UNIQUEMENT");
//                    return await CreateWalkingOnlyItinerary(originPos, destPos, "pas de stations disponibles");
//                }

//                Console.WriteLine($"   ✅ Station départ: {originStation.name} ({originStation.available_bikes} vélos)");
//                Console.WriteLine($"   ✅ Station arrivée: {destStation.name} ({destStation.available_bike_stands} places)");

//                // 5️⃣ NOUVELLE LOGIQUE : Comparer le TEMPS total
//                Console.WriteLine($"\n⏱️ ÉTAPE 4: Comparaison des temps de trajet");

//                // Calculer temps MARCHE SEULE
//                Console.WriteLine("   🚶 Calcul temps marche seule...");
//                var walkOnlyRoute = await _routeService.GetWalkingRoute(originPos, destPos);
//                var walkOnlyTime = walkOnlyRoute.Duration; // secondes

//                // Calculer temps AVEC VÉLO
//                Console.WriteLine("   🚴 Calcul temps avec vélo...");
//                var walkToStationRoute = await _routeService.GetWalkingRoute(originPos, originStation.position);
//                var bikeRoute = await _routeService.GetCyclingRoute(originStation.position, destStation.position);
//                var walkFromStationRoute = await _routeService.GetWalkingRoute(destStation.position, destPos);

//                var bikeTime = walkToStationRoute.Duration + 30 + bikeRoute.Duration + 30 + walkFromStationRoute.Duration;
//                // 30 secondes pour prendre/déposer le vélo

//                var timeSaved = walkOnlyTime - bikeTime;

//                Console.WriteLine($"\n   📊 RÉSULTATS:");
//                Console.WriteLine($"      Marche seule: {(walkOnlyTime / 60):F1} min");
//                Console.WriteLine($"      Avec vélo:    {(bikeTime / 60):F1} min");
//                Console.WriteLine($"      Gain:         {(timeSaved / 60):F1} min");

//                // 6️⃣ Décision : utiliser le vélo SI gain de temps >= 2 minutes
//                const int MIN_TIME_SAVING = 120; // 2 minutes en secondes

//                if (timeSaved >= MIN_TIME_SAVING)
//                {
//                    Console.WriteLine($"\n   ✅ DÉCISION: VÉLO (gain de {(timeSaved / 60):F1} min >= 2 min)");
//                    return await CreateBikeItinerary(originPos, destPos, originStation, destStation,
//                                                     walkToStationRoute, bikeRoute, walkFromStationRoute);
//                }
//                else
//                {
//                    Console.WriteLine($"\n   ⚠️ DÉCISION: MARCHE SEULE (gain de {(timeSaved / 60):F1} min < 2 min)");
//                    return await CreateWalkingOnlyItinerary(originPos, destPos, $"gain de temps insuffisant ({(timeSaved / 60):F1} min)");
//                }
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"\n❌ ERREUR FATALE: {ex.Message}");
//                Console.WriteLine($"   Stack: {ex.StackTrace}");
//                return CreateErrorResponse($"❌ Erreur lors du calcul: {ex.Message}");
//            }
//        }

//        private async Task<ItineraryResponse> CreateWalkingOnlyItinerary(Position origin, Position dest, string reason)
//        {
//            var route = await _routeService.GetWalkingRoute(origin, dest);

//            Console.WriteLine($"\n✅ ITINÉRAIRE MARCHE SEULE:");
//            Console.WriteLine($"   Raison: {reason}");
//            Console.WriteLine($"   Distance: {(route.Distance / 1000):F2} km");
//            Console.WriteLine($"   Durée: {(route.Duration / 60):F1} min");
//            Console.WriteLine($"   Waypoints: {route.Waypoints?.Count ?? 0}");

//            return new ItineraryResponse
//            {
//                Instructions = $"✅ Itinéraire à pied uniquement ({reason})",
//                TotalDistance = route.Distance,
//                TotalDuration = route.Duration,
//                Steps = new List<Step>
//                {
//                    new Step
//                    {
//                        Instruction = $"Marcher jusqu'à la destination ({(route.Distance/1000):F2} km)",
//                        Distance = route.Distance,
//                        Duration = route.Duration,
//                        Type = "walk",
//                        Waypoints = route.Waypoints ?? new List<Position> { origin, dest }
//                    }
//                }
//            };
//        }

//        private async Task<ItineraryResponse> CreateBikeItinerary(
//            Position origin, Position dest,
//            Station originStation, Station destStation,
//            RouteSegment walkToStation, RouteSegment bikeRoute, RouteSegment walkFromStation)
//        {
//            var steps = new List<Step>();
//            double totalDistance = 0;
//            double totalDuration = 0;

//            Console.WriteLine($"\n✅ ITINÉRAIRE AVEC VÉLO:");

//            // Étape 1: Marcher jusqu'à la station de départ
//            steps.Add(new Step
//            {
//                Instruction = $"🚶 Marcher jusqu'à la station '{originStation.name}' ({(walkToStation.Distance / 1000):F2} km)",
//                Distance = walkToStation.Distance,
//                Duration = walkToStation.Duration,
//                Type = "walk",
//                Waypoints = walkToStation.Waypoints ?? new List<Position> { origin, originStation.position }
//            });
//            totalDistance += walkToStation.Distance;
//            totalDuration += walkToStation.Duration;
//            Console.WriteLine($"   1. Marche vers station: {(walkToStation.Distance / 1000):F2} km, {(walkToStation.Duration / 60):F1} min, {walkToStation.Waypoints?.Count ?? 0} pts");

//            // Étape 2: Prendre un vélo
//            steps.Add(new Step
//            {
//                Instruction = $"🚲 Prendre un vélo à '{originStation.name}' ({originStation.available_bikes} disponibles)",
//                Distance = 0,
//                Duration = 30,
//                Type = "bike",
//                Waypoints = new List<Position> { originStation.position }
//            });
//            totalDuration += 30;
//            Console.WriteLine($"   2. Prendre vélo: 30s");

//            // Étape 3: Faire du vélo
//            steps.Add(new Step
//            {
//                Instruction = $"🚴 Rouler jusqu'à la station '{destStation.name}' ({(bikeRoute.Distance / 1000):F2} km)",
//                Distance = bikeRoute.Distance,
//                Duration = bikeRoute.Duration,
//                Type = "bike",
//                Waypoints = bikeRoute.Waypoints ?? new List<Position> { originStation.position, destStation.position }
//            });
//            totalDistance += bikeRoute.Distance;
//            totalDuration += bikeRoute.Duration;
//            Console.WriteLine($"   3. Vélo: {(bikeRoute.Distance / 1000):F2} km, {(bikeRoute.Duration / 60):F1} min, {bikeRoute.Waypoints?.Count ?? 0} pts");

//            // Étape 4: Déposer le vélo
//            steps.Add(new Step
//            {
//                Instruction = $"🅿️ Déposer le vélo à '{destStation.name}' ({destStation.available_bike_stands} places libres)",
//                Distance = 0,
//                Duration = 30,
//                Type = "bike",
//                Waypoints = new List<Position> { destStation.position }
//            });
//            totalDuration += 30;
//            Console.WriteLine($"   4. Déposer vélo: 30s");

//            // Étape 5: Marcher jusqu'à la destination
//            steps.Add(new Step
//            {
//                Instruction = $"🚶 Marcher jusqu'à la destination ({(walkFromStation.Distance / 1000):F2} km)",
//                Distance = walkFromStation.Distance,
//                Duration = walkFromStation.Duration,
//                Type = "walk",
//                Waypoints = walkFromStation.Waypoints ?? new List<Position> { destStation.position, dest }
//            });
//            totalDistance += walkFromStation.Distance;
//            totalDuration += walkFromStation.Duration;
//            Console.WriteLine($"   5. Marche finale: {(walkFromStation.Distance / 1000):F2} km, {(walkFromStation.Duration / 60):F1} min, {walkFromStation.Waypoints?.Count ?? 0} pts");

//            Console.WriteLine($"\n   📊 TOTAL: {(totalDistance / 1000):F2} km, {(totalDuration / 60):F1} min");

//            return new ItineraryResponse
//            {
//                Instructions = $"✅ Itinéraire avec vélo calculé ! Distance: {(totalDistance / 1000):F2} km - Durée: {(totalDuration / 60):F0} min",
//                TotalDistance = totalDistance,
//                TotalDuration = totalDuration,
//                Steps = steps
//            };
//        }

//        private ItineraryResponse CreateErrorResponse(string message)
//        {
//            return new ItineraryResponse
//            {
//                Instructions = message,
//                TotalDistance = 0,
//                TotalDuration = 0,
//                Steps = new List<Step>()
//            };
//        }
//    }
//}



////using System;
////using System.Collections.Generic;
////using System.Linq;
////using System.ServiceModel;
////using System.Threading.Tasks;
////using Newtonsoft.Json;

////namespace RoutingServer
////{
////    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
////    public class RoutingService : IRoutingService
////    {
////        private readonly OpenRouteService _routeService;
////        private readonly JCDecauxProxy _jcdProxy;

////        public RoutingService()
////        {
////            _routeService = new OpenRouteService();
////            _jcdProxy = new JCDecauxProxy();
////            Console.WriteLine("✅ RoutingService initialisé avec logique multi-contrats");
////        }

////        public async Task<ItineraryResponse> GetItinerary(ItineraryRequest request)
////        {
////            if (request == null)
////            {
////                Console.WriteLine("❌ REQUEST EST NULL!");
////                return CreateErrorResponse("❌ Requête invalide (null)");
////            }

////            Console.WriteLine($"\n╔════════════════════════════════════════════╗");
////            Console.WriteLine($"║  🚴 NOUVELLE REQUÊTE D'ITINÉRAIRE         ║");
////            Console.WriteLine($"╚════════════════════════════════════════════╝");
////            Console.WriteLine($"   📍 Origine: '{request.Origin ?? "NULL"}'");
////            Console.WriteLine($"   🎯 Destination: '{request.Destination ?? "NULL"}'");
////            Console.WriteLine($"   🚲 Vélos min: {request.MinBikes}");

////            if (string.IsNullOrWhiteSpace(request.Origin) || string.IsNullOrWhiteSpace(request.Destination))
////            {
////                return CreateErrorResponse("❌ Origine ou destination manquante");
////            }

////            try
////            {
////                // 1️⃣ Géocoder origine et destination
////                Console.WriteLine("\n📍 ÉTAPE 1: Géocodage des adresses");
////                var originPos = await _routeService.GeocodeAddress(request.Origin);
////                var destPos = await _routeService.GeocodeAddress(request.Destination);

////                if (originPos == null || destPos == null)
////                {
////                    return CreateErrorResponse("❌ Impossible de localiser l'origine ou la destination");
////                }

////                // 2️⃣ Calculer distance directe
////                var directDistance = _routeService.CalculateDistanceMeters(originPos, destPos);
////                Console.WriteLine($"\n📏 ÉTAPE 2: Distance directe = {(directDistance / 1000):F2} km");

////                // 3️⃣ Vérifier distance minimale absolue (200m)
////                if (directDistance < 200)
////                {
////                    Console.WriteLine($"   ⚠️ Distance trop courte ({directDistance:F0}m < 200m) → MARCHE UNIQUEMENT");
////                    return await CreateWalkingOnlyItinerary(originPos, destPos, "distance trop courte");
////                }

////                // 4️⃣ Détecter contrats (villes)
////                var originCity = DetermineCityFromPosition(originPos);
////                var destCity = DetermineCityFromPosition(destPos);

////                Console.WriteLine($"\n🏙️ DÉTECTION DES CONTRATS:");
////                Console.WriteLine($"   Origine: {originCity}");
////                Console.WriteLine($"   Destination: {destCity}");

////                // 5️⃣ Cas: même contrat vs différents
////                if (string.Equals(originCity, destCity, StringComparison.OrdinalIgnoreCase) || originCity == "Unknown" || destCity == "Unknown")
////                {
////                    Console.WriteLine("   ✅ Même contrat (ou inconnue) → Itinéraire simple");
////                    return await CreateSameContractItinerary(originPos, destPos, originCity, request.MinBikes);
////                }
////                else
////                {
////                    Console.WriteLine("   ⚠️ Contrats différents → Itinéraire multi-modal");
////                    return await CreateMultiContractItinerary(originPos, destPos, originCity, destCity, request.MinBikes);
////                }
////            }
////            catch (Exception ex)
////            {
////                Console.WriteLine($"\n❌ ERREUR FATALE: {ex.Message}");
////                Console.WriteLine($"   Stack: {ex.StackTrace}");
////                return CreateErrorResponse($"❌ Erreur lors du calcul: {ex.Message}");
////            }
////        }

////        #region City detection
////        private string DetermineCityFromPosition(Position position)
////        {
////            if (position == null)
////                return "Unknown";

////            // Nice
////            if (position.lat >= 43.5 && position.lat <= 43.75 && position.lng >= 6.9 && position.lng <= 7.35)
////                return "Nice";

////            // Lyon
////            if (position.lat >= 45.7 && position.lat <= 45.85 && position.lng >= 4.7 && position.lng <= 4.95)
////                return "Lyon";

////            // Paris
////            if (position.lat >= 48.8 && position.lat <= 49.0 && position.lng >= 2.2 && position.lng <= 2.45)
////                return "Paris";

////            // Marseille
////            if (position.lat >= 43.25 && position.lat <= 43.35 && position.lng >= 5.3 && position.lng <= 5.45)
////                return "Marseille";

////            // Toulouse
////            if (position.lat >= 43.55 && position.lat <= 43.65 && position.lng >= 1.4 && position.lng <= 1.5)
////                return "Toulouse";

////            return "Unknown";
////        }
////        #endregion

////        #region Same-contract itinerary
////        private async Task<ItineraryResponse> CreateSameContractItinerary(Position origin, Position dest, string city, int minBikes)
////        {
////            Console.WriteLine($"\n🌐 ITINÉRAIRE DANS LE MÊME CONTRAT: {city}");

////            // Chercher stations (proxy gère detection de la ville si nécessaire)
////            var originStation = await _jcdProxy.GetClosestStation(origin, minBikes > 0 ? minBikes : 1);
////            var destStation = await _jcdProxy.GetClosestStation(dest, 1);

////            if (originStation == null || destStation == null)
////            {
////                Console.WriteLine("   ⚠️ Aucune station trouvée dans ce contrat → MARCHE UNIQUEMENT");
////                return await CreateWalkingOnlyItinerary(origin, dest, "pas de stations disponibles dans le contrat");
////            }

////            // Calculer temps marche vs vélo
////            Console.WriteLine("\n⏱️ Comparaison des temps: marche seule vs vélo (même contrat)");

////            var walkOnlyRoute = await _routeService.GetWalkingRoute(origin, dest);
////            var walkOnlyTime = walkOnlyRoute.Duration;

////            var walkToStationRoute = await _routeService.GetWalkingRoute(origin, originStation.position);
////            var bikeRoute = await _routeService.GetCyclingRoute(originStation.position, destStation.position);
////            var walkFromStationRoute = await _routeService.GetWalkingRoute(destStation.position, dest);

////            var bikeTime = walkToStationRoute.Duration + 30 + bikeRoute.Duration + 30 + walkFromStationRoute.Duration;
////            var timeSaved = Math.Max(0, walkOnlyTime - bikeTime);//gain

////            Console.WriteLine($"   Marche seule: {(walkOnlyTime / 60):F1} min | Avec vélo: {(bikeTime / 60):F1} min | Gain: {(timeSaved / 60):F1} min");

////            const int MIN_TIME_SAVING = 120; // 2 minutes

////            if (timeSaved >= MIN_TIME_SAVING)
////            {
////                Console.WriteLine("   ✅ Décision: utiliser vélo");
////                return await CreateBikeItinerary(origin, dest, originStation, destStation, walkToStationRoute, bikeRoute, walkFromStationRoute);
////            }
////            else
////            {
////                Console.WriteLine("   ⚠️ Décision: marche seule (gain insuffisant)");
////                return await CreateWalkingOnlyItinerary(origin, dest,
////                    timeSaved == 0 ? "trajet vélo plus lent que la marche" :
////                                     $"gain de temps insuffisant ({(timeSaved / 60):F1} min)");
////            }
////        }
////        #endregion

////        #region Multi-contract itinerary
////        private async Task<ItineraryResponse> CreateMultiContractItinerary(
////            Position origin, Position dest,
////            string originCity, string destCity, int minBikes)
////        {
////            Console.WriteLine($"\n🌍 ITINÉRAIRE MULTI-CONTRATS: {originCity} → {destCity}");

////            var steps = new List<Step>();
////            double totalDistance = 0;
////            double totalDuration = 0;

////            // PARTIE 1: dans la ville d'origine
////            Console.WriteLine($"\n📍 PARTIE 1: Déplacement dans {originCity}");

////            var originStation = await FindBestStationTowardsDestination(origin, dest, originCity, minBikes);

////            if (originStation != null)
////            {
////                var walkToStation = await _routeService.GetWalkingRoute(origin, originStation.position);
////                steps.Add(new Step
////                {
////                    Instruction = $"🚶 Marcher jusqu'à '{originStation.name}' ({(walkToStation.Distance / 1000):F2} km)",
////                    Distance = walkToStation.Distance,
////                    Duration = walkToStation.Duration,
////                    Type = "walk",
////                    Waypoints = walkToStation.Waypoints
////                });
////                totalDistance += walkToStation.Distance;
////                totalDuration += walkToStation.Duration;

////                steps.Add(new Step
////                {
////                    Instruction = $"🚲 Prendre un vélo à '{originStation.name}' ({originStation.available_bikes} disponibles)",
////                    Distance = 0,
////                    Duration = 30,
////                    Type = "bike",
////                    Waypoints = new List<Position> { originStation.position }
////                });
////                totalDuration += 30;

////                var exitPoint = await FindCityExitPoint(originStation.position, dest, originCity);
////                var bikeRoute = await _routeService.GetCyclingRoute(originStation.position, exitPoint);
////                steps.Add(new Step
////                {
////                    Instruction = $"🚴 Rouler vers la sortie de {originCity} ({(bikeRoute.Distance / 1000):F2} km)",
////                    Distance = bikeRoute.Distance,
////                    Duration = bikeRoute.Duration,
////                    Type = "bike",
////                    Waypoints = bikeRoute.Waypoints
////                });
////                totalDistance += bikeRoute.Distance;
////                totalDuration += bikeRoute.Duration;

////                // Déposer vélo près de la sortie
////                var exitStation = await _jcdProxy.GetClosestStation(exitPoint, 0);
////                if (exitStation != null)
////                {
////                    steps.Add(new Step
////                    {
////                        Instruction = $"🅿️ Déposer le vélo à '{exitStation.name}' ({exitStation.available_bike_stands} places)",
////                        Distance = 0,
////                        Duration = 30,
////                        Type = "bike",
////                        Waypoints = new List<Position> { exitStation.position }
////                    });
////                    totalDuration += 30;
////                }
////            }
////            else
////            {
////                Console.WriteLine("   ⚠️ Aucune station utile trouvée dans la ville d'origine → on marchera jusqu'au point de transition");
////            }

////            // PARTIE 2: transition (train/bus)
////            Console.WriteLine($"\n🚄 PARTIE 2: Transition {originCity} → {destCity}");
////            var transitionStart = originStation?.position ?? origin;
////            var transitionEnd = dest; // simplification: arriver directement au dest city center

////            var transitionDistance = _routeService.CalculateDistanceMeters(transitionStart, transitionEnd);
////            var transitionDuration = transitionDistance / 20000.0; // approx in seconds assuming 72 km/h -> 20000 m/s? (adjusted below)

////            // Better: assume train speed ~ 72 km/h = 20 m/s => seconds = meters / 20
////            transitionDuration = transitionDistance / 20.0;

////            steps.Add(new Step
////            {
////                Instruction = $"🚄 Transition entre villes (train/bus recommandé, {(transitionDistance / 1000):F2} km)",
////                Distance = transitionDistance,
////                Duration = transitionDuration,
////                Type = "transit",
////                Waypoints = new List<Position> { transitionStart, transitionEnd }
////            });
////            totalDistance += transitionDistance;
////            totalDuration += transitionDuration;

////            // PARTIE 3: dans la ville de destination
////            Console.WriteLine($"\n📍 PARTIE 3: Déplacement dans {destCity}");

////            var destStation = await _jcdProxy.GetClosestStation(dest, minBikes > 0 ? minBikes : 1);

////            if (destStation != null)
////            {
////                var walkToDestStation = await _routeService.GetWalkingRoute(transitionEnd, destStation.position);
////                steps.Add(new Step
////                {
////                    Instruction = $"🚶 Marcher jusqu'à '{destStation.name}' ({(walkToDestStation.Distance / 1000):F2} km)",
////                    Distance = walkToDestStation.Distance,
////                    Duration = walkToDestStation.Duration,
////                    Type = "walk",
////                    Waypoints = walkToDestStation.Waypoints
////                });
////                totalDistance += walkToDestStation.Distance;
////                totalDuration += walkToDestStation.Duration;

////                steps.Add(new Step
////                {
////                    Instruction = $"🚲 Prendre un vélo à '{destStation.name}'",
////                    Distance = 0,
////                    Duration = 30,
////                    Type = "bike",
////                    Waypoints = new List<Position> { destStation.position }
////                });
////                totalDuration += 30;

////                var finalBikeRoute = await _routeService.GetCyclingRoute(destStation.position, dest);
////                steps.Add(new Step
////                {
////                    Instruction = $"🚴 Rouler jusqu'à la destination ({(finalBikeRoute.Distance / 1000):F2} km)",
////                    Distance = finalBikeRoute.Distance,
////                    Duration = finalBikeRoute.Duration,
////                    Type = "bike",
////                    Waypoints = finalBikeRoute.Waypoints
////                });
////                totalDistance += finalBikeRoute.Distance;
////                totalDuration += finalBikeRoute.Duration;

////                var finalStation = await _jcdProxy.GetClosestStation(dest, 0);
////                if (finalStation != null)
////                {
////                    steps.Add(new Step
////                    {
////                        Instruction = $"🅿️ Déposer le vélo à '{finalStation.name}'",
////                        Distance = 0,
////                        Duration = 30,
////                        Type = "bike",
////                        Waypoints = new List<Position> { finalStation.position }
////                    });
////                    totalDuration += 30;
////                }

////                var finalWalk = await _routeService.GetWalkingRoute(finalStation?.position ?? destStation.position, dest);
////                steps.Add(new Step
////                {
////                    Instruction = $"🚶 Marcher jusqu'à la destination ({(finalWalk.Distance / 1000):F2} km)",
////                    Distance = finalWalk.Distance,
////                    Duration = finalWalk.Duration,
////                    Type = "walk",
////                    Waypoints = finalWalk.Waypoints
////                });
////                totalDistance += finalWalk.Distance;
////                totalDuration += finalWalk.Duration;
////            }
////            else
////            {
////                Console.WriteLine("   ⚠️ Aucune station disponible dans la ville de destination → marche/transit final");
////                var finalWalk = await _routeService.GetWalkingRoute(transitionEnd, dest);
////                steps.Add(new Step
////                {
////                    Instruction = $"🚶 Marcher jusqu'à la destination ({(finalWalk.Distance / 1000):F2} km)",
////                    Distance = finalWalk.Distance,
////                    Duration = finalWalk.Duration,
////                    Type = "walk",
////                    Waypoints = finalWalk.Waypoints
////                });
////                totalDistance += finalWalk.Distance;
////                totalDuration += finalWalk.Duration;
////            }

////            Console.WriteLine($"\n   📊 TOTAL MULTI-CONTRATS: {(totalDistance / 1000):F2} km, {(totalDuration / 60):F1} min");

////            return new ItineraryResponse
////            {
////                Instructions = $"✅ Itinéraire multi-villes calculé ! {originCity} → {destCity}",
////                TotalDistance = totalDistance,
////                TotalDuration = totalDuration,
////                Steps = steps
////            };
////        }
////        #endregion

////        #region Helpers for multi-contract
////        private async Task<Station> FindBestStationTowardsDestination(Position origin, Position finalDest, string city, int minBikes)
////        {
////            Console.WriteLine($"   🔎 Recherche stations dans {city} pour s'approcher de la destination...");
////            try
////            {
////                var allStations = await _jcdProxy.GetStationsAsync(city);
////                if (allStations == null || allStations.Count == 0)
////                    return null;


////                var availableStations = allStations
////                    .Where(s => s.available_bikes >= (minBikes > 0 ? minBikes : 1) && string.Equals(s.status, "OPEN", StringComparison.OrdinalIgnoreCase))
////                    .ToList();

////                if (!availableStations.Any())
////                    return null;

////                var candidates = availableStations
////                    .Select(station => new
////                    {
////                        Station = station,
////                        WalkDistance = _routeService.CalculateDistanceMeters(origin, station.position),
////                        ProgressTowardsDest = CalculateProgress(origin, station.position, finalDest)
////                    })
////                    .Where(x => x.WalkDistance < 1000) // max 1km de marche
////                    .OrderByDescending(x => x.ProgressTowardsDest)
////                    .ThenBy(x => x.WalkDistance)
////                    .ToList();

////                return candidates.FirstOrDefault()?.Station;
////            }
////            catch (Exception ex)
////            {
////                Console.WriteLine($"   ❌ Erreur FindBestStationTowardsDestination: {ex.Message}");
////                return null;
////            }
////        }

////        private double CalculateProgress(Position start, Position current, Position end)
////        {
////            var totalDistance = _routeService.CalculateDistanceMeters(start, end);
////            if (totalDistance <= 0) return 0;
////            var progress = _routeService.CalculateDistanceMeters(start, current);
////            return progress / totalDistance;
////        }

////        private async Task<Position> FindCityExitPoint(Position current, Position destination, string city)
////        {
////            // Simplification pragmatique: point à 30% vers la destination depuis la position courante
////            var directionLat = (destination.lat - current.lat) * 0.3;
////            var directionLng = (destination.lng - current.lng) * 0.3;

////            return new Position
////            {
////                lat = current.lat + directionLat,
////                lng = current.lng + directionLng
////            };
////        }

////        #endregion

////        #region Existing itinerary builders (walking / bike / error)
////        private async Task<ItineraryResponse> CreateWalkingOnlyItinerary(Position origin, Position dest, string reason)
////        {
////            var route = await _routeService.GetWalkingRoute(origin, dest);

////            Console.WriteLine($"\n✅ ITINÉRAIRE MARCHE SEULE:");
////            Console.WriteLine($"   Raison: {reason}");
////            Console.WriteLine($"   Distance: {(route.Distance / 1000):F2} km");
////            Console.WriteLine($"   Durée: {(route.Duration / 60):F1} min");
////            Console.WriteLine($"   Waypoints: {route.Waypoints?.Count ?? 0}");

////            return new ItineraryResponse
////            {
////                Instructions = $"✅ Itinéraire à pied uniquement ({reason})",
////                TotalDistance = route.Distance,
////                TotalDuration = route.Duration,
////                Steps = new List<Step>
////                {
////                    new Step
////                    {
////                        Instruction = $"Marcher jusqu'à la destination ({(route.Distance/1000):F2} km)",
////                        Distance = route.Distance,
////                        Duration = route.Duration,
////                        Type = "walk",
////                        Waypoints = route.Waypoints ?? new List<Position> { origin, dest }
////                    }
////                }
////            };
////        }

////        private async Task<ItineraryResponse> CreateBikeItinerary(
////            Position origin, Position dest,
////            Station originStation, Station destStation,
////            RouteSegment walkToStation, RouteSegment bikeRoute, RouteSegment walkFromStation)
////        {
////            var steps = new List<Step>();
////            double totalDistance = 0;
////            double totalDuration = 0;

////            Console.WriteLine($"\n✅ ITINÉRAIRE AVEC VÉLO:");

////            // Marche jusqu'à la station
////            steps.Add(new Step
////            {
////                Instruction = $"🚶 Marcher jusqu'à la station '{originStation.name}' ({(walkToStation.Distance / 1000):F2} km)",
////                Distance = walkToStation.Distance,
////                Duration = walkToStation.Duration,
////                Type = "walk",
////                Waypoints = walkToStation.Waypoints ?? new List<Position> { origin, originStation.position }
////            });
////            totalDistance += walkToStation.Distance;
////            totalDuration += walkToStation.Duration;

////            // Prendre un vélo
////            steps.Add(new Step
////            {
////                Instruction = $"🚲 Prendre un vélo à '{originStation.name}' ({originStation.available_bikes} disponibles)",
////                Distance = 0,
////                Duration = 30,
////                Type = "bike",
////                Waypoints = new List<Position> { originStation.position }
////            });
////            totalDuration += 30;

////            // Vélo
////            steps.Add(new Step
////            {
////                Instruction = $"🚴 Rouler jusqu'à la station '{destStation.name}' ({(bikeRoute.Distance / 1000):F2} km)",
////                Distance = bikeRoute.Distance,
////                Duration = bikeRoute.Duration,
////                Type = "bike",
////                Waypoints = bikeRoute.Waypoints ?? new List<Position> { originStation.position, destStation.position }
////            });
////            totalDistance += bikeRoute.Distance;
////            totalDuration += bikeRoute.Duration;

////            // Déposer vélo
////            steps.Add(new Step
////            {
////                Instruction = $"🅿️ Déposer le vélo à '{destStation.name}' ({destStation.available_bike_stands} places libres)",
////                Distance = 0,
////                Duration = 30,
////                Type = "bike",
////                Waypoints = new List<Position> { destStation.position }
////            });
////            totalDuration += 30;

////            // Marche finale
////            steps.Add(new Step
////            {
////                Instruction = $"🚶 Marcher jusqu'à la destination ({(walkFromStation.Distance / 1000):F2} km)",
////                Distance = walkFromStation.Distance,
////                Duration = walkFromStation.Duration,
////                Type = "walk",
////                Waypoints = walkFromStation.Waypoints ?? new List<Position> { destStation.position, dest }
////            });
////            totalDistance += walkFromStation.Distance;
////            totalDuration += walkFromStation.Duration;

////            Console.WriteLine($"\n   📊 TOTAL: {(totalDistance / 1000):F2} km, {(totalDuration / 60):F1} min");

////            return new ItineraryResponse
////            {
////                Instructions = $"✅ Itinéraire avec vélo calculé ! Distance: {(totalDistance / 1000):F2} km - Durée: {(totalDuration / 60):F0} min",
////                TotalDistance = totalDistance,
////                TotalDuration = totalDuration,
////                Steps = steps
////            };
////        }

////        private ItineraryResponse CreateErrorResponse(string message)
////        {
////            return new ItineraryResponse
////            {
////                Instructions = message,
////                TotalDistance = 0,
////                TotalDuration = 0,
////                Steps = new List<Step>()
////            };
////        }
////        #endregion
////    }
////}



































//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.ServiceModel;
//using System.Threading.Tasks;
//using Newtonsoft.Json;

//namespace RoutingServer
//{
//    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
//    public class RoutingService : IRoutingService
//    {
//        private readonly OpenRouteService _routeService;
//        private readonly JCDecauxProxy _jcdProxy;

//        // Configuration
//        private const int MIN_TIME_SAVING_SECONDS = 120; // 2 minutes
//        private const int MAX_INTERMEDIATE_SEGMENTS = 5; // Option B choisi
//        private const double MAX_WALK_TO_STATION_METERS = 1000; // distance acceptée pour marcher vers une station
//        private const double INTERMEDIATE_WALK_THRESHOLD_METERS = 500; // marche max pour démarrer un segment (départ)
//        private const double INTERMEDIATE_BIKE_MAX_METERS = 5000; // critère vélo "raisonnable" pour segmentation

//        public RoutingService()
//        {
//            _routeService = new OpenRouteService();
//            _jcdProxy = new JCDecauxProxy();
//            Console.WriteLine("✅ RoutingService initialisé avec logique multi-contrats et stations intermédiaires");
//        }

//        public async Task<ItineraryResponse> GetItinerary(ItineraryRequest request)
//        {
//            if (request == null)
//            {
//                Console.WriteLine("❌ REQUEST EST NULL!");
//                return CreateErrorResponse("❌ Requête invalide (null)");
//            }

//            Console.WriteLine($"\n╔════════════════════════════════════════════╗");
//            Console.WriteLine($"║  🚴 NOUVELLE REQUÊTE D'ITINÉRAIRE         ║");
//            Console.WriteLine($"╚════════════════════════════════════════════╝");
//            Console.WriteLine($"   📍 Origine: '{request.Origin ?? "NULL"}'");
//            Console.WriteLine($"   🎯 Destination: '{request.Destination ?? "NULL"}'");
//            Console.WriteLine($"   🚲 Vélos min: {request.MinBikes}");

//            if (string.IsNullOrWhiteSpace(request.Origin) || string.IsNullOrWhiteSpace(request.Destination))
//            {
//                return CreateErrorResponse("❌ Origine ou destination manquante");
//            }

//            try
//            {
//                // 1) Géocodage
//                Console.WriteLine("\n📍 ÉTAPE 1: Géocodage des adresses");
//                var originPos = await _routeService.GeocodeAddress(request.Origin);
//                var destPos = await _routeService.GeocodeAddress(request.Destination);

//                if (originPos == null || destPos == null)
//                    return CreateErrorResponse("❌ Impossible de localiser l'origine ou la destination");

//                // 2) Distance directe
//                var directDistance = _routeService.CalculateDistanceMeters(originPos, destPos);
//                Console.WriteLine($"\n📏 ÉTAPE 2: Distance directe = {FormatDistance(directDistance)}");

//                // 3) Si très proche -> marche seulement
//                if (directDistance < 200)
//                {
//                    Console.WriteLine($"   ⚠️ Distance trop courte ({directDistance:F0}m < 200m) → MARCHE UNIQUEMENT");
//                    return await CreateWalkingOnlyItinerary(originPos, destPos, "distance trop courte");
//                }

//                // 4) Détection des contrats (villes)
//                var originCity = DetermineCityFromPosition(originPos);
//                var destCity = DetermineCityFromPosition(destPos);

//                Console.WriteLine($"\n🏙️ DÉTECTION DES CONTRATS:");
//                Console.WriteLine($"   Origine: {originCity}");
//                Console.WriteLine($"   Destination: {destCity}");

//                // 5) Si même contrat OU l'une des villes inconnue -> on considère traitement interne (même contrat)
//                if (string.Equals(originCity, destCity, StringComparison.OrdinalIgnoreCase) || originCity == "Unknown" || destCity == "Unknown")
//                {
//                    Console.WriteLine("   ✅ Même contrat (ou inconnue) → Itinéraire intra-contrat");
//                    return await CreateSameContractItinerary(originPos, destPos, originCity, request.MinBikes);
//                }
//                else
//                {
//                    Console.WriteLine("   ⚠️ Contrats différents → Itinéraire multi-modal (version légale)");
//                    return await CreateMultiContractItinerary(originPos, destPos, originCity, destCity, request.MinBikes);
//                }
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"\n❌ ERREUR FATALE: {ex.Message}");
//                Console.WriteLine($"   Stack: {ex.StackTrace}");
//                return CreateErrorResponse($"❌ Erreur lors du calcul: {ex.Message}");
//            }
//        }

//        #region City detection
//        private string DetermineCityFromPosition(Position position)
//        {
//            if (position == null) return "Unknown";

//            // Nice
//            if (position.lat >= 43.5 && position.lat <= 43.75 && position.lng >= 6.9 && position.lng <= 7.35) return "Nice";
//            // Lyon
//            if (position.lat >= 45.7 && position.lat <= 45.85 && position.lng >= 4.7 && position.lng <= 4.95) return "Lyon";
//            // Paris
//            if (position.lat >= 48.8 && position.lat <= 49.0 && position.lng >= 2.2 && position.lng <= 2.45) return "Paris";
//            // Marseille
//            if (position.lat >= 43.25 && position.lat <= 43.35 && position.lng >= 5.3 && position.lng <= 5.45) return "Marseille";
//            // Toulouse
//            if (position.lat >= 43.55 && position.lat <= 43.65 && position.lng >= 1.4 && position.lng <= 1.5) return "Toulouse";

//            return "Unknown";
//        }
//        #endregion

//        #region Same-contract itinerary (with smart intermediate stations)
//        private async Task<ItineraryResponse> CreateSameContractItinerary(Position origin, Position dest, string city, int minBikes)
//        {
//            Console.WriteLine($"\n🌐 ITINÉRAIRE DANS LE MÊME CONTRAT: {city}");

//            // 1) Calculer itinéraire marche seul
//            var walkOnlyRoute = await _routeService.GetWalkingRoute(origin, dest);
//            var walkingDuration = walkOnlyRoute?.Duration ?? double.PositiveInfinity;

//            // 2) Tenter itinéraire simple vélo (ta logique actuelle)
//            var originStation = await _jcdProxy.GetClosestStation(origin, minBikes > 0 ? minBikes : 1);
//            var destStation = await _jcdProxy.GetClosestStation(dest, 1);

//            RouteSegment walkToStationRoute = null;
//            RouteSegment bikeRouteSimple = null;
//            RouteSegment walkFromStationRoute = null;
//            double simpleBikeDuration = double.PositiveInfinity;

//            if (originStation != null && destStation != null)
//            {
//                walkToStationRoute = await _routeService.GetWalkingRoute(origin, originStation.position);
//                bikeRouteSimple = await _routeService.GetCyclingRoute(originStation.position, destStation.position);
//                walkFromStationRoute = await _routeService.GetWalkingRoute(destStation.position, dest);

//                simpleBikeDuration = (walkToStationRoute?.Duration ?? 0) + 30 + (bikeRouteSimple?.Duration ?? 0) + 30 + (walkFromStationRoute?.Duration ?? 0);
//            }

//            // 3) Itinéraire multi-stations (intelligent) - seulement si >=1 station disponible in city
//            var allStations = await _jcdProxy.GetStationsAsync(city);
//            var availableStations = (allStations ?? new List<Station>())
//                .Where(s => s.status == "OPEN" && s.available_bikes >= (minBikes > 0 ? minBikes : 1))
//                .ToList();

//            MultiStationRoute multiRoute = null;
//            double multiRouteDuration = double.PositiveInfinity;
//            if (availableStations.Any())
//            {
//                multiRoute = await FindOptimalMultiStationRoute(origin, dest, availableStations, MAX_INTERMEDIATE_SEGMENTS);
//                if (multiRoute != null && multiRoute.Segments.Count > 0)
//                {
//                    // Estimer durée totale pour route multi (marches + prises/dépôts + vélos)
//                    multiRouteDuration = await EstimateDurationForMultiRoute(origin, dest, multiRoute);
//                }
//            }

//            Console.WriteLine($"\n📊 COMPARAISON DES OPTIONS:");
//            Console.WriteLine($"   Marche seule: {(walkingDuration / 60):F1} min");
//            Console.WriteLine($"   Vélo simple: {(simpleBikeDuration / 60):F1} min (si station départ/arrivée disponibles)");
//            Console.WriteLine($"   Vélo multi:  {(multiRouteDuration / 60):F1} min (si multi-stations trouvées)");

//            // 4) Choisir la meilleure option : min duration among valid options
//            var bestDuration = walkingDuration;
//            var bestMode = "walk";
//            if (simpleBikeDuration < bestDuration) { bestDuration = simpleBikeDuration; bestMode = "bike_simple"; }
//            if (multiRouteDuration < bestDuration) { bestDuration = multiRouteDuration; bestMode = "bike_multi"; }

//            // 5) Si meilleur mode vélo et gain >= threshold, retourner vélo, sinon marche
//            var timeSaved = Math.Max(0, walkingDuration - bestDuration);
//            Console.WriteLine($"   Gain potentiel: {(timeSaved / 60):F1} min");

//            if (bestMode.StartsWith("bike") && timeSaved >= MIN_TIME_SAVING_SECONDS)
//            {
//                if (bestMode == "bike_simple")
//                {
//                    Console.WriteLine("   ✅ Décision: Vélo (itinéraire simple)");
//                    return await CreateBikeItinerary(origin, dest, originStation, destStation, walkToStationRoute, bikeRouteSimple, walkFromStationRoute);
//                }
//                else
//                {
//                    Console.WriteLine($"   ✅ Décision: Vélo (itinéraire multi-stations avec {multiRoute.Segments.Count} segments)");
//                    return await BuildItineraryFromMultiRoute(origin, dest, multiRoute);
//                }
//            }
//            else
//            {
//                Console.WriteLine("   ⚠️ Décision: Marche seule (vélo pas assez avantageux ou indisponible)");
//                string reason = bestMode.StartsWith("bike") ?
//                    (timeSaved == 0 ? "trajet vélo plus lent que la marche" : $"gain de temps insuffisant ({(timeSaved / 60):F1} min)") :
//                    "pas d'option vélo viable";
//                return await CreateWalkingOnlyItinerary(origin, dest, reason);
//            }
//        }

//        private async Task<double> EstimateDurationForMultiRoute(Position origin, Position dest, MultiStationRoute route)
//        {
//            double total = 0;
//            Position current = origin;

//            foreach (var seg in route.Segments)
//            {
//                var walkTo = await _routeService.GetWalkingRoute(current, seg.StartStation.position);
//                total += walkTo?.Duration ?? 0;
//                total += 30; // prise vélo
//                var bikeSeg = await _routeService.GetCyclingRoute(seg.StartStation.position, seg.EndStation.position);
//                total += bikeSeg?.Duration ?? 0;
//                total += 30; // dépôt vélo
//                current = seg.EndStation.position;
//            }

//            // marche finale
//            var finalWalk = await _routeService.GetWalkingRoute(current, dest);
//            total += finalWalk?.Duration ?? 0;

//            return total;
//        }
//        #endregion

//        #region Multi-contract itinerary (legal version)
//        private async Task<ItineraryResponse> CreateMultiContractItinerary(
//            Position origin, Position dest,
//            string originCity, string destCity, int minBikes)
//        {
//            Console.WriteLine($"\n🌍 ITINÉRAIRE MULTI-CONTRATS: {originCity} → {destCity}");

//            var steps = new List<Step>();
//            double totalDistance = 0;
//            double totalDuration = 0;

//            // PARTIE 1: Dans la ville d'origine - essayer d'utiliser vélo autant que possible
//            Console.WriteLine($"\n📍 PARTIE 1: Déplacements dans {originCity}");
//            var originStation = await FindBestStationTowardsDestination(origin, dest, originCity, minBikes);

//            if (originStation != null)
//            {
//                var walkToStation = await _routeService.GetWalkingRoute(origin, originStation.position);
//                steps.Add(new Step
//                {
//                    Instruction = $"🚶 Marcher jusqu'à '{originStation.name}' ({FormatDistance(walkToStation.Distance)})",
//                    Distance = walkToStation.Distance,
//                    Duration = walkToStation.Duration,
//                    Type = "walk",
//                    Waypoints = walkToStation.Waypoints
//                });
//                totalDistance += walkToStation.Distance;
//                totalDuration += walkToStation.Duration;

//                steps.Add(new Step
//                {
//                    Instruction = $"🚲 Prendre un vélo à '{originStation.name}' ({originStation.available_bikes} disponibles)",
//                    Distance = 0,
//                    Duration = 30,
//                    Type = "bike",
//                    Waypoints = new List<Position> { originStation.position }
//                });
//                totalDuration += 30;

//                var exitPoint = await FindCityExitPoint(originStation.position, dest, originCity);
//                var bikeRoute = await _routeService.GetCyclingRoute(originStation.position, exitPoint);
//                steps.Add(new Step
//                {
//                    Instruction = $"🚴 Rouler vers la sortie de {originCity} ({FormatDistance(bikeRoute.Distance)})",
//                    Distance = bikeRoute.Distance,
//                    Duration = bikeRoute.Duration,
//                    Type = "bike",
//                    Waypoints = bikeRoute.Waypoints
//                });
//                totalDistance += bikeRoute.Distance;
//                totalDuration += bikeRoute.Duration;

//                var exitStation = await _jcdProxy.GetClosestStation(exitPoint, 0);
//                if (exitStation != null)
//                {
//                    steps.Add(new Step
//                    {
//                        Instruction = $"🅿️ Déposer le vélo à '{exitStation.name}' ({exitStation.available_bike_stands} places)",
//                        Distance = 0,
//                        Duration = 30,
//                        Type = "bike",
//                        Waypoints = new List<Position> { exitStation.position }
//                    });
//                    totalDuration += 30;
//                }
//            }
//            else
//            {
//                Console.WriteLine("   ⚠️ Aucune station utile trouvée dans la ville d'origine → marche jusqu'au point de transition");
//            }

//            // PARTIE 2: Transition inter-cités (train/bus recommandé)
//            Console.WriteLine($"\n🚄 PARTIE 2: Transition {originCity} → {destCity}");
//            var transitionStart = originStation?.position ?? origin;
//            var transitionEnd = dest; // simplification pragmatique, on arrive au centre de la destination

//            var transitionDistance = _routeService.CalculateDistanceMeters(transitionStart, transitionEnd);
//            // train speed ~72km/h => 20 m/s
//            var transitionDuration = transitionDistance / 20.0;

//            steps.Add(new Step
//            {
//                Instruction = $"🚄 Transition inter-cités (train/bus recommandé, {FormatDistance(transitionDistance)})",
//                Distance = transitionDistance,
//                Duration = transitionDuration,
//                Type = "transit",
//                Waypoints = new List<Position> { transitionStart, transitionEnd }
//            });
//            totalDistance += transitionDistance;
//            totalDuration += transitionDuration;

//            // PARTIE 3: Dans la ville de destination - essayer vélo local si possible
//            Console.WriteLine($"\n📍 PARTIE 3: Déplacements dans {destCity}");
//            var destStation = await _jcdProxy.GetClosestStation(dest, minBikes > 0 ? minBikes : 1);
//            if (destStation != null)
//            {
//                var walkToDestStation = await _routeService.GetWalkingRoute(transitionEnd, destStation.position);
//                steps.Add(new Step
//                {
//                    Instruction = $"🚶 Marcher jusqu'à '{destStation.name}' ({FormatDistance(walkToDestStation.Distance)})",
//                    Distance = walkToDestStation.Distance,
//                    Duration = walkToDestStation.Duration,
//                    Type = "walk",
//                    Waypoints = walkToDestStation.Waypoints
//                });
//                totalDistance += walkToDestStation.Distance;
//                totalDuration += walkToDestStation.Duration;

//                steps.Add(new Step
//                {
//                    Instruction = $"🚲 Prendre un vélo à '{destStation.name}'",
//                    Distance = 0,
//                    Duration = 30,
//                    Type = "bike",
//                    Waypoints = new List<Position> { destStation.position }
//                });
//                totalDuration += 30;

//                var finalBikeRoute = await _routeService.GetCyclingRoute(destStation.position, dest);
//                steps.Add(new Step
//                {
//                    Instruction = $"🚴 Rouler jusqu'à la destination ({FormatDistance(finalBikeRoute.Distance)})",
//                    Distance = finalBikeRoute.Distance,
//                    Duration = finalBikeRoute.Duration,
//                    Type = "bike",
//                    Waypoints = finalBikeRoute.Waypoints
//                });
//                totalDistance += finalBikeRoute.Distance;
//                totalDuration += finalBikeRoute.Duration;

//                var finalStation = await _jcdProxy.GetClosestStation(dest, 0);
//                if (finalStation != null)
//                {
//                    steps.Add(new Step
//                    {
//                        Instruction = $"🅿️ Déposer le vélo à '{finalStation.name}'",
//                        Distance = 0,
//                        Duration = 30,
//                        Type = "bike",
//                        Waypoints = new List<Position> { finalStation.position }
//                    });
//                    totalDuration += 30;
//                }

//                var finalWalk = await _routeService.GetWalkingRoute(finalStation?.position ?? destStation.position, dest);
//                steps.Add(new Step
//                {
//                    Instruction = $"🚶 Marcher jusqu'à la destination ({FormatDistance(finalWalk.Distance)})",
//                    Distance = finalWalk.Distance,
//                    Duration = finalWalk.Duration,
//                    Type = "walk",
//                    Waypoints = finalWalk.Waypoints
//                });
//                totalDistance += finalWalk.Distance;
//                totalDuration += finalWalk.Duration;
//            }
//            else
//            {
//                Console.WriteLine("   ⚠️ Aucune station disponible dans la ville de destination → marche/transit final");
//                var finalWalk = await _routeService.GetWalkingRoute(transitionEnd, dest);
//                steps.Add(new Step
//                {
//                    Instruction = $"🚶 Marcher jusqu'à la destination ({FormatDistance(finalWalk.Distance)})",
//                    Distance = finalWalk.Distance,
//                    Duration = finalWalk.Duration,
//                    Type = "walk",
//                    Waypoints = finalWalk.Waypoints
//                });
//                totalDistance += finalWalk.Distance;
//                totalDuration += finalWalk.Duration;
//            }

//            Console.WriteLine($"\n   📊 TOTAL MULTI-CONTRATS: {FormatDistance(totalDistance)}, {(totalDuration / 60):F1} min");

//            return new ItineraryResponse
//            {
//                Instructions = $"✅ Itinéraire multi-villes calculé ! {originCity} → {destCity}",
//                TotalDistance = totalDistance,
//                TotalDuration = totalDuration,
//                Steps = steps
//            };
//        }
//        #endregion

//        #region Helpers for multi-contract & intermediate stations
//        private async Task<Station> FindBestStationTowardsDestination(Position origin, Position finalDest, string city, int minBikes)
//        {
//            Console.WriteLine($"   🔎 Recherche stations dans {city} pour s'approcher de la destination...");
//            try
//            {
//                var allStations = await _jcdProxy.GetStationsAsync(city);
//                if (allStations == null || allStations.Count == 0) return null;

//                var availableStations = allStations
//                    .Where(s => s.available_bikes >= (minBikes > 0 ? minBikes : 1) && string.Equals(s.status, "OPEN", StringComparison.OrdinalIgnoreCase))
//                    .ToList();

//                if (!availableStations.Any()) return null;

//                var candidates = availableStations
//                    .Select(station => new
//                    {
//                        Station = station,
//                        WalkDistance = _routeService.CalculateDistanceMeters(origin, station.position),
//                        ProgressTowardsDest = CalculateProgress(origin, station.position, finalDest)
//                    })
//                    .Where(x => x.WalkDistance < MAX_WALK_TO_STATION_METERS)
//                    .OrderByDescending(x => x.ProgressTowardsDest)
//                    .ThenBy(x => x.WalkDistance)
//                    .ToList();

//                return candidates.FirstOrDefault()?.Station;
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"   ❌ Erreur FindBestStationTowardsDestination: {ex.Message}");
//                return null;
//            }
//        }

//        private double CalculateProgress(Position start, Position current, Position end)
//        {
//            var totalDistance = _routeService.CalculateDistanceMeters(start, end);
//            if (totalDistance <= 0) return 0;
//            var progress = _routeService.CalculateDistanceMeters(start, current);
//            return progress / totalDistance;
//        }

//        private async Task<Position> FindCityExitPoint(Position current, Position destination, string city)
//        {
//            // Point pragmatique vers la destination — peut être amélioré par cartographie réelle
//            var directionLat = (destination.lat - current.lat) * 0.3;
//            var directionLng = (destination.lng - current.lng) * 0.3;

//            return new Position
//            {
//                lat = current.lat + directionLat,
//                lng = current.lng + directionLng
//            };
//        }

//        // Find optimal multi-station route (greedy) up to maxSegments
//        private async Task<MultiStationRoute> FindOptimalMultiStationRoute(Position origin, Position dest, List<Station> availableStations, int maxSegments)
//        {
//            Console.WriteLine("   🔀 Recherche de route multi-stations (greedy)");
//            var route = new MultiStationRoute();
//            var currentPos = origin;
//            var used = new HashSet<int>();

//            for (int i = 0; i < maxSegments; i++)
//            {
//                // Find start station accessible (walk threshold)
//                var start = FindBestAccessibleStation(currentPos, dest, availableStations, used, isStart: true);
//                if (start == null) break;
//                used.Add(start.number);

//                // Find end station for this segment
//                var end = FindBestAccessibleStation(start.position, dest, availableStations, used, isStart: false)
//                          ?? FindClosestStationToDestination(dest, availableStations, used);

//                if (end == null) break;
//                used.Add(end.number);

//                // Sanity: ensure end is different from start
//                if (end.number == start.number) break;

//                route.Segments.Add(new BikeSegment { StartStation = start, EndStation = end });
//                currentPos = end.position;

//                var distToDest = _routeService.CalculateDistanceMeters(currentPos, dest);
//                if (distToDest < 500) break; // close enough
//            }

//            return route.Segments.Count > 0 ? route : null;
//        }

//        private Station FindBestAccessibleStation(Position from, Position finalDest, List<Station> stations, HashSet<int> usedStations, bool isStart)
//        {
//            var walkLimit = isStart ? INTERMEDIATE_WALK_THRESHOLD_METERS : MAX_WALK_TO_STATION_METERS;
//            var fromToDest = _routeService.CalculateDistanceMeters(from, finalDest);

//            var query = stations
//                .Where(s => !usedStations.Contains(s.number) && s.status == "OPEN" && s.available_bike_stands >= 0)
//                .Select(s => new
//                {
//                    Station = s,
//                    WalkDistance = _routeService.CalculateDistanceMeters(from, s.position),
//                    DistToDest = _routeService.CalculateDistanceMeters(s.position, finalDest),
//                    Progress = CalculateProgress(from, s.position, finalDest)
//                })
//                .Where(x => x.WalkDistance <= walkLimit) // reachable by foot
//                .Where(x => x.DistToDest < fromToDest + 0.1 * fromToDest) // must progress toward dest (with tolerance)
//                .OrderByDescending(x => x.Progress)
//                .ThenBy(x => x.WalkDistance)
//                .ToList();

//            return query.FirstOrDefault()?.Station;
//        }

//        private Station FindClosestStationToDestination(Position dest, List<Station> stations, HashSet<int> usedStations)
//        {
//            return stations
//                .Where(s => !usedStations.Contains(s.number) && s.available_bike_stands > 0 && s.status == "OPEN")
//                .OrderBy(s => _routeService.CalculateDistanceMeters(s.position, dest))
//                .FirstOrDefault();
//        }

//        // Build itinerary from a MultiStationRoute (concrete steps)
//        private async Task<ItineraryResponse> BuildItineraryFromMultiRoute(Position origin, Position dest, MultiStationRoute route)
//        {
//            var steps = new List<Step>();
//            double totalDistance = 0;
//            double totalDuration = 0;
//            var current = origin;

//            foreach (var seg in route.Segments)
//            {
//                var walkTo = await _routeService.GetWalkingRoute(current, seg.StartStation.position);
//                steps.Add(new Step
//                {
//                    Instruction = $"🚶 Marcher jusqu'à '{seg.StartStation.name}' ({FormatDistance(walkTo.Distance)})",
//                    Distance = walkTo.Distance,
//                    Duration = walkTo.Duration,
//                    Type = "walk",
//                    Waypoints = walkTo.Waypoints
//                });
//                totalDistance += walkTo.Distance;
//                totalDuration += walkTo.Duration;

//                steps.Add(new Step
//                {
//                    Instruction = $"🚲 Prendre un vélo à '{seg.StartStation.name}' ({seg.StartStation.available_bikes} disponibles)",
//                    Distance = 0,
//                    Duration = 30,
//                    Type = "bike",
//                    Waypoints = new List<Position> { seg.StartStation.position }
//                });
//                totalDuration += 30;

//                var bikeSeg = await _routeService.GetCyclingRoute(seg.StartStation.position, seg.EndStation.position);
//                steps.Add(new Step
//                {
//                    Instruction = $"🚴 Rouler jusqu'à '{seg.EndStation.name}' ({FormatDistance(bikeSeg.Distance)})",
//                    Distance = bikeSeg.Distance,
//                    Duration = bikeSeg.Duration,
//                    Type = "bike",
//                    Waypoints = bikeSeg.Waypoints
//                });
//                totalDistance += bikeSeg.Distance;
//                totalDuration += bikeSeg.Duration;

//                steps.Add(new Step
//                {
//                    Instruction = $"🅿️ Déposer le vélo à '{seg.EndStation.name}' ({seg.EndStation.available_bike_stands} places)",
//                    Distance = 0,
//                    Duration = 30,
//                    Type = "bike",
//                    Waypoints = new List<Position> { seg.EndStation.position }
//                });
//                totalDuration += 30;

//                current = seg.EndStation.position;
//            }

//            // Final walk to destination
//            var finalWalk = await _routeService.GetWalkingRoute(current, dest);
//            steps.Add(new Step
//            {
//                Instruction = $"🚶 Marcher jusqu'à la destination ({FormatDistance(finalWalk.Distance)})",
//                Distance = finalWalk.Distance,
//                Duration = finalWalk.Duration,
//                Type = "walk",
//                Waypoints = finalWalk.Waypoints
//            });
//            totalDistance += finalWalk.Distance;
//            totalDuration += finalWalk.Duration;

//            Console.WriteLine($"\n   📊 TOTAL MULTI-STATIONS: {FormatDistance(totalDistance)}, {(totalDuration / 60):F1} min");

//            return new ItineraryResponse
//            {
//                Instructions = $"✅ Itinéraire optimisé avec {route.Segments.Count} stations intermédiaires",
//                TotalDistance = totalDistance,
//                TotalDuration = totalDuration,
//                Steps = steps
//            };
//        }
//        #endregion

//        #region Existing itinerary builders (walking / bike / error)
//        private async Task<ItineraryResponse> CreateWalkingOnlyItinerary(Position origin, Position dest, string reason)
//        {
//            var route = await _routeService.GetWalkingRoute(origin, dest);

//            Console.WriteLine($"\n✅ ITINÉRAIRE MARCHE SEULE:");
//            Console.WriteLine($"   Raison: {reason}");
//            Console.WriteLine($"   Distance: {FormatDistance(route.Distance)}");
//            Console.WriteLine($"   Durée: {(route.Duration / 60):F1} min");
//            Console.WriteLine($"   Waypoints: {route.Waypoints?.Count ?? 0}");

//            return new ItineraryResponse
//            {
//                Instructions = $"✅ Itinéraire à pied uniquement ({reason})",
//                TotalDistance = route.Distance,
//                TotalDuration = route.Duration,
//                Steps = new List<Step>
//                {
//                    new Step
//                    {
//                        Instruction = $"Marcher jusqu'à la destination ({FormatDistance(route.Distance)})",
//                        Distance = route.Distance,
//                        Duration = route.Duration,
//                        Type = "walk",
//                        Waypoints = route.Waypoints ?? new List<Position> { origin, dest }
//                    }
//                }
//            };
//        }

//        private async Task<ItineraryResponse> CreateBikeItinerary(
//            Position origin, Position dest,
//            Station originStation, Station destStation,
//            RouteSegment walkToStation, RouteSegment bikeRoute, RouteSegment walkFromStation)
//        {
//            var steps = new List<Step>();
//            double totalDistance = 0;
//            double totalDuration = 0;

//            Console.WriteLine($"\n✅ ITINÉRAIRE AVEC VÉLO:");

//            // Walk to start station
//            steps.Add(new Step
//            {
//                Instruction = $"🚶 Marcher jusqu'à la station '{originStation.name}' ({FormatDistance(walkToStation.Distance)})",
//                Distance = walkToStation.Distance,
//                Duration = walkToStation.Duration,
//                Type = "walk",
//                Waypoints = walkToStation.Waypoints ?? new List<Position> { origin, originStation.position }
//            });
//            totalDistance += walkToStation.Distance;
//            totalDuration += walkToStation.Duration;

//            // Take bike
//            steps.Add(new Step
//            {
//                Instruction = $"🚲 Prendre un vélo à '{originStation.name}' ({originStation.available_bikes} disponibles)",
//                Distance = 0,
//                Duration = 30,
//                Type = "bike",
//                Waypoints = new List<Position> { originStation.position }
//            });
//            totalDuration += 30;

//            // Bike
//            steps.Add(new Step
//            {
//                Instruction = $"🚴 Rouler jusqu'à la station '{destStation.name}' ({FormatDistance(bikeRoute.Distance)})",
//                Distance = bikeRoute.Distance,
//                Duration = bikeRoute.Duration,
//                Type = "bike",
//                Waypoints = bikeRoute.Waypoints ?? new List<Position> { originStation.position, destStation.position }
//            });
//            totalDistance += bikeRoute.Distance;
//            totalDuration += bikeRoute.Duration;

//            // Drop bike
//            steps.Add(new Step
//            {
//                Instruction = $"🅿️ Déposer le vélo à '{destStation.name}' ({destStation.available_bike_stands} places libres)",
//                Distance = 0,
//                Duration = 30,
//                Type = "bike",
//                Waypoints = new List<Position> { destStation.position }
//            });
//            totalDuration += 30;

//            // Final walk
//            steps.Add(new Step
//            {
//                Instruction = $"🚶 Marcher jusqu'à la destination ({FormatDistance(walkFromStation.Distance)})",
//                Distance = walkFromStation.Distance,
//                Duration = walkFromStation.Duration,
//                Type = "walk",
//                Waypoints = walkFromStation.Waypoints ?? new List<Position> { destStation.position, dest }
//            });
//            totalDistance += walkFromStation.Distance;
//            totalDuration += walkFromStation.Duration;

//            Console.WriteLine($"\n   📊 TOTAL: {FormatDistance(totalDistance)}, {(totalDuration / 60):F1} min");

//            return new ItineraryResponse
//            {
//                Instructions = $"✅ Itinéraire avec vélo calculé ! Distance: {FormatDistance(totalDistance)} - Durée: {(totalDuration / 60):F0} min",
//                TotalDistance = totalDistance,
//                TotalDuration = totalDuration,
//                Steps = steps
//            };
//        }

//        private ItineraryResponse CreateErrorResponse(string message)
//        {
//            return new ItineraryResponse
//            {
//                Instructions = message,
//                TotalDistance = 0,
//                TotalDuration = 0,
//                Steps = new List<Step>()
//            };
//        }
//        #endregion

//        #region Utilities & small classes
//        private string FormatDistance(double meters)
//        {
//            if (double.IsInfinity(meters) || double.IsNaN(meters)) return "inconnue";
//            if (meters < 1000) return $"{meters:F0} m";
//            var km = meters / 1000.0;
//            if (km < 10) return $"{km:F2} km";
//            if (km < 100) return $"{km:F1} km";
//            return $"{km:F0} km";
//        }

//        // Small types used for multi-station routing
//        private class MultiStationRoute
//        {
//            public List<BikeSegment> Segments { get; set; } = new List<BikeSegment>();
//        }

//        private class BikeSegment
//        {
//            public Station StartStation { get; set; }
//            public Station EndStation { get; set; }
//        }
//        #endregion
//    }
//}







using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;

namespace RoutingServer
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class RoutingService : IRoutingService
    {
        private readonly OpenRouteService _routeService;
        private readonly JCDecauxProxy _jcdProxy;

        // Configuration
        private const int MIN_TIME_SAVING_SECONDS = 120;      // 2 minutes
        private const int MAX_INTERMEDIATE_SEGMENTS = 5;      // utilisé pour multi-stations (si activé)
        private const int MAX_MULTI_BIKES = 3;                // <-- Choix : 3 vélos max
        private const double MAX_WALK_TO_STATION_METERS = 1000;
        private const double INTERMEDIATE_WALK_THRESHOLD_METERS = 500;
        private const double PROGRESS_TOLERANCE_RATIO = 0.10; // tolérance pour accepter progression

        public RoutingService()
        {
            _routeService = new OpenRouteService();
            _jcdProxy = new JCDecauxProxy();
            Console.WriteLine("✅ RoutingService initialisé (intelligent multi-bike, max 3 vélos)");
        }

        public async Task<ItineraryResponse> GetItinerary(ItineraryRequest request)
        {
            if (request == null)
            {
                Console.WriteLine("❌ REQUEST EST NULL!");
                return CreateErrorResponse("❌ Requête invalide (null)");
            }

            Console.WriteLine($"\n╔════════════════════════════════════════════╗");
            Console.WriteLine($"║  🚴 NOUVELLE REQUÊTE D'ITINÉRAIRE         ║");
            Console.WriteLine($"╚════════════════════════════════════════════╝");
            Console.WriteLine($"   📍 Origine: '{request.Origin ?? "NULL"}'");
            Console.WriteLine($"   🎯 Destination: '{request.Destination ?? "NULL"}'");
            Console.WriteLine($"   🚲 Vélos min: {request.MinBikes}");

            if (string.IsNullOrWhiteSpace(request.Origin) || string.IsNullOrWhiteSpace(request.Destination))
                return CreateErrorResponse("❌ Origine ou destination manquante");

            try
            {
                // Géocodage
                Console.WriteLine("\n📍 ÉTAPE 1: Géocodage des adresses");
                var originPos = await _routeService.GeocodeAddress(request.Origin);
                var destPos = await _routeService.GeocodeAddress(request.Destination);

                if (originPos == null || destPos == null)
                    return CreateErrorResponse("❌ Impossible de localiser l'origine ou la destination");

                // Distance directe
                var directDistance = _routeService.CalculateDistanceMeters(originPos, destPos);
                Console.WriteLine($"\n📏 ÉTAPE 2: Distance directe = {FormatDistance(directDistance)}");

                // Trop proche
                if (directDistance < 200)
                {
                    Console.WriteLine($"   ⚠️ Distance trop courte ({directDistance:F0}m < 200m) → MARCHE UNIQUEMENT");
                    return await CreateWalkingOnlyItinerary(originPos, destPos, "distance trop courte");
                }

                // Contrats (villes)
                var originCity = DetermineCityFromPosition(originPos);
                var destCity = DetermineCityFromPosition(destPos);
                Console.WriteLine($"\n🏙️ DÉTECTION DES CONTRATS: Origine: {originCity} | Destination: {destCity}");

                // Même contrat ou inconnue => intra-contrat logic (avec multi-bike intelligent)
                if (string.Equals(originCity, destCity, StringComparison.OrdinalIgnoreCase) || originCity == "Unknown" || destCity == "Unknown")
                {
                    Console.WriteLine("   ✅ Itinéraire intra-contrat (traitement intelligent)");
                    return await CreateSameContractItinerary(originPos, destPos, originCity, request.MinBikes);
                }
                else
                {
                    Console.WriteLine("   ⚠️ Itinéraire multi-contrats (version légale)");
                    return await CreateMultiContractItinerary(originPos, destPos, originCity, destCity, request.MinBikes);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ ERREUR FATALE: {ex.Message}");
                Console.WriteLine($"   Stack: {ex.StackTrace}");
                return CreateErrorResponse($"❌ Erreur lors du calcul: {ex.Message}");
            }
        }

        #region City detection
        private string DetermineCityFromPosition(Position position)
        {
            if (position == null) return "Unknown";

            if (position.lat >= 43.5 && position.lat <= 43.75 && position.lng >= 6.9 && position.lng <= 7.35) return "Nice";
            if (position.lat >= 45.7 && position.lat <= 45.85 && position.lng >= 4.7 && position.lng <= 4.95) return "Lyon";
            if (position.lat >= 48.8 && position.lat <= 49.0 && position.lng >= 2.2 && position.lng <= 2.45) return "Paris";
            if (position.lat >= 43.25 && position.lat <= 43.35 && position.lng >= 5.3 && position.lng <= 5.45) return "Marseille";
            if (position.lat >= 43.55 && position.lat <= 43.65 && position.lng >= 1.4 && position.lng <= 1.5) return "Toulouse";

            return "Unknown";
        }
        #endregion

        #region Same-contract itinerary (intelligent multi-bike)
        private async Task<ItineraryResponse> CreateSameContractItinerary(Position origin, Position dest, string city, int minBikes)
        {
            Console.WriteLine($"\n🌐 ITINÉRAIRE INTRA-CONTRAT (intelligent) : {city}");

            // 1) Marche seule
            var walkOnlyRoute = await _routeService.GetWalkingRoute(origin, dest);
            var walkingDuration = walkOnlyRoute?.Duration ?? double.PositiveInfinity;

            // 2) Itinéraire simple vélo (1 vélo) - si stations disponibles
            var originStation = await _jcdProxy.GetClosestStation(origin, minBikes > 0 ? minBikes : 1);
            var destStation = await _jcdProxy.GetClosestStation(dest, 1);

            RouteSegment walkToStation = null, bikeSimple = null, walkFromStation = null;
            double simpleBikeDuration = double.PositiveInfinity;
            if (originStation != null && destStation != null)
            {
                walkToStation = await _routeService.GetWalkingRoute(origin, originStation.position);
                bikeSimple = await _routeService.GetCyclingRoute(originStation.position, destStation.position);
                walkFromStation = await _routeService.GetWalkingRoute(destStation.position, dest);
                simpleBikeDuration = (walkToStation?.Duration ?? 0) + 30 + (bikeSimple?.Duration ?? 0) + 30 + (walkFromStation?.Duration ?? 0);
            }

            // 3) Itinéraire multi-bike intelligent (jusqu'à MAX_MULTI_BIKES)
            var allStations = await _jcdProxy.GetStationsAsync(city) ?? new List<Station>();
            var availableStations = allStations
                .Where(s => s.status == "OPEN" && s.available_bikes >= (minBikes > 0 ? minBikes : 1))
                .ToList();

            MultiBikeChain bestChain = null;
            double bestChainDuration = double.PositiveInfinity;
            if (availableStations.Any())
            {
                bestChain = await BuildOptimizedBikeChain(origin, dest, availableStations, MAX_MULTI_BIKES);
                if (bestChain != null)
                    bestChainDuration = await EstimateDurationForBikeChain(origin, dest, bestChain);
            }

            // Logging
            Console.WriteLine($"\n📊 OPTIONS:");
            Console.WriteLine($"   Marche seule: {(walkingDuration / 60):F1} min");
            Console.WriteLine($"   Vélo simple (1): {(simpleBikeDuration / 60):F1} min");
            Console.WriteLine($"   Vélo multi (chain): {(bestChainDuration / 60):F1} min");

            // Choisir la meilleure option
            var bestDuration = walkingDuration;
            var bestMode = "walk";
            if (simpleBikeDuration < bestDuration) { bestDuration = simpleBikeDuration; bestMode = "bike_simple"; }
            if (bestChainDuration < bestDuration) { bestDuration = bestChainDuration; bestMode = "bike_chain"; }

            var timeSaved = Math.Max(0, walkingDuration - bestDuration);
            Console.WriteLine($"   Gain potentiel: {(timeSaved / 60):F1} min");

            if (bestMode == "bike_chain" && timeSaved >= MIN_TIME_SAVING_SECONDS)
            {
                Console.WriteLine($"   ✅ Décision: vélo multi (chain) avec {bestChain?.Segments?.Count ?? 0} segments");
                return await BuildItineraryFromBikeChain(origin, dest, bestChain);
            }
            else if (bestMode == "bike_simple" && timeSaved >= MIN_TIME_SAVING_SECONDS)
            {
                Console.WriteLine("   ✅ Décision: vélo simple");
                return await CreateBikeItinerary(origin, dest, originStation, destStation, walkToStation, bikeSimple, walkFromStation);
            }
            else
            {
                Console.WriteLine("   ⚠️ Décision: marche seule (vélo non avantageux ou indisponible)");
                string reason = bestMode.StartsWith("bike") ?
                    (timeSaved == 0 ? "trajet vélo plus lent que la marche" : $"gain de temps insuffisant ({(timeSaved / 60):F1} min)") :
                    "pas d'option vélo viable";
                return await CreateWalkingOnlyItinerary(origin, dest, reason);
            }
        }

        // Build optimized bike chain (intelligent greedy + evaluation)
        private async Task<MultiBikeChain> BuildOptimizedBikeChain(Position origin, Position dest, List<Station> stations, int maxBikes)
        {
            // Greedy approach with local evaluation:
            // At each step pick next StartStation reachable by walk, and a subsequent EndStation that advances towards destination.
            // Stop if adding another bike does not improve estimated duration against walking.
            Console.WriteLine("   🔎 BuildOptimizedBikeChain: recherche chaines potentielles...");

            MultiBikeChain bestChain = null;
            double bestDuration = double.PositiveInfinity;

            // We will build chains of length 1..maxBikes and keep the best.
            for (int chainLength = 1; chainLength <= maxBikes; chainLength++)
            {
                var chain = await GreedyBuildChainWithLength(origin, dest, stations, chainLength);
                if (chain == null || chain.Segments.Count == 0) continue;

                var estDuration = await EstimateDurationForBikeChain(origin, dest, chain);
                Console.WriteLine($"      Chaîne longueur {chainLength}: durée estimée {(estDuration / 60):F1} min");

                if (estDuration < bestDuration)
                {
                    bestDuration = estDuration;
                    bestChain = chain;
                }
            }

            return bestChain;
        }

        // Greedy helper: attempt to build a chain with exactly chainLength bike segments
        private async Task<MultiBikeChain> GreedyBuildChainWithLength(Position origin, Position dest, List<Station> stations, int chainLength)
        {
            var used = new HashSet<int>();
            var chain = new MultiBikeChain();

            Position currentPos = origin;
            for (int seg = 0; seg < chainLength; seg++)
            {
                // Choose start station reachable from currentPos
                var start = stations
                    .Where(s => !used.Contains(s.number) && s.status == "OPEN")
                    .Select(s => new
                    {
                        Station = s,
                        WalkDist = _routeService.CalculateDistanceMeters(currentPos, s.position),
                        Progress = CalculateProgress(currentPos, s.position, dest)
                    })
                    .Where(x => x.WalkDist <= INTERMEDIATE_WALK_THRESHOLD_METERS)
                    .OrderByDescending(x => x.Progress)
                    .ThenBy(x => x.WalkDist)
                    .FirstOrDefault()?.Station;

                if (start == null) return null; // cannot build chain of desired length

                used.Add(start.number);

                // Choose end station that advances towards dest and differs from start
                var endCandidates = stations
                    .Where(s => !used.Contains(s.number) && s.number != start.number && s.status == "OPEN")
                    .Select(s => new
                    {
                        Station = s,
                        BikeDist = _routeService.CalculateDistanceMeters(start.position, s.position),
                        DistToDest = _routeService.CalculateDistanceMeters(s.position, dest),
                        Progress = CalculateProgress(start.position, s.position, dest)
                    })
                    // Prefer ones that progress toward dest and are not absurdly far to bike
                    .Where(x => x.Progress > 0.01)
                    .OrderByDescending(x => x.Progress)
                    .ThenBy(x => x.BikeDist)
                    .ToList();

                Station chosenEnd = null;
                if (endCandidates.Any())
                {
                    chosenEnd = endCandidates.First().Station;
                }
                else
                {
                    // fallback: pick closest station to destination (not used)
                    chosenEnd = stations
                        .Where(s => !used.Contains(s.number) && s.status == "OPEN")
                        .OrderBy(s => _routeService.CalculateDistanceMeters(s.position, dest))
                        .FirstOrDefault();
                }

                if (chosenEnd == null) return null;

                used.Add(chosenEnd.number);

                chain.Segments.Add(new BikeLeg { StartStation = start, EndStation = chosenEnd });
                currentPos = chosenEnd.position;
            }

            return chain;
        }

        // Estimate duration for a bike chain (walks + takes + bikes + drops + final walk)
        private async Task<double> EstimateDurationForBikeChain(Position origin, Position dest, MultiBikeChain chain)
        {
            double total = 0;
            Position current = origin;

            foreach (var leg in chain.Segments)
            {
                var walkTo = await _routeService.GetWalkingRoute(current, leg.StartStation.position);
                total += walkTo?.Duration ?? 0;
                total += 30; // prise vélo
                var bikeSeg = await _routeService.GetCyclingRoute(leg.StartStation.position, leg.EndStation.position);
                total += bikeSeg?.Duration ?? 0;
                total += 30; // dépôt
                current = leg.EndStation.position;
            }

            var finalWalk = await _routeService.GetWalkingRoute(current, dest);
            total += finalWalk?.Duration ?? 0;
            return total;
        }

        // Build itinerary from a bike chain (concrete steps)
        private async Task<ItineraryResponse> BuildItineraryFromBikeChain(Position origin, Position dest, MultiBikeChain chain)
        {
            var steps = new List<Step>();
            double totalDistance = 0;
            double totalDuration = 0;
            var current = origin;

            foreach (var leg in chain.Segments)
            {
                var walkTo = await _routeService.GetWalkingRoute(current, leg.StartStation.position);
                steps.Add(new Step
                {
                    Instruction = $"🚶 Marcher jusqu'à '{leg.StartStation.name}' ({FormatDistance(walkTo.Distance)})",
                    Distance = walkTo.Distance,
                    Duration = walkTo.Duration,
                    Type = "walk",
                    Waypoints = walkTo.Waypoints
                });
                totalDistance += walkTo.Distance;
                totalDuration += walkTo.Duration;

                steps.Add(new Step
                {
                    Instruction = $"🚲 Prendre un vélo à '{leg.StartStation.name}' ({leg.StartStation.available_bikes} disponibles)",
                    Distance = 0,
                    Duration = 30,
                    Type = "bike",
                    Waypoints = new List<Position> { leg.StartStation.position }
                });
                totalDuration += 30;

                var bikeSeg = await _routeService.GetCyclingRoute(leg.StartStation.position, leg.EndStation.position);
                steps.Add(new Step
                {
                    Instruction = $"🚴 Rouler jusqu'à '{leg.EndStation.name}' ({FormatDistance(bikeSeg.Distance)})",
                    Distance = bikeSeg.Distance,
                    Duration = bikeSeg.Duration,
                    Type = "bike",
                    Waypoints = bikeSeg.Waypoints
                });
                totalDistance += bikeSeg.Distance;
                totalDuration += bikeSeg.Duration;

                steps.Add(new Step
                {
                    Instruction = $"🅿️ Déposer le vélo à '{leg.EndStation.name}' ({leg.EndStation.available_bike_stands} places)",
                    Distance = 0,
                    Duration = 30,
                    Type = "bike",
                    Waypoints = new List<Position> { leg.EndStation.position }
                });
                totalDuration += 30;

                current = leg.EndStation.position;
            }

            // Final walk
            var finalWalk = await _routeService.GetWalkingRoute(current, dest);
            steps.Add(new Step
            {
                Instruction = $"🚶 Marcher jusqu'à la destination ({FormatDistance(finalWalk.Distance)})",
                Distance = finalWalk.Distance,
                Duration = finalWalk.Duration,
                Type = "walk",
                Waypoints = finalWalk.Waypoints
            });
            totalDistance += finalWalk.Distance;
            totalDuration += finalWalk.Duration;

            Console.WriteLine($"\n   📊 TOTAL CHAÎNE VÉLO: {FormatDistance(totalDistance)}, {(totalDuration / 60):F1} min");

            return new ItineraryResponse
            {
                Instructions = $"✅ Itinéraire optimisé multi-vélos ({chain.Segments.Count} segments)",
                TotalDistance = totalDistance,
                TotalDuration = totalDuration,
                Steps = steps
            };
        }
        #endregion

        #region Multi-contract itinerary (legal)
        private async Task<ItineraryResponse> CreateMultiContractItinerary(
            Position origin, Position dest,
            string originCity, string destCity, int minBikes)
        {
            Console.WriteLine($"\n🌍 ITINÉRAIRE MULTI-CONTRATS: {originCity} → {destCity}");

            var steps = new List<Step>();
            double totalDistance = 0;
            double totalDuration = 0;

            // Partie 1: essayer d'utiliser vélo dans la ville d'origine (intelligemment)
            Console.WriteLine($"\n📍 PARTIE 1: Déplacements dans {originCity}");
            var originStation = await FindBestStationTowardsDestination(origin, dest, originCity, minBikes);
            if (originStation != null)
            {
                var walkToStation = await _routeService.GetWalkingRoute(origin, originStation.position);
                steps.Add(new Step
                {
                    Instruction = $"🚶 Marcher jusqu'à '{originStation.name}' ({FormatDistance(walkToStation.Distance)})",
                    Distance = walkToStation.Distance,
                    Duration = walkToStation.Duration,
                    Type = "walk",
                    Waypoints = walkToStation.Waypoints
                });
                totalDistance += walkToStation.Distance;
                totalDuration += walkToStation.Duration;

                steps.Add(new Step
                {
                    Instruction = $"🚲 Prendre un vélo à '{originStation.name}' ({originStation.available_bikes} disponibles)",
                    Distance = 0,
                    Duration = 30,
                    Type = "bike",
                    Waypoints = new List<Position> { originStation.position }
                });
                totalDuration += 30;

                var exitPoint = await FindCityExitPoint(originStation.position, dest, originCity);
                var bikeRoute = await _routeService.GetCyclingRoute(originStation.position, exitPoint);
                steps.Add(new Step
                {
                    Instruction = $"🚴 Rouler vers la sortie de {originCity} ({FormatDistance(bikeRoute.Distance)})",
                    Distance = bikeRoute.Distance,
                    Duration = bikeRoute.Duration,
                    Type = "bike",
                    Waypoints = bikeRoute.Waypoints
                });
                totalDistance += bikeRoute.Distance;
                totalDuration += bikeRoute.Duration;

                // déposer vélo si station disponible
                var exitStation = await _jcdProxy.GetClosestStation(exitPoint, 0);
                if (exitStation != null)
                {
                    steps.Add(new Step
                    {
                        Instruction = $"🅿️ Déposer le vélo à '{exitStation.name}' ({exitStation.available_bike_stands} places)",
                        Distance = 0,
                        Duration = 30,
                        Type = "bike",
                        Waypoints = new List<Position> { exitStation.position }
                    });
                    totalDuration += 30;
                }
            }
            else
            {
                Console.WriteLine("   ⚠️ Pas de station utile dans la ville d'origine");
            }

            // Partie 2: transition (train/bus)
            Console.WriteLine($"\n🚄 PARTIE 2: Transition {originCity} → {destCity}");
            var transitionStart = originStation?.position ?? origin;
            var transitionEnd = dest; // simplification pragmatique

            var transitionDistance = _routeService.CalculateDistanceMeters(transitionStart, transitionEnd);
            var transitionDuration = transitionDistance / 20.0; // approx train speed 72 km/h -> 20 m/s

            steps.Add(new Step
            {
                Instruction = $"🚄 Transition inter-cités (train/bus recommandé, {FormatDistance(transitionDistance)})",
                Distance = transitionDistance,
                Duration = transitionDuration,
                Type = "transit",
                Waypoints = new List<Position> { transitionStart, transitionEnd }
            });
            totalDistance += transitionDistance;
            totalDuration += transitionDuration;

            // Partie 3: dans la ville de destination
            Console.WriteLine($"\n📍 PARTIE 3: Déplacements dans {destCity}");
            var destStation = await _jcdProxy.GetClosestStation(dest, minBikes > 0 ? minBikes : 1);
            if (destStation != null)
            {
                var walkToDestStation = await _routeService.GetWalkingRoute(transitionEnd, destStation.position);
                steps.Add(new Step
                {
                    Instruction = $"🚶 Marcher jusqu'à '{destStation.name}' ({FormatDistance(walkToDestStation.Distance)})",
                    Distance = walkToDestStation.Distance,
                    Duration = walkToDestStation.Duration,
                    Type = "walk",
                    Waypoints = walkToDestStation.Waypoints
                });
                totalDistance += walkToDestStation.Distance;
                totalDuration += walkToDestStation.Duration;

                steps.Add(new Step
                {
                    Instruction = $"🚲 Prendre un vélo à '{destStation.name}'",
                    Distance = 0,
                    Duration = 30,
                    Type = "bike",
                    Waypoints = new List<Position> { destStation.position }
                });
                totalDuration += 30;

                var finalBikeRoute = await _routeService.GetCyclingRoute(destStation.position, dest);
                steps.Add(new Step
                {
                    Instruction = $"🚴 Rouler jusqu'à la destination ({FormatDistance(finalBikeRoute.Distance)})",
                    Distance = finalBikeRoute.Distance,
                    Duration = finalBikeRoute.Duration,
                    Type = "bike",
                    Waypoints = finalBikeRoute.Waypoints
                });
                totalDistance += finalBikeRoute.Distance;
                totalDuration += finalBikeRoute.Duration;

                var finalStation = await _jcdProxy.GetClosestStation(dest, 0);
                if (finalStation != null)
                {
                    steps.Add(new Step
                    {
                        Instruction = $"🅿️ Déposer le vélo à '{finalStation.name}'",
                        Distance = 0,
                        Duration = 30,
                        Type = "bike",
                        Waypoints = new List<Position> { finalStation.position }
                    });
                    totalDuration += 30;
                }

                var finalWalk = await _routeService.GetWalkingRoute(finalStation?.position ?? destStation.position, dest);
                steps.Add(new Step
                {
                    Instruction = $"🚶 Marcher jusqu'à la destination ({FormatDistance(finalWalk.Distance)})",
                    Distance = finalWalk.Distance,
                    Duration = finalWalk.Duration,
                    Type = "walk",
                    Waypoints = finalWalk.Waypoints
                });
                totalDistance += finalWalk.Distance;
                totalDuration += finalWalk.Duration;
            }
            else
            {
                Console.WriteLine("   ⚠️ Aucune station disponible dans la ville de destination → marche/transit final");
                var finalWalk = await _routeService.GetWalkingRoute(transitionEnd, dest);
                steps.Add(new Step
                {
                    Instruction = $"🚶 Marcher jusqu'à la destination ({FormatDistance(finalWalk.Distance)})",
                    Distance = finalWalk.Distance,
                    Duration = finalWalk.Duration,
                    Type = "walk",
                    Waypoints = finalWalk.Waypoints
                });
                totalDistance += finalWalk.Distance;
                totalDuration += finalWalk.Duration;
            }

            Console.WriteLine($"\n   📊 TOTAL MULTI-CONTRATS: {FormatDistance(totalDistance)}, {(totalDuration / 60):F1} min");

            return new ItineraryResponse
            {
                Instructions = $"✅ Itinéraire multi-villes calculé ! {originCity} → {destCity}",
                TotalDistance = totalDistance,
                TotalDuration = totalDuration,
                Steps = steps
            };
        }
        #endregion

        #region Helpers used across methods
        private async Task<Station> FindBestStationTowardsDestination(Position origin, Position finalDest, string city, int minBikes)
        {
            Console.WriteLine($"   🔎 Recherche stations dans {city} pour s'approcher de la destination...");
            try
            {
                var allStations = await _jcdProxy.GetStationsAsync(city);
                if (allStations == null || allStations.Count == 0) return null;

                var availableStations = allStations
                    .Where(s => s.available_bikes >= (minBikes > 0 ? minBikes : 1) && string.Equals(s.status, "OPEN", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (!availableStations.Any()) return null;

                var candidates = availableStations
                    .Select(station => new
                    {
                        Station = station,
                        WalkDistance = _routeService.CalculateDistanceMeters(origin, station.position),
                        ProgressTowardsDest = CalculateProgress(origin, station.position, finalDest)
                    })
                    .Where(x => x.WalkDistance <= MAX_WALK_TO_STATION_METERS)
                    .OrderByDescending(x => x.ProgressTowardsDest)
                    .ThenBy(x => x.WalkDistance)
                    .ToList();

                return candidates.FirstOrDefault()?.Station;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ❌ Erreur FindBestStationTowardsDestination: {ex.Message}");
                return null;
            }
        }

        private double CalculateProgress(Position start, Position current, Position end)
        {
            var totalDistance = _routeService.CalculateDistanceMeters(start, end);
            if (totalDistance <= 0) return 0;
            var progress = _routeService.CalculateDistanceMeters(start, current);
            return progress / totalDistance;
        }

        private async Task<Position> FindCityExitPoint(Position current, Position destination, string city)
        {
            var directionLat = (destination.lat - current.lat) * 0.3;
            var directionLng = (destination.lng - current.lng) * 0.3;

            return new Position
            {
                lat = current.lat + directionLat,
                lng = current.lng + directionLng
            };
        }
        #endregion

        #region Existing itinerary builders (walking / single-bike) & utils
        private async Task<ItineraryResponse> CreateWalkingOnlyItinerary(Position origin, Position dest, string reason)
        {
            var route = await _routeService.GetWalkingRoute(origin, dest);

            Console.WriteLine($"\n✅ ITINÉRAIRE MARCHE SEULE:");
            Console.WriteLine($"   Raison: {reason}");
            Console.WriteLine($"   Distance: {FormatDistance(route.Distance)}");
            Console.WriteLine($"   Durée: {(route.Duration / 60):F1} min");

            return new ItineraryResponse
            {
                Instructions = $"✅ Itinéraire à pied uniquement ({reason})",
                TotalDistance = route.Distance,
                TotalDuration = route.Duration,
                Steps = new List<Step>
                {
                    new Step
                    {
                        Instruction = $"Marcher jusqu'à la destination ({FormatDistance(route.Distance)})",
                        Distance = route.Distance,
                        Duration = route.Duration,
                        Type = "walk",
                        Waypoints = route.Waypoints ?? new List<Position> { origin, dest }
                    }
                }
            };
        }

        private async Task<ItineraryResponse> CreateBikeItinerary(
            Position origin, Position dest,
            Station originStation, Station destStation,
            RouteSegment walkToStation, RouteSegment bikeRoute, RouteSegment walkFromStation)
        {
            var steps = new List<Step>();
            double totalDistance = 0;
            double totalDuration = 0;

            Console.WriteLine($"\n✅ ITINÉRAIRE AVEC VÉLO (simple):");

            // Walk to start station
            steps.Add(new Step
            {
                Instruction = $"🚶 Marcher jusqu'à la station '{originStation.name}' ({FormatDistance(walkToStation.Distance)})",
                Distance = walkToStation.Distance,
                Duration = walkToStation.Duration,
                Type = "walk",
                Waypoints = walkToStation.Waypoints ?? new List<Position> { origin, originStation.position }
            });
            totalDistance += walkToStation.Distance;
            totalDuration += walkToStation.Duration;

            // Take bike
            steps.Add(new Step
            {
                Instruction = $"🚲 Prendre un vélo à '{originStation.name}' ({originStation.available_bikes} disponibles)",
                Distance = 0,
                Duration = 30,
                Type = "bike",
                Waypoints = new List<Position> { originStation.position }
            });
            totalDuration += 30;

            // Bike
            steps.Add(new Step
            {
                Instruction = $"🚴 Rouler jusqu'à la station '{destStation.name}' ({FormatDistance(bikeRoute.Distance)})",
                Distance = bikeRoute.Distance,
                Duration = bikeRoute.Duration,
                Type = "bike",
                Waypoints = bikeRoute.Waypoints ?? new List<Position> { originStation.position, destStation.position }
            });
            totalDistance += bikeRoute.Distance;
            totalDuration += bikeRoute.Duration;

            // Drop bike
            steps.Add(new Step
            {
                Instruction = $"🅿️ Déposer le vélo à '{destStation.name}' ({destStation.available_bike_stands} places libres)",
                Distance = 0,
                Duration = 30,
                Type = "bike",
                Waypoints = new List<Position> { destStation.position }
            });
            totalDuration += 30;

            // Final walk
            steps.Add(new Step
            {
                Instruction = $"🚶 Marcher jusqu'à la destination ({FormatDistance(walkFromStation.Distance)})",
                Distance = walkFromStation.Distance,
                Duration = walkFromStation.Duration,
                Type = "walk",
                Waypoints = walkFromStation.Waypoints ?? new List<Position> { destStation.position, dest }
            });
            totalDistance += walkFromStation.Distance;
            totalDuration += walkFromStation.Duration;

            Console.WriteLine($"\n   📊 TOTAL (simple bike): {FormatDistance(totalDistance)}, {(totalDuration / 60):F1} min");

            return new ItineraryResponse
            {
                Instructions = $"✅ Itinéraire avec vélo calculé ! Distance: {FormatDistance(totalDistance)} - Durée: {(totalDuration / 60):F0} min",
                TotalDistance = totalDistance,
                TotalDuration = totalDuration,
                Steps = steps
            };
        }

        private ItineraryResponse CreateErrorResponse(string message)
        {
            return new ItineraryResponse
            {
                Instructions = message,
                TotalDistance = 0,
                TotalDuration = 0,
                Steps = new List<Step>()
            };
        }

        private string FormatDistance(double meters)
        {
            if (double.IsInfinity(meters) || double.IsNaN(meters)) return "inconnue";
            if (meters < 1000) return $"{meters:F0} m";
            var km = meters / 1000.0;
            if (km < 10) return $"{km:F2} km";
            if (km < 100) return $"{km:F1} km";
            return $"{km:F0} km";
        }
        #endregion

        #region Small types for multi-bike chain
        private class MultiBikeChain
        {
            public List<BikeLeg> Segments { get; set; } = new List<BikeLeg>();
        }

        private class BikeLeg
        {
            public Station StartStation { get; set; }
            public Station EndStation { get; set; }
        }
        #endregion
    }
}
