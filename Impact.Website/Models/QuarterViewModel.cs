using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Web.Mvc;

namespace Impact.Website.Models
{
    public class QuarterViewModel
    {
        public IEnumerable<SelectListItem> Quarters { get; set; }
        public string SelectedQuarter { get; set; }
        
        public BalanceChartViewModel BalanceChartViewModel { get; set; }
        public WeeksChartViewModel WeeksChartViewModel { get; set; }
        public PieChartViewModel PieChartViewModel { get; set; }
        public GaugeChartViewModel PotentialChartViewModel { get; set; }
    }
}