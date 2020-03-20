using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Impact.Website.Models.Charts;

namespace Impact.Website.Models
{
    public class PlanningViewModel
    {
        [DisplayName("Fra")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime From { get; set; }
        
        [DisplayName("Til")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime To { get; set; }

        public PieChartViewModel PieChartViewModel { get; set; }
    }
}