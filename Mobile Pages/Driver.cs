using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kats
{
    public class Driver
    {
        //public string? DriverID { get; set; }
        public string? Name { get; set; }
        //public string? ParentType { get; set; }
        public bool IsFemale { get; set; }
        public bool IsMale { get; set; }
        //public bool IsGurdian { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? Image {  get; set; }
        public required Routes? Route { get; set; }  // Updated reference to Route
        public required Users user { get; set; }
        //public List<Bus> BusAssigned { get; set; } = new List<Bus>();
        public required Bus? bus { get; set; }
    }
}
