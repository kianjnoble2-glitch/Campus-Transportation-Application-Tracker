using Kats.Models;

namespace Kats
{
    public class Parent
    {
        public string? ParentID { get; set; }
        public string? Name { get; set; }
        public string? ParentType { get; set; }
        public bool IsMother { get; set; }
        public bool IsFather { get; set; }
        public bool IsGurdian { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public LocationData? Location { get; set; }
        public Routes Route { get; set; } = new Routes(); // Default initialization  // Updated reference to Route
        public required Users user { get; set; }
        public List<Children> Children { get; set; } = new List<Children>();
    }

    public class LocationData
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
