using System.Linq;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;

namespace TimeTracker
{
    public class TimeRecordsByTime : AbstractIndexCreationTask<TimeRecord>
    {
        public TimeRecordsByTime()
        {
            Map = trs => from tr in trs
                         select new { tr.Time };

            Sort(x => x.Time, SortOptions.String);
        }
    }
}