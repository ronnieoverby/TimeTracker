using System;
using System.Linq;
using System.Text;

namespace TimeTracker
{
    class Program
    {
        static void Main()
        {
            using (Raven.DocStore)
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
                default:
                    Console.WriteLine("Unrecognized selection.");
                    break;
            }
        }

        private static void DisplayTimeRecords()
        {
            var since = Prompt("Since ({0}): ", DateTimeOffset.Parse, DateTimeOffset.Now.Date);
            var until = Prompt("Until ({0}): ", DateTimeOffset.Parse, DateTimeOffset.Now);
            
            Console.Clear();
            Console.WriteLine("Records since {0}", since);
            Console.WriteLine("and until {0}", until);
            Console.WriteLine();

            using (var s = Raven.OpenSession())
            {
                var records = s.Query<TimeRecord>().Where(x => x.Time >= since && x.Time <= until).OrderBy(x => x.Time);

                foreach (var tr in records)
                    Console.WriteLine("{0,-25} {1}", tr.Time.LocalDateTime, tr.Description);
            }
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

            return Prompt(prompt.ToString(), s => (MenuItem) Enum.Parse(typeof (MenuItem), s.RemoveWhitespace(), true));
        }

        static void NewTimeRecord()
        {
            using (var s = Raven.OpenSession())
            {
                s.Store(new TimeRecord
                            {
                                Description = Prompt("Description: "),
                                Time = Prompt("Time ({0}): ", DateTimeOffset.Parse, DateTimeOffset.Now)
                            });

                s.SaveChanges();
            }
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