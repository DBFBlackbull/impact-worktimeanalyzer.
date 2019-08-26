using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Impact.Business.Time;
using Impact.Core.Constants;
using Impact.Core.Model;
using Impact.DataAccess.Timelog;
using Impact.Website.Models;
using Impact.Website.Models.Charts;
using Impact.Website.Models.Options;
using TimeLog.TransactionalApi.SDK.ProjectManagementService;

namespace Impact.Website.Controllers
{
    public class AwesomeThursdayController : Controller
    {
        private readonly ITimeRepository _timeRepository;
        private readonly ITimeService _timeService;

        public AwesomeThursdayController(ITimeRepository timeRepository, ITimeService timeService)
        {
            _timeRepository = timeRepository;
            _timeService = timeService;
        }

        // GET: AwesomeThursday
        public ActionResult Index()
        {
            if (!(HttpContext.Session[ApplicationConstants.SessionName.Token] is SecurityToken token))
                return RedirectToAction("Index", "Login");

            var hireDate = (DateTime) HttpContext.Session[ApplicationConstants.SessionName.HireDate];
            var awesomeThursdays = _timeRepository.GetAwesomeThursdays(hireDate, token).ToList();
            const string disclaimer = 
                "<p>Fed torsdag / R&D er i personalehåndbogen punkt 1.6.4 defineret til at være <i>'fra kl 12.30 og resten af arbejdsdagen'</i>. " +
                "Denne tidsmængde er afhænging af hvornår man møder om morgenen. " +
                "Antages det at der mødes kl 8.00 vil man altså have 3,5 timers Fed torsdag / R&D, men møder man først kl 9.00 har man 4,5 timers Fed torsdag / R&D.</p>" +
                "<p>Managers ønsker ikke at Fed torsdag / R&D bliver defineret super firkantet, da man dermed har mulighed for at skippe og skubbe den. " +
                "Der er nogle medarbejdere der aldrig afholder den, hvilket giver mulighed for at andre kan holde mere</p>" +
                "<p>Dermed er der ikke en præcis tidsenhed for hvor meget Fed torsdag / R&D der er tilgængelig hver måned. " +
                "<p>Derfor har denne analyse defineret Fed torsdag / R&D til at være 'en halv dag' hvilket er beregnet til 7,5 timer / 2 = <b>3,75 timer (eller 3 timer og 45 minutter) pr. måned</b></p>";
            
            var awesomeThursdayViewModel = new AwesomeThursdayViewModel
            {
                BalanceChartViewModel = CreateBalanceViewModel(awesomeThursdays),
                BarColumnChartViewModel = CreateMonthsOverviewViewModel(awesomeThursdays),
                Disclaimer = disclaimer
            };

            return View(awesomeThursdayViewModel);
        }
        
        private static BarColumnChartViewModel CreateBalanceViewModel(List<Month> months)
        {
            var sum = months.Sum(m => m.AwesomeThursdayHours + m.RAndDHours);
            var awesomeThursdayRegistered = Math.Round(Convert.ToDecimal(sum), 2);
            var totalAwesomeThursdayApproximation = months.Count * ApplicationConstants.AwesomeThursdayApproximation;

            var balance = totalAwesomeThursdayApproximation - awesomeThursdayRegistered;

            int dynamicXMax = (int) Math.Ceiling(Math.Abs(balance / 5)) * 5;
            int xMax = Math.Max(10, dynamicXMax);
            
            List<object[]> googleFormattedBalance = new List<object[]>
            {
                new object[]
                {
                    new Column{Label = "", Type = "string"},
                    new Column{Label = "Timer", Type = "number"},
                }
            };
            googleFormattedBalance.Add(new object[] {"Saldo", balance});

            var color = balance >= 0 ? ApplicationConstants.Color.Blue : ApplicationConstants.Color.Black;

            var options = new BarColumnOptions.MaterialOptionsViewModel()
            {
                Height = 170,
                Colors = new List<string> {color},
                Bars = BarOrientation.Horizontal,
                HAxis = new BarColumnOptions.AxisViewModel
                {
                    ViewWindow = new BarColumnOptions.AxisViewModel.ViewWindowViewModel
                    {
                        Max = xMax,
                        Min = xMax * -1
                    }
                }
            };
            options.Chart = new BarColumnOptions.MaterialOptionsViewModel.ChartViewModel
            {
                Title = "Fed torsdags / R&D saldo",
                Subtitle = "Viser din Fed tordags / R&D \"time-saldo\" siden 2012. Dette er summen af alle dine Fed torsdags / R&D timer divideret med 3,75 time pr. måned"
            };

            var balanceViewModel = new BarColumnChartViewModel
            {
                DivId = "balance_chart",
                RawData = googleFormattedBalance,
                Options = options
            };
            return balanceViewModel;
        }

        private BarColumnChartViewModel CreateMonthsOverviewViewModel(List<Month> months)
        {
            var firstDate = months.FirstOrDefault()?.Date;
            var lastDate = months.LastOrDefault()?.Date;

            var normalizedMonths = _timeService.GetNormalizedMonths(months);

            var max = Convert.ToInt32(Math.Max(months.Max(m => m.AwesomeThursdayHours), months.Max(m => m.RAndDHours)));

            var vAxisViewModel = new BarColumnOptions.AxisViewModel
            {
                ViewWindow = new BarColumnOptions.AxisViewModel.ViewWindowViewModel
                {
                    Max = max,
                    Min = 0
                }
            };

            var chartViewModel = new BarColumnOptions.MaterialOptionsViewModel.ChartViewModel
            {
                Title = $"Fed torsdage / R&D fra {firstDate:Y} til {lastDate:Y}",
                Subtitle = "De registrerede timer på Fed torsdag og R&D inden for perioden"
            };

            var optionsViewModel = new BarColumnOptions.MaterialOptionsViewModel
            {
                Height = 500,
                Chart = chartViewModel,
                VAxis = vAxisViewModel,
                IsStacked = true,
                Colors = new List<string>{ApplicationConstants.Color.Blue, ApplicationConstants.Color.Orange} 
            };

            var overviewChartViewModel = new BarColumnChartViewModel
            {
                DivId = "overview_chart",
                RawData = GetDataArray(months),
                NormalizedAllData = GetDataArray(normalizedMonths),
                Options = optionsViewModel
            };

            return overviewChartViewModel;
        }

        private static List<object[]> GetDataArray(IEnumerable<Month> months)
        {
            List<object[]> googleFormattedWeeks = new List<object[]>
            {
                new object[]
                {
                    new Column {Label = "Måned", Type = "string"},
                    new Column {Label = "Fed torsdag", Type = "number"},
                    new Column {Label = "R&D", Type = "number"}
                }
            };
            googleFormattedWeeks.AddRange(months.Select(month => month.ToArray()));
            return googleFormattedWeeks;
        }
    }
}