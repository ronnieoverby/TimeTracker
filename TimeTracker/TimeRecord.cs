using System;

namespace TimeTracker
{
    public class TimeRecord
    {
        public string Description { get; set; }
        public DateTimeOffset Time { get; set; }
    }
}