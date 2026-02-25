using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kats
{
    public class Stop
    {
        public string? StopID { get; set; }  // Unique identifier for the stop
        public string? Area { get; set; }     // Area associated with the stop
        public Coordinate? Coordinates { get; set; } // Coordinates of the stop
        public int Order { get; set; }
    }


}

