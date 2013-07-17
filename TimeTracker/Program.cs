using System;
using System.Linq;
using System.Text;

namespace TimeTracker
{
    class Program
    {
        private const string DatabasePath = "data.txt";
        private static readonly Database DB = Database.Open(DatabasePath);

        static void Main()
        {
            using (DB)
            {
                MenuItem selection;
                while ((selection = GetMenuSelection()) != MenuItem.Exit)
                {
                    Console.Clear();
                    CarryOutSelection(selection);
                    Prompt("Press enter to continue.");
                    Console.Clear();
                }
            }
        }

        private static void CarryOutSelection(MenuItem selection)
        {
            switch (selection)
            {
                case MenuItem.NewTimeRecord:
                    NewTimeRecord();
                    break;
                case MenuItem.DisplayTimeRecords:
                    DisplayTimeRecords();
                    break;
                case MenuItem.DeleteTimeRecord:
                    DeleteTimeRecord();
                    break;
                default:
                    Console.WriteLine("Unrecognized selection.");
                    break;
            }
        }

        private static void DeleteTimeRecord()
        {
            const int pageSize = 10;
            var page = 1;

            PageSelection selection;
            do
            {
                var skip = page*pageSize - pageSize;
                var prompt = new StringBuilder("Choose a record to delete. Page#: " + page).AppendLine();

                var items =
                    DB.TimeRecords
                      .OrderByDescending(x => x.Time)
                      .Skip(skip)
                      .Take(pageSize)
                      .Select((x, i) => new { Record = x, Number = i + 1 })
                      .ToDictionary(x => x.Number, x => x.Record);

                foreach (var item in items)
                    prompt.AppendFormat("{0}: {1} {2}", item.Key, item.Value.Time, item.Value.Description)
                          .AppendLine();

                prompt.AppendFormat("Commands: {0} (Default is {1})", string.Join(", ", Enum.GetNames(typeof(PageSelection))), default(PageSelection))
                      .AppendLine();

                selection = Prompt(prompt.ToString(), s => (PageSelection)Enum.Parse(typeof(PageSelection), s, true));

                Console.Clear();

                switch (selection)
                {
                    case PageSelection.Next:
                        page++;
                        continue;
                    case PageSelection.Previous:
                        page--;
                        continue;
                    case PageSelection.Finished:
                        break;
                    default:
                        try
                        {
                            DB.TimeRecords.Remove(items[(int) selection]);
                            DB.Save();
                        }
                        catch
                        {
                            Console.WriteLine("Invalid selection.");
                        }
                        break;
                }

            } while (selection != PageSelection.Finished);
        }

        private static void DisplayTimeRecords()
        {
            var since = Prompt("Since ({0}): ", DateTimeOffset.Parse, DateTimeOffset.Now.Date);
            var until = Prompt("Until ({0}): ", DateTimeOffset.Parse, DateTimeOffset.Now);

            Console.Clear();
            Console.WriteLine("Records since {0}", since);
            Console.WriteLine("and until {0}", until);
            Console.WriteLine();

            var records = DB.TimeRecords.Where(x => x.Time >= since && x.Time <= until).OrderBy(x => x.Time);

            foreach (var tr in records)
                Console.WriteLine("{0,-25} {1}", tr.Time.LocalDateTime, tr.Description);
        }

        private static MenuItem GetMenuSelection()
        {
            var prompt = new StringBuilder();

            var items = from e in Enum.GetValues(typeof(MenuItem)).Cast<MenuItem>()
                        select string.Format("{0}: {1}", (int)e, e.ToString().MakeReadable());

            foreach (var item in items)
                prompt.AppendLine(item);

            prompt.AppendLine();
            prompt.Append("Make a selection: ");

            return Prompt(prompt.ToString(), s => (MenuItem)Enum.Parse(typeof(MenuItem), s.RemoveWhitespace(), true));
        }

        static void NewTimeRecord()
        {
            DB.TimeRecords.Add(new TimeRecord
                                   {
                                       Description = Prompt("Description: "),
                                       Time = Prompt("Time ({0}): ", DateTimeOffset.Parse, DateTimeOffset.Now)
                                   });

            DB.Save();
        }

        private static T Prompt<T>(string prompt, Func<string, T> parser, T @default = default(T))
        {
            while (true)
            {
                Console.Write(prompt, @default);
                var input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                    return @default;

                try
                {
                    return parser(input);
                }
                catch
                {
                    Console.WriteLine("Couldn't understand input. Try again.");
                }
            }
        }

        private static string Prompt(string prompt, string @default = null)
        {
            return Prompt(prompt, s => s, @default);
        }

    }
}