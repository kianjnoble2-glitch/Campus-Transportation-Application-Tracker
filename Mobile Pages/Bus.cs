using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kats
{
    public class Bus
    {
        public string? BusNumber { get; set; }
        //public string? Image { get; set; }
        public required Routes? Route { get; set; }  // Updated reference to Route
        //public required Driver DriverAssigned { get; set; }
    }
}
