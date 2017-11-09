using System;
using System.Collections.Generic;
using Impact.Core.Model;

namespace Impact.Business.Scanner
{
    public interface IHoursScannerService
    {
        IEnumerable<DateTime> ScanWeeks(IEnumerable<Week> weeks);
    }
}