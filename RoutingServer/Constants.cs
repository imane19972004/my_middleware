namespace RoutingServer
{
    public static class Constants
    {
        public const string BaseAddress = "http://localhost:8000/RoutingServer";
        public const string ProxyServiceUrl = "http://localhost:8001/ProxyServer";

        // API de calcul d'itinéraire (OpenRouteService gratuit)
        public const string ORS_API_KEY = "VOTRE_CLE_ORS"; // Obtenez-la sur openrouteservice.org
        public const string ORS_BASE_URL = "https://api.openrouteservice.org/v2/directions/";
    }
}