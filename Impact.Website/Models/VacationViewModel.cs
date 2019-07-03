using System.Collections.Generic;
using System.Web.Mvc;
using Impact.Core.Model;
using Impact.Website.Models.Charts;

namespace Impact.Website.Models
{
    public class VacationViewModel
    {
        public string DivId { get; set; }
        public IEnumerable<SelectListItem> VacationYears { get; set; }
        public string SelectedVacationYear { get; set; }
        public VacationYear VacationYear { get; set; }
        public IEnumerable<VacationDay> VacationDays { get; set; }
        public decimal SummedVacationDays { get; set; }
        public decimal SummedExtraVacationDays { get; set; }
    }
}