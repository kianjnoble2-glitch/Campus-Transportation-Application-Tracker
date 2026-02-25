namespace Kats
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

    public class CheckIn
    {
        public Children? Child { get; set; }
        public bool? CheckedIn { get; set; }
        public string? TimeStamp { get; set; }
    }
}
