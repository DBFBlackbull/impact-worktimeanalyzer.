using System;

namespace Impact.Core.Model
{
    public class Profile
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string Initials { get; set; }
        public string Title { get; set; }
        public string Department { get; set; }
        public DateTime HireDate { get; set; }
        public double CostPrice { get; set; }
        public double HourlyRate { get; set; }
        public bool IsDeveloper { get; set; }
    }
}