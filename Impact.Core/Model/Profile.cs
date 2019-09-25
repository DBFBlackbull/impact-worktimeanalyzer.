using System;
using Impact.Core.Extension;

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
        
        private decimal _normalWorkWeek;
        public decimal NormalWorkWeek => _normalWorkWeek > 0 
            ? _normalWorkWeek
            : _normalWorkWeek = NormalWorkDay * 5;

        private decimal _normalWorkMonth;
        // 52 weeks divided by 12 months
        public decimal NormalWorkMonth => _normalWorkMonth > 0 
            ? _normalWorkMonth
            : _normalWorkMonth = Math.Round(NormalWorkWeek * 52m / 12m, 2, MidpointRounding.AwayFromZero).Normalize();
    }
}