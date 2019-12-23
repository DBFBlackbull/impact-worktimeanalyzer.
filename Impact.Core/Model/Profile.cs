using System;

namespace Impact.Core.Model
{
    public class Profile
    {
        public string FirstName { get; }
        public string LastName { get; }
        public string FullName { get; }
        public string Initials { get; }
        public int EmployeeId { get; }
        public string Title { get; }
        public string Email { get; } // Reporting only
        public string DepartmentName { get; }
        public int DepartmentId { get; } // Reporting only
        public DateTime HiredDate { get; }
        public double CostPrice { get; }
//        public double HourlyRate { get; set; } // Transactional only
        public bool IsDeveloper { get; set; }
        public decimal NormalWorkDay { get; set; }
        public decimal NormalWorkWeek { get; set; }
        /// <summary>
        /// <p>Gets the average amount of hours in a month. The follow relations are validated by asking people</p>
        /// <p>37,5 -> 162,5</p>
        /// <p>37 -> 160,33</p>
        /// <p>35 -> 151,67</p>
        /// <p>34 -> 147,33</p>
        /// <p>33 -> 142,9</p>
        /// <p>30 -> 130</p>
        /// <p>19 -> 82,33</p>
        /// <p>OBS: 33 returns 143 even though people have reported it to be 142,9 on their paycheck</p>
        /// </summary>
        public decimal NormalWorkMonth { get; set; }

        public Profile(int employeeId, string firstName, string lastName, string fullName, string initials, string title, string email, string departmentName, int departmentId, DateTime hiredDate, double costPrice)
        {
            EmployeeId = employeeId;
            FirstName = firstName;
            LastName = lastName;
            FullName = fullName;
            Initials = initials;
            Title = title;
            Email = email;
            DepartmentName = departmentName;
            DepartmentId = departmentId;
            HiredDate = hiredDate;
            CostPrice = costPrice;
        }
    }
}