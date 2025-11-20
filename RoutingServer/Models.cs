//using System.Collections.Generic;
//using System.Runtime.Serialization;

//namespace RoutingServer
//{
//    [DataContract(Namespace = "http://tempuri.org/")]
//    public class ItineraryRequest
//    {
//        [DataMember(Order = 1, IsRequired = true)]
//        public string Origin { get; set; }

//        [DataMember(Order = 2, IsRequired = true)]
//        public string Destination { get; set; }

//        [DataMember(Order = 3, IsRequired = false)]
//        public int MinBikes { get; set; }
//    }

//    [DataContract(Namespace = "http://tempuri.org/")]
//    public class ItineraryResponse
//    {
//        [DataMember]
//        public string Instructions { get; set; }

//        [DataMember]
//        public double TotalDistance { get; set; }

//        [DataMember]
//        public double TotalDuration { get; set; }

//        [DataMember]
//        public List<Step> Steps { get; set; }
//    }

//    [DataContract(Namespace = "http://tempuri.org/")]
//    public class Step
//    {
//        [DataMember]
//        public string Instruction { get; set; }

//        [DataMember]
//        public double Distance { get; set; }

//        [DataMember]
//        public string Type { get; set; }

//        // ✅ AJOUT DES COORDONNÉES GPS
//        [DataMember]
//        public Position StartPosition { get; set; }

//        [DataMember]
//        public Position EndPosition { get; set; }
//    }

//    [DataContract(Namespace = "http://tempuri.org/")]
//    public class Station
//    {
//        [DataMember]
//        public int number { get; set; }

//        [DataMember]
//        public string name { get; set; }

//        [DataMember]
//        public string address { get; set; }

//        [DataMember]
//        public Position position { get; set; }

//        [DataMember]
//        public int available_bikes { get; set; }

//        [DataMember]
//        public int available_bike_stands { get; set; }

//        [DataMember]
//        public string status { get; set; }
//    }

//    [DataContract(Namespace = "http://tempuri.org/")]
//    public class Position
//    {
//        [DataMember]
//        public double lat { get; set; }

//        [DataMember]
//        public double lng { get; set; }
//    }
//}

//using System.Collections.Generic;
//using System.Runtime.Serialization;

//namespace RoutingServer
//{
//    [DataContract(Namespace = "http://tempuri.org/")]
//    public class ItineraryRequest
//    {
//        [DataMember(Order = 1, IsRequired = true)]
//        public string Origin { get; set; }

//        [DataMember(Order = 2, IsRequired = true)]
//        public string Destination { get; set; }

//        [DataMember(Order = 3, IsRequired = false)]
//        public int MinBikes { get; set; }
//    }

//    [DataContract(Namespace = "http://tempuri.org/")]
//    public class ItineraryResponse
//    {
//        [DataMember]
//        public string Instructions { get; set; }

//        [DataMember]
//        public double TotalDistance { get; set; }

//        [DataMember]
//        public double TotalDuration { get; set; }

//        [DataMember]
//        public List<Step> Steps { get; set; }
//    }

//    [DataContract(Namespace = "http://tempuri.org/")]
//    public class Step
//    {
//        [DataMember]
//        public string Instruction { get; set; }

//        [DataMember]
//        public double Distance { get; set; }

//        [DataMember]
//        public double Duration { get; set; }

//        [DataMember]
//        public string Type { get; set; }

//        // ✅ NOUVEAU : Liste des points du tracé (vraies routes)
//        [DataMember]
//        public List<Position> Waypoints { get; set; }

//        // ⚠️ DEPRECATED : Gardés pour compatibilité mais plus utilisés
//        [DataMember]
//        public Position StartPosition { get; set; }

//        [DataMember]
//        public Position EndPosition { get; set; }
//    }

//    [DataContract(Namespace = "http://tempuri.org/")]
//    public class Station
//    {
//        [DataMember]
//        public int number { get; set; }

//        [DataMember]
//        public string name { get; set; }

//        [DataMember]
//        public string address { get; set; }

//        [DataMember]
//        public Position position { get; set; }

//        [DataMember]
//        public int available_bikes { get; set; }

//        [DataMember]
//        public int available_bike_stands { get; set; }

//        [DataMember]
//        public string status { get; set; }
//    }

//    [DataContract(Namespace = "http://tempuri.org/")]
//    public class Position
//    {
//        [DataMember]
//        public double lat { get; set; }

//        [DataMember]
//        public double lng { get; set; }
//    }
//}


using System.Collections.Generic;
using System.Runtime.Serialization;

namespace RoutingServer
{
    [DataContract(Namespace = "http://tempuri.org/")]
    public class ItineraryRequest
    {
        [DataMember(Order = 1, IsRequired = true)]
        public string Origin { get; set; }

        [DataMember(Order = 2, IsRequired = true)]
        public string Destination { get; set; }

        [DataMember(Order = 3, IsRequired = false)]
        public int MinBikes { get; set; }
    }

    [DataContract(Namespace = "http://tempuri.org/")]
    public class ItineraryResponse
    {
        [DataMember]
        public string Instructions { get; set; }

        [DataMember]
        public double TotalDistance { get; set; }

        [DataMember]
        public double TotalDuration { get; set; }

        [DataMember]
        public List<Step> Steps { get; set; }
    }

    [DataContract(Namespace = "http://tempuri.org/")]
    public class Step
    {
        [DataMember]
        public string Instruction { get; set; }

        [DataMember]
        public double Distance { get; set; }

        [DataMember]
        public double Duration { get; set; }

        [DataMember]
        public string Type { get; set; }

        // ✅ NOUVEAU : Liste des points du tracé (vraies routes)
        [DataMember]
        public List<Position> Waypoints { get; set; }

        // ⚠️ DEPRECATED : Gardés pour compatibilité mais plus utilisés
        [DataMember]
        public Position StartPosition { get; set; }

        [DataMember]
        public Position EndPosition { get; set; }
    }

    [DataContract(Namespace = "http://tempuri.org/")]
    public class Station
    {
        [DataMember]
        public int number { get; set; }

        [DataMember]
        public string name { get; set; }

        [DataMember]
        public string address { get; set; }

        [DataMember]
        public Position position { get; set; }

        [DataMember]
        public int available_bikes { get; set; }

        [DataMember]
        public int available_bike_stands { get; set; }

        [DataMember]
        public string status { get; set; }
    }

    [DataContract(Namespace = "http://tempuri.org/")]
    public class Position
    {
        [DataMember]
        public double lat { get; set; }

        [DataMember]
        public double lng { get; set; }
    }
}