using System.Collections.Generic;
using System.Runtime.Serialization;

namespace RoutingServer
{
    [DataContract]
    public class ItineraryRequest
    {
        [DataMember]
        public string Origin { get; set; }

        [DataMember]
        public string Destination { get; set; }

        [DataMember]
        public int MinBikes { get; set; }
    }

    [DataContract]
    public class ItineraryResponse
    {
        [DataMember]
        public string Instructions { get; set; }

        [DataMember]
        public double TotalDistance { get; set; } // en mètres

        [DataMember]
        public double TotalDuration { get; set; } // en secondes

        [DataMember]
        public List<Step> Steps { get; set; }
    }

    [DataContract]
    public class Step
    {
        [DataMember]
        public string Instruction { get; set; }

        [DataMember]
        public double Distance { get; set; }

        [DataMember]
        public string Type { get; set; } // "walk" ou "bike"
    }

    [DataContract]
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

    [DataContract]
    public class Position
    {
        [DataMember]
        public double lat { get; set; }

        [DataMember]
        public double lng { get; set; }
    }
}