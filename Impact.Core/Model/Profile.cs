using System;

namespace Impact.Core.Model
{
    public class Profile
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string Initials { get; set; }
        public int EmployeeId { get; set; }
        public string Title { get; set; }
        public string Email { get; set; } // Reporting only
        public string DepartmentName { get; set; }
        public int DepartmentId { get; set; } // Reporting only
        public DateTime HiredDate { get; set; }
        public double CostPrice { get; set; }
//        public double HourlyRate { get; set; } // Transactional only
        public bool IsDeveloper { get; set; }
        public decimal NormalWorkDay { get; set; }
        public decimal NormalWorkWeek { get; set; }
        /// <summary>
        /// <p>Gets the average amount of hours in a month. The follow relations are validated by asking people</p>
        /// <p>37,5 -> 162,5</p>
        /// <p>37 -> 160,33</p>
        /// <p>35 -> 151,67</p>
        /// <p>33 -> 142,9</p>
        /// <p>30 -> 130</p>
        /// <p>19 -> 82,33</p>
        /// <p>OBS: 33 -> 142,9 is being rounded to 143</p>
        /// </summary>
        public decimal NormalWorkMonth { get; set; }
    }
}