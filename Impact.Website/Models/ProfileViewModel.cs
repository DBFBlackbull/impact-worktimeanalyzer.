using System;
using System.ComponentModel;
using Impact.Core.Model;

namespace Impact.Website.Models
{
    public class ProfileViewModel
    {
        [DisplayName("Fornavn(e)")]
        public string FirstName { get; set; }
        [DisplayName("Efternavn")]
        public string LastName { get; set; }
        [DisplayName("Fulde navn")]
        public string FullName { get; set; }
        [DisplayName("Initialer")]
        public string Initials { get; set; }
        [DisplayName("Title")]
        public string Title { get; set; }
        [DisplayName("Afdeling")]
        public string Department { get; set; }
        [DisplayName("Ansættelsesdato")]
        public DateTime HireDate { get; set; }
        [DisplayName("Standard timepris til kunder")]
        public double HourlyRate { get; set; }
        [DisplayName("Intern kostpris")]
        public double CostPrice { get; set; }
        [DisplayName("Er udvikler")]
        public bool IsDeveloper { get; set; }

        public ProfileViewModel(Profile profile)
        {
            FirstName = profile.FirstName;
            LastName = profile.LastName;
            FullName = profile.FullName;
            Initials = profile.Initials;
            Title = profile.Title;
            Department = profile.Department;
            HireDate = profile.HiredDate;
            HourlyRate = profile.HourlyRate;
            CostPrice = profile.CostPrice;
            IsDeveloper = profile.IsDeveloper;
        }
    }
}