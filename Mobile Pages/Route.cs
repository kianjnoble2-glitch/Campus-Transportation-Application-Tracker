using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kats
{
    public class Route
    {
        public string? RouteName { get; set; } // The name of the route
        public List<Stop> Stops { get; set; } = new List<Stop>(); // The list of stops on this route
    }
}

