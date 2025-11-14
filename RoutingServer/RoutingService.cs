using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;

namespace RoutingServer
{
    /// <summary>
    /// Service principal de calcul d'itinéraires vélo
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class RoutingService : IRoutingService
    {
        private readonly OpenRouteService _routeService;
        private readonly JCDecauxProxy _jcdProxy;

        public RoutingService()
        {
            _routeService = new OpenRouteService();
            _jcdProxy = new JCDecauxProxy();
            Console.WriteLine("✅ RoutingService initialisé");
        }

        public async Task<ItineraryResponse> GetItinerary(ItineraryRequest request)
        {
            Console.WriteLine($"\n🚴 NOUVELLE REQUÊTE: {request.Origin} → {request.Destination}");

            try
            {
                // 1️⃣ Géocoder origine et destination
                var originPos = await _routeService.GeocodeAddress(request.Origin);
                var destPos = await _routeService.GeocodeAddress(request.Destination);

                if (originPos == null || destPos == null)
                {
                    return CreateErrorResponse("❌ Impossible de localiser l'origine ou la destination");
                }

                // 2️⃣ Calculer distance directe
                var directDistance = _routeService.CalculateDistanceMeters(originPos, destPos);
                Console.WriteLine($"   📏 Distance directe: {(directDistance / 1000):F2} km");

                // 3️⃣ Vérifier si le vélo est pertinent
                if (directDistance < Constants.MinDistanceForBike)
                {
                    Console.WriteLine($"   ⚠️ Distance trop courte ({directDistance:F0}m < {Constants.MinDistanceForBike}m)");
                    return await CreateWalkingOnlyItinerary(originPos, destPos);
                }

                // 4️⃣ Chercher des stations vélo
                var originStation = await _jcdProxy.GetClosestStation(originPos, request.MinBikes);
                var destStation = await _jcdProxy.GetClosestStation(destPos, 1); // Au moins 1 place libre

                if (originStation == null || destStation == null)
                {
                    Console.WriteLine("   ⚠️ Pas de stations disponibles → Mode piéton");
                    return await CreateWalkingOnlyItinerary(originPos, destPos);
                }

                // 5️⃣ Vérifier si le détour en vaut la peine
                var walkToOriginStation = _routeService.CalculateDistanceMeters(originPos, originStation.position);
                var walkFromDestStation = _routeService.CalculateDistanceMeters(destStation.position, destPos);
                var totalDetour = walkToOriginStation + walkFromDestStation;

                if (totalDetour > directDistance * Constants.MaxDetourRatio)
                {
                    Console.WriteLine($"   ⚠️ Détour trop grand ({totalDetour:F0}m > {directDistance * Constants.MaxDetourRatio:F0}m)");
                    return await CreateWalkingOnlyItinerary(originPos, destPos);
                }

                // 6️⃣ Créer itinéraire avec vélo
                Console.WriteLine("   ✅ Itinéraire vélo calculé !");
                return await CreateBikeItinerary(originPos, destPos, originStation, destStation);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ❌ ERREUR: {ex.Message}");
                return CreateErrorResponse($"Erreur lors du calcul: {ex.Message}");
            }
        }

        /// <summary>
        /// Crée un itinéraire 100% piéton
        /// </summary>
        private async Task<ItineraryResponse> CreateWalkingOnlyItinerary(Position origin, Position dest)
        {
            var route = await _routeService.GetWalkingRoute(origin, dest);

            return new ItineraryResponse
            {
                Instructions = "✅ Itinéraire à pied uniquement (pas de vélo disponible ou non rentable)",
                TotalDistance = route.Distance,
                TotalDuration = route.Duration,
                Steps = new List<Step>
                {
                    new Step
                    {
                        Instruction = $"Marcher jusqu'à la destination ({(route.Distance/1000):F2} km)",
                        Distance = route.Distance,
                        Type = "walk"
                    }
                }
            };
        }

        /// <summary>
        /// Crée un itinéraire combinant marche + vélo + marche
        /// </summary>
        private async Task<ItineraryResponse> CreateBikeItinerary(
            Position origin, Position dest, Station originStation, Station destStation)
        {
            var steps = new List<Step>();
            double totalDistance = 0;
            double totalDuration = 0;

            // Étape 1: Marcher jusqu'à la station de départ
            var walkToStation = await _routeService.GetWalkingRoute(origin, originStation.position);
            steps.Add(new Step
            {
                Instruction = $"🚶 Marcher jusqu'à la station '{originStation.name}' ({walkToStation.Distance:F0}m)",
                Distance = walkToStation.Distance,
                Type = "walk"
            });
            totalDistance += walkToStation.Distance;
            totalDuration += walkToStation.Duration;

            // Étape 2: Prendre un vélo
            steps.Add(new Step
            {
                Instruction = $"🚲 Prendre un vélo à '{originStation.name}' ({originStation.available_bikes} disponibles)",
                Distance = 0,
                Type = "bike"
            });

            // Étape 3: Faire du vélo jusqu'à la station d'arrivée
            var bikeRoute = await _routeService.GetCyclingRoute(originStation.position, destStation.position);
            steps.Add(new Step
            {
                Instruction = $"🚴 Rouler jusqu'à la station '{destStation.name}' ({(bikeRoute.Distance / 1000):F2} km)",
                Distance = bikeRoute.Distance,
                Type = "bike"
            });
            totalDistance += bikeRoute.Distance;
            totalDuration += bikeRoute.Duration;

            // Étape 4: Déposer le vélo
            steps.Add(new Step
            {
                Instruction = $"🅿️ Déposer le vélo à '{destStation.name}' ({destStation.available_bike_stands} places libres)",
                Distance = 0,
                Type = "bike"
            });

            // Étape 5: Marcher jusqu'à la destination finale
            var walkFromStation = await _routeService.GetWalkingRoute(destStation.position, dest);
            steps.Add(new Step
            {
                Instruction = $"🚶 Marcher jusqu'à la destination ({walkFromStation.Distance:F0}m)",
                Distance = walkFromStation.Distance,
                Type = "walk"
            });
            totalDistance += walkFromStation.Distance;
            totalDuration += walkFromStation.Duration;

            return new ItineraryResponse
            {
                Instructions = $"✅ Itinéraire avec vélo calculé ! Distance: {(totalDistance / 1000):F2} km - Durée: {(totalDuration / 60):F0} min",
                TotalDistance = totalDistance,
                TotalDuration = totalDuration,
                Steps = steps
            };
        }

        /// <summary>
        /// Crée une réponse d'erreur
        /// </summary>
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