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
        
        public BalanceViewModel BalanceViewModel { get; set; }
        public WeeksViewModel WeeksViewModel { get; set; }
    }
}