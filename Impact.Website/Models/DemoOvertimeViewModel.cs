using System.Collections.Generic;
using Impact.Core.Model;

namespace Impact.Website.Models
{
    public class DemoOvertimeViewModel
    {
        public List<Week> Weeks { get; set; }

        public DemoOvertimeViewModel()
        {
            Weeks = new List<Week>();
        }
    }
}