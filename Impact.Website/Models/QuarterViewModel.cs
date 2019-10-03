using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Impact.Website.Models.Charts;

namespace Impact.Website.Models
{
    public class QuarterViewModel
    {
        public IEnumerable<SelectListItem> Quarters { get; set; }
        public string SelectedQuarter { get; set; }
        public bool IncludeAllWeeks { get; set; }
        public bool ShowIncludeAllWeeksButton { get; set; }
        public List<Tuple<string, string>> DisplayNormalWorkWeeks { get; set; }
        public string DisplayFlexZero { get; set; }
        public string DisplayFlex100 { get; set; }
        public string DisplayPayout { get; set; }
        
        public BarColumnChartViewModel BalanceChartViewModel { get; set; }
        public BarColumnChartViewModel BarColumnChartViewModel { get; set; }
        public PieChartViewModel PieChartViewModel { get; set; }
        public GaugeChartViewModel PotentialChartViewModel { get; set; }
        public SummedViewModel SummedViewModel { get; set; }
    }
}