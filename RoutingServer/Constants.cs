namespace RoutingServer
{
    public static class Constants
    {
        // Adresse de votre RoutingServer
        public const string BaseAddress = "http://localhost:8003/RoutingServer";

        // Adresse du ProxyServer (pour récupérer les stations JCDecaux)
        public const string ProxyServiceUrl = "http://localhost:8001/ProxyServer";

        // Seuils de décision
        public const double MinDistanceForBike = 500; // mètres - en dessous, marche uniquement
        public const double MaxDetourRatio = 0.5; // 50% - si détour > 50% de distance directe, marche uniquement
    }
}