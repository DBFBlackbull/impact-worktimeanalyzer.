using System.Collections.Generic;
using Impact.Core.Model;

namespace Impact.Website.Models
{
    public class DemoOvertimeViewModel : QuarterViewModel
    {
        public List<Week> InputWeeks { get; set; }
        public DemoOvertimeViewModel()
        {
            InputWeeks = new List<Week>();
        }
    }
}