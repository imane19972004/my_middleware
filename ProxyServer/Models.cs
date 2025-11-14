using System.Runtime.Serialization;

namespace ProxyServer
{
    [DataContract]
    public class SimplifiedGeoCoordinate
    {
        [DataMember] public double Latitude { get; set; }
        [DataMember] public double Longitude { get; set; }

        public SimplifiedGeoCoordinate() { }

        public SimplifiedGeoCoordinate(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }
    }

    [DataContract]
    public class Station
    {
        public static string StatusOpen = "OPEN";
        public static string StatusClosed = "CLOSED";

        [DataMember] public int number { get; set; }
        [DataMember] public string contract_name { get; set; }
        [DataMember] public string name { get; set; }
        [DataMember] public string address { get; set; }  // ✅ AJOUTÉ
        [DataMember] public Position position { get; set; }
        [DataMember] public bool banking { get; set; }
        [DataMember] public bool bonus { get; set; }
        [DataMember] public int bike_stands { get; set; }
        [DataMember] public int available_bike_stands { get; set; }
        [DataMember] public int available_bikes { get; set; }
        [DataMember] public string status { get; set; }
        [DataMember] public long last_update { get; set; }
    }

    [DataContract]
    public class Position
    {
        [DataMember] public double lat { get; set; }
        [DataMember] public double lng { get; set; }
    }
}