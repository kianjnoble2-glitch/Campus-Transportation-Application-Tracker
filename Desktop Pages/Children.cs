using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdminApp
{
    public class Children
    {
        public string? Name { get; set; }
        public string? SchoolName { get; set; } = "Safe S School";
        public bool? IsFemale { get; set; }
        public bool? IsMale { get; set; }
        public string? Grade { get; set; }
        public string? BusNumber { get; set; }
    }

    public class CheckInHistory
    {
        public Children? Child { get; set; }
        public bool? CheckedIn { get; set; }
        public string? TimeStamp { get; set; }
    }
}
