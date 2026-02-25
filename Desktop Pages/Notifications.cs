using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdminApp
{
    public class Notifications
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? TimeStamp { get; set; }
        public bool? IsVisibleDriver { get; set; }
        public bool? IsVisibleParent { get; set; }
    }
}
