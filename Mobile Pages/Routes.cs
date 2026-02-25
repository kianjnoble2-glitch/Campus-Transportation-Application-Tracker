using GoogleApi.Entities.Maps.Directions.Response;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kats
{
    public class Routes
    {
        public string? RouteID { get; set; }  // Unique identifier for the route
        public string? RouteName { get; set; } // Name of the route
        
        // Changed Stops to a Dictionary to match the JSON structure
        public Dictionary<string, Stop> Stops { get; set; } = new Dictionary<string, Stop>();
        
        // Updated AreaNames to accommodate the new dictionary structure
        public List<string?> AreaNames => Stops.Values.Select(s => s.Area).Distinct().ToList();
    }

    public class Stop
    {
        public string? Key { get; set; }
        public string? Area { get; set; }
        public Coordinates? Coordinates { get; set; }
    }

    public class Coordinates
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}