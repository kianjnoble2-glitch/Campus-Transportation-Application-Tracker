using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdminApp
{
    public class Parent
    {
        //public string? ParentID { get; set; }
        public string? Name { get; set; }
        //public string? ParentType { get; set; }
        public bool IsMother { get; set; }
        public bool IsFather { get; set; }
        public bool IsGurdian { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? Image {  get; set; }
        public required Routes? Route { get; set; }  // Updated reference to Route
        public required Users user { get; set; }
        public List<Children> Children { get; set; } = new List<Children>();

    }
}
