using System;

namespace Impact.Core.Model
{
    public class TimeRegistration
    {
        public string JiraId { get; }
        public string Task { get; }
        public string Project { get; }
        public DateTime Date { get; }
        public string Comment { get; }
        public decimal RawHours { get; }
        public string DisplayHours
        {
            get
            {
                var hours = Math.Truncate(RawHours);
                var minutes = (RawHours - hours) * 60m;
                return $"{hours:00}:{minutes:00}";
            }
        }

        public TimeRegistration(string jiraId, string task, string project, DateTime date, string comment, decimal rawHours)
        {
            JiraId = jiraId;
            Task = task;
            Project = project;
            Date = date;
            Comment = comment;
            RawHours = rawHours;
        }
    }
}