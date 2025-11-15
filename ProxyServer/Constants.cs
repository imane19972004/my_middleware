namespace ProxyServer
{
    public static class Constants
    {
        public const string BaseAddress = "http://localhost:8001/ProxyServer";
        public const string JcdBaseAddress = "https://api.jcdecaux.com/vls/v1/stations";
        public const string EnvJcdecauxApiKey = "JCDECAUX_API_KEY"; //L'API key JCDecaux sera lue depuis une variable d'environnement
    }
}

//Centraliser toutes les constane du projet
//Pouvoir changer l'adresse du serveur ou l'API key sans le reste su code
