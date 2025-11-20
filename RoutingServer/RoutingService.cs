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

        public RoutingService()
        {
            _routeService = new OpenRouteService();
            _jcdProxy = new JCDecauxProxy();
            Console.WriteLine("✅ RoutingService initialisé avec nouvelle logique de temps");
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
            {
                return CreateErrorResponse("❌ Origine ou destination manquante");
            }

            try
            {
                // 1️⃣ Géocoder origine et destination
                Console.WriteLine("\n📍 ÉTAPE 1: Géocodage des adresses");
                var originPos = await _routeService.GeocodeAddress(request.Origin);
                var destPos = await _routeService.GeocodeAddress(request.Destination);

                if (originPos == null || destPos == null)
                {
                    return CreateErrorResponse("❌ Impossible de localiser l'origine ou la destination");
                }

                // 2️⃣ Calculer distance directe
                var directDistance = _routeService.CalculateDistanceMeters(originPos, destPos);
                Console.WriteLine($"\n📏 ÉTAPE 2: Distance directe = {(directDistance / 1000):F2} km");

                // 3️⃣ Vérifier distance minimale absolue (200m)
                if (directDistance < 200)
                {
                    Console.WriteLine($"   ⚠️ Distance trop courte ({directDistance:F0}m < 200m) → MARCHE UNIQUEMENT");
                    return await CreateWalkingOnlyItinerary(originPos, destPos, "distance trop courte");
                }

                // 4️⃣ Chercher des stations vélo
                Console.WriteLine($"\n🚲 ÉTAPE 3: Recherche de stations JCDecaux");
                var originStation = await _jcdProxy.GetClosestStation(originPos, request.MinBikes > 0 ? request.MinBikes : 1);
                var destStation = await _jcdProxy.GetClosestStation(destPos, 1);

                if (originStation == null || destStation == null)
                {
                    Console.WriteLine("   ⚠️ Aucune station disponible → MARCHE UNIQUEMENT");
                    return await CreateWalkingOnlyItinerary(originPos, destPos, "pas de stations disponibles");
                }

                Console.WriteLine($"   ✅ Station départ: {originStation.name} ({originStation.available_bikes} vélos)");
                Console.WriteLine($"   ✅ Station arrivée: {destStation.name} ({destStation.available_bike_stands} places)");

                // 5️⃣ NOUVELLE LOGIQUE : Comparer le TEMPS total
                Console.WriteLine($"\n⏱️ ÉTAPE 4: Comparaison des temps de trajet");

                // Calculer temps MARCHE SEULE
                Console.WriteLine("   🚶 Calcul temps marche seule...");
                var walkOnlyRoute = await _routeService.GetWalkingRoute(originPos, destPos);
                var walkOnlyTime = walkOnlyRoute.Duration; // secondes

                // Calculer temps AVEC VÉLO
                Console.WriteLine("   🚴 Calcul temps avec vélo...");
                var walkToStationRoute = await _routeService.GetWalkingRoute(originPos, originStation.position);
                var bikeRoute = await _routeService.GetCyclingRoute(originStation.position, destStation.position);
                var walkFromStationRoute = await _routeService.GetWalkingRoute(destStation.position, destPos);

                var bikeTime = walkToStationRoute.Duration + 30 + bikeRoute.Duration + 30 + walkFromStationRoute.Duration;
                // 30 secondes pour prendre/déposer le vélo

                var timeSaved = walkOnlyTime - bikeTime;

                Console.WriteLine($"\n   📊 RÉSULTATS:");
                Console.WriteLine($"      Marche seule: {(walkOnlyTime / 60):F1} min");
                Console.WriteLine($"      Avec vélo:    {(bikeTime / 60):F1} min");
                Console.WriteLine($"      Gain:         {(timeSaved / 60):F1} min");

                // 6️⃣ Décision : utiliser le vélo SI gain de temps >= 2 minutes
                const int MIN_TIME_SAVING = 120; // 2 minutes en secondes

                if (timeSaved >= MIN_TIME_SAVING)
                {
                    Console.WriteLine($"\n   ✅ DÉCISION: VÉLO (gain de {(timeSaved / 60):F1} min >= 2 min)");
                    return await CreateBikeItinerary(originPos, destPos, originStation, destStation,
                                                     walkToStationRoute, bikeRoute, walkFromStationRoute);
                }
                else
                {
                    Console.WriteLine($"\n   ⚠️ DÉCISION: MARCHE SEULE (gain de {(timeSaved / 60):F1} min < 2 min)");
                    return await CreateWalkingOnlyItinerary(originPos, destPos, $"gain de temps insuffisant ({(timeSaved / 60):F1} min)");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ ERREUR FATALE: {ex.Message}");
                Console.WriteLine($"   Stack: {ex.StackTrace}");
                return CreateErrorResponse($"❌ Erreur lors du calcul: {ex.Message}");
            }
        }

        private async Task<ItineraryResponse> CreateWalkingOnlyItinerary(Position origin, Position dest, string reason)
        {
            var route = await _routeService.GetWalkingRoute(origin, dest);

            Console.WriteLine($"\n✅ ITINÉRAIRE MARCHE SEULE:");
            Console.WriteLine($"   Raison: {reason}");
            Console.WriteLine($"   Distance: {(route.Distance / 1000):F2} km");
            Console.WriteLine($"   Durée: {(route.Duration / 60):F1} min");
            Console.WriteLine($"   Waypoints: {route.Waypoints?.Count ?? 0}");

            return new ItineraryResponse
            {
                Instructions = $"✅ Itinéraire à pied uniquement ({reason})",
                TotalDistance = route.Distance,
                TotalDuration = route.Duration,
                Steps = new List<Step>
                {
                    new Step
                    {
                        Instruction = $"Marcher jusqu'à la destination ({(route.Distance/1000):F2} km)",
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

            Console.WriteLine($"\n✅ ITINÉRAIRE AVEC VÉLO:");

            // Étape 1: Marcher jusqu'à la station de départ
            steps.Add(new Step
            {
                Instruction = $"🚶 Marcher jusqu'à la station '{originStation.name}' ({(walkToStation.Distance / 1000):F2} km)",
                Distance = walkToStation.Distance,
                Duration = walkToStation.Duration,
                Type = "walk",
                Waypoints = walkToStation.Waypoints ?? new List<Position> { origin, originStation.position }
            });
            totalDistance += walkToStation.Distance;
            totalDuration += walkToStation.Duration;
            Console.WriteLine($"   1. Marche vers station: {(walkToStation.Distance / 1000):F2} km, {(walkToStation.Duration / 60):F1} min, {walkToStation.Waypoints?.Count ?? 0} pts");

            // Étape 2: Prendre un vélo
            steps.Add(new Step
            {
                Instruction = $"🚲 Prendre un vélo à '{originStation.name}' ({originStation.available_bikes} disponibles)",
                Distance = 0,
                Duration = 30,
                Type = "bike",
                Waypoints = new List<Position> { originStation.position }
            });
            totalDuration += 30;
            Console.WriteLine($"   2. Prendre vélo: 30s");

            // Étape 3: Faire du vélo
            steps.Add(new Step
            {
                Instruction = $"🚴 Rouler jusqu'à la station '{destStation.name}' ({(bikeRoute.Distance / 1000):F2} km)",
                Distance = bikeRoute.Distance,
                Duration = bikeRoute.Duration,
                Type = "bike",
                Waypoints = bikeRoute.Waypoints ?? new List<Position> { originStation.position, destStation.position }
            });
            totalDistance += bikeRoute.Distance;
            totalDuration += bikeRoute.Duration;
            Console.WriteLine($"   3. Vélo: {(bikeRoute.Distance / 1000):F2} km, {(bikeRoute.Duration / 60):F1} min, {bikeRoute.Waypoints?.Count ?? 0} pts");

            // Étape 4: Déposer le vélo
            steps.Add(new Step
            {
                Instruction = $"🅿️ Déposer le vélo à '{destStation.name}' ({destStation.available_bike_stands} places libres)",
                Distance = 0,
                Duration = 30,
                Type = "bike",
                Waypoints = new List<Position> { destStation.position }
            });
            totalDuration += 30;
            Console.WriteLine($"   4. Déposer vélo: 30s");

            // Étape 5: Marcher jusqu'à la destination
            steps.Add(new Step
            {
                Instruction = $"🚶 Marcher jusqu'à la destination ({(walkFromStation.Distance / 1000):F2} km)",
                Distance = walkFromStation.Distance,
                Duration = walkFromStation.Duration,
                Type = "walk",
                Waypoints = walkFromStation.Waypoints ?? new List<Position> { destStation.position, dest }
            });
            totalDistance += walkFromStation.Distance;
            totalDuration += walkFromStation.Duration;
            Console.WriteLine($"   5. Marche finale: {(walkFromStation.Distance / 1000):F2} km, {(walkFromStation.Duration / 60):F1} min, {walkFromStation.Waypoints?.Count ?? 0} pts");

            Console.WriteLine($"\n   📊 TOTAL: {(totalDistance / 1000):F2} km, {(totalDuration / 60):F1} min");

            return new ItineraryResponse
            {
                Instructions = $"✅ Itinéraire avec vélo calculé ! Distance: {(totalDistance / 1000):F2} km - Durée: {(totalDuration / 60):F0} min",
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
    }
}