using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using Impact.Core.Extension;
using Impact.Core.Model;

namespace Impact.Website.Models
{
    public class JiraIdViewModel
    {
        [Required]
        [DisplayName("Jira ID")]
        [RegularExpression("^[A-Za-z]+-[0-9]+$", ErrorMessage = "Jira Id skal have formatet XXX-###")]
        public string JiraId { get; set; }

        [DisplayName("Fra")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime From { get; set; }
        
        [DisplayName("Til")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime To { get; set; }

        [DisplayName("Projekt")]
        public IEnumerable<SelectListItem> Projects { get; set; }
        public string SelectedProject { get; set; }

        public string Message { get; set; }
        public IEnumerable<TimeRegistration> TimeRegistrations { get; set; }
        public decimal SummedRawHours => TimeRegistrations.Sum(r => Math.Round(r.RawHours, 2, MidpointRounding.AwayFromZero)).Normalize(); 
        public string SummedDisplayHours
        {
            get
            {
                var hours = Math.Truncate(SummedRawHours);
                var minutes = (SummedRawHours - hours) * 60m;
                return $"{hours:00}:{minutes:00}";
            }
        }

        public JiraIdViewModel()
        {
            TimeRegistrations = new List<TimeRegistration>();
        }
    }
}