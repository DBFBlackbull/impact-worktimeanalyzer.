using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Impact.Website.Models
{
    public class QuarterViewModel
    {
        public IEnumerable<SelectListItem> Quarters { get; set; }
        public string SelectedQuarter { get; set; }
        public string GraphTitle { get; set; }
        public bool Normalized { get; set; }

        [DisplayName("Fra")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime From { get; set; }

        [DisplayName("Til")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime To { get; set; }
        public string Json { get; set; }
        public string NormalizedJson { get; set; }
    }
}