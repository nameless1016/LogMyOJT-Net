using System.Globalization;
using System.Text;

namespace LogMyOJT.Services;

public class TimeLogEntry
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly TimeIn { get; set; }
    public TimeOnly? TimeOut { get; set; }
    public string Note { get; set; } = "";
    public string Status { get; set; } = "pending";   // pending, approved, rejected

    public bool IsRunning => TimeOut is null;

    public double Hours => TimeOut is null
        ? 0
        : Math.Round(Math.Max(0, (TimeOut.Value.ToTimeSpan() - TimeIn.ToTimeSpan()).TotalHours), 1);
}

public class Placement
{
    public string CompanyName { get; set; } = "";
    public string CompanyAddress { get; set; } = "";
    public string SupervisorName { get; set; } = "";
    public string SupervisorEmail { get; set; } = "";
    public string SupervisorPhone { get; set; } = "";
    public double RequiredHours { get; set; } = 486;
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string ApprovalCode { get; set; } = MakeCode();

    public bool IsSet => !string.IsNullOrWhiteSpace(CompanyName) && StartDate is not null && EndDate is not null;

    public Placement Clone() => (Placement)MemberwiseClone();

    private static string MakeCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        string Part(int n) => new(Enumerable.Range(0, n).Select(_ => chars[Random.Shared.Next(chars.Length)]).ToArray());
        return $"{Part(4)}-{Part(4)}";
    }
}

public record Reminder(string Tone, string Icon, string Title, string Detail);   // tone is "warn" or "ok"

public class ReportRecord
{
    public int Id { get; set; }
    public DateTime GeneratedAt { get; set; }
    public string Label { get; set; } = "";
    public double Hours { get; set; }
    public string Csv { get; set; } = "";
    public string FileName { get; set; } = "";
}

// Everything the app tracks for OJT hours. Lives in memory for the session, no database yet.
public class OjtState
{
    private static readonly CultureInfo C = CultureInfo.InvariantCulture;
    private int nextId = 1;
    private int nextReportId = 1;

    public List<TimeLogEntry> Logs { get; } = new();
    public Placement Placement { get; set; } = new();
    public List<ReportRecord> Reports { get; } = new();

    public IEnumerable<TimeLogEntry> Newest => Logs.OrderByDescending(l => l.Date).ThenByDescending(l => l.TimeIn);
    public TimeLogEntry? Running => Logs.FirstOrDefault(l => l.IsRunning);
    public TimeLogEntry? Find(int id) => Logs.FirstOrDefault(l => l.Id == id);

    // finished entries that were not rejected are what counts toward the total
    private IEnumerable<TimeLogEntry> Counted => Logs.Where(l => !l.IsRunning && l.Status != "rejected");

    public double HoursLogged => Math.Round(Counted.Sum(l => l.Hours), 1);
    public double HoursRemaining => Math.Max(0, Math.Round(Placement.RequiredHours - HoursLogged, 1));
    public int PercentDone => Placement.RequiredHours <= 0 ? 0 : (int)Math.Min(100, Math.Round(HoursLogged / Placement.RequiredHours * 100));
    public int PendingCount => Logs.Count(l => !l.IsRunning && l.Status == "pending");

    public static DateOnly WeekStart(DateOnly d) => d.AddDays(-(((int)d.DayOfWeek + 6) % 7));

    public double HoursInRange(DateOnly from, DateOnly to) =>
        Math.Round(Counted.Where(l => l.Date >= from && l.Date <= to).Sum(l => l.Hours), 1);

    public double HoursThisWeek(DateOnly today)
    {
        var start = WeekStart(today);
        return HoursInRange(start, start.AddDays(6));
    }

    // clock in and out
    public TimeLogEntry ClockIn(DateTime now)
    {
        if (Running is { } already) return already;
        var entry = new TimeLogEntry
        {
            Date = DateOnly.FromDateTime(now),
            TimeIn = new TimeOnly(now.Hour, now.Minute),
        };
        Add(entry);
        return entry;
    }

    public void ClockOut(DateTime now)
    {
        if (Running is not { } entry) return;
        var out_ = new TimeOnly(now.Hour, now.Minute);
        entry.TimeOut = out_ < entry.TimeIn ? entry.TimeIn : out_;
    }

    public void Add(TimeLogEntry entry)
    {
        entry.Id = nextId++;
        Logs.Add(entry);
    }

    public void Remove(int id) => Logs.RemoveAll(l => l.Id == id);

    // pace and deadlines, these only work once the placement dates are filled in
    public double? PaceDelta(DateOnly today)
    {
        if (Placement.StartDate is not { } s || Placement.EndDate is not { } e || e <= s) return null;
        double total = e.DayNumber - s.DayNumber + 1;
        double elapsed = Math.Clamp(today.DayNumber - s.DayNumber + 1, 0, total);
        return Math.Round(HoursLogged - Placement.RequiredHours * elapsed / total, 1);
    }

    public int? DaysLeft(DateOnly today) =>
        Placement.EndDate is { } e ? Math.Max(0, e.DayNumber - today.DayNumber) : null;

    public double? NeededPerWeek()
    {
        if (Placement.StartDate is not { } s || Placement.EndDate is not { } e || e <= s) return null;
        return Math.Round(Placement.RequiredHours / ((e.DayNumber - s.DayNumber + 1) / 7.0), 1);
    }

    public double? NeededPerWorkingDay(DateOnly today)
    {
        if (Placement.EndDate is not { } e) return null;
        int days = 0;
        for (var d = today; d <= e; d = d.AddDays(1))
            if (d.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday)) days++;
        return days == 0 ? null : Math.Round(HoursRemaining / days, 1);
    }

    public (int Week, int Total)? WeekInfo(DateOnly today)
    {
        if (Placement.StartDate is not { } s || Placement.EndDate is not { } e || e <= s) return null;
        int total = (int)Math.Ceiling((e.DayNumber - s.DayNumber + 1) / 7.0);
        int week = Math.Clamp((today.DayNumber - s.DayNumber) / 7 + 1, 1, total);
        return (week, total);
    }

    public List<Reminder> Reminders(DateOnly today)
    {
        var list = new List<Reminder>();

        if (!Placement.IsSet)
            list.Add(new Reminder("warn", "briefcase", "Set up your placement", "Add your company and dates to track your pace."));

        if (PaceDelta(today) is { } delta)
        {
            if (delta < -0.5)
                list.Add(new Reminder("warn", "alert", $"You're {Fmt.Hours(-delta)} hrs behind pace",
                    NeededPerWeek() is { } w ? $"About {Fmt.Hours(w)} hrs a week gets you there." : "Log a few extra hours to catch up."));
            else
                list.Add(new Reminder("ok", "checkc", "You're on pace", "Keep logging and you'll finish on time."));
        }

        var noNote = Logs.Where(l => !l.IsRunning && string.IsNullOrWhiteSpace(l.Note)).OrderByDescending(l => l.Date).FirstOrDefault();
        if (noNote is not null)
            list.Add(new Reminder("warn", "file", $"{Fmt.Date(noNote.Date)} has no activity note", "Add one so it can be approved."));

        if (PendingCount > 0)
            list.Add(new Reminder("ok", "clock", $"{PendingCount} {(PendingCount == 1 ? "entry is" : "entries are")} waiting for approval",
                "Your supervisor still needs to review them."));

        return list;
    }

    // hours for the last few weeks, oldest first, for the bar chart
    public List<(DateOnly Start, double Hours)> WeeklyHours(DateOnly today, int weeks = 6)
    {
        var first = WeekStart(today).AddDays(-7 * (weeks - 1));
        return Enumerable.Range(0, weeks)
            .Select(i => first.AddDays(7 * i))
            .Select(s => (s, HoursInRange(s, s.AddDays(6))))
            .ToList();
    }

    // reports
    public ReportRecord BuildReport(string period, DateOnly today, bool includeNotes, DateTime now, string student)
    {
        DateOnly from, to;
        string label;
        switch (period)
        {
            case "Weekly":
                from = WeekStart(today); to = from.AddDays(6);
                label = $"Weekly · {from.ToString("MMM d", C)} – {to.ToString("MMM d", C)}";
                break;
            case "Monthly":
                from = new DateOnly(today.Year, today.Month, 1); to = from.AddMonths(1).AddDays(-1);
                label = $"Monthly · {from.ToString("MMMM yyyy", C)}";
                break;
            default:
                from = DateOnly.MinValue; to = DateOnly.MaxValue;
                label = "Full placement to date";
                break;
        }

        var rows = Counted.Where(l => l.Date >= from && l.Date <= to).OrderBy(l => l.Date).ThenBy(l => l.TimeIn).ToList();
        double total = Math.Round(rows.Sum(l => l.Hours), 1);

        var sb = new StringBuilder();
        sb.AppendLine($"Student,{Esc(student)}");
        sb.AppendLine($"Company,{Esc(Placement.CompanyName)}");
        sb.AppendLine($"Report,{Esc(label)}");
        sb.AppendLine();
        sb.AppendLine(includeNotes ? "Date,Time in,Time out,Hours,Status,Note" : "Date,Time in,Time out,Hours,Status");
        foreach (var l in rows)
        {
            var line = $"{l.Date.ToString("yyyy-MM-dd", C)},{Fmt.Time(l.TimeIn)},{Fmt.Time(l.TimeOut!.Value)},{Fmt.Hours(l.Hours)},{l.Status}";
            sb.AppendLine(includeNotes ? $"{line},{Esc(l.Note)}" : line);
        }
        sb.AppendLine($"Total,,,{Fmt.Hours(total)},");

        var report = new ReportRecord
        {
            Id = nextReportId++,
            GeneratedAt = now,
            Label = label,
            Hours = total,
            Csv = sb.ToString(),
        };
        report.FileName = $"logmyojt-report-{now.ToString("yyyyMMdd", C)}-{report.Id}.csv";
        Reports.Insert(0, report);
        return report;
    }

    private static string Esc(string s) =>
        s.Contains(',') || s.Contains('"') || s.Contains('\n') ? "\"" + s.Replace("\"", "\"\"") + "\"" : s;

    // handy for trying the app out without typing everything in
    public void LoadSample(DateOnly today)
    {
        if (!Placement.IsSet)
        {
            Placement = new Placement
            {
                CompanyName = "Northwind Tech",
                CompanyAddress = "12 Osmeña Blvd, Cebu City",
                SupervisorName = "Engr. Daniel Cruz",
                SupervisorEmail = "d.cruz@northwind.example",
                SupervisorPhone = "0917 555 0142",
                RequiredHours = 486,
                StartDate = today.AddDays(-14),
                EndDate = today.AddDays(125),
            };
        }

        var rows = new (string In, string Out, string Note, string Status)[]
        {
            ("09:01", "17:00", "Fixed bugs on the login form", "approved"),
            ("08:58", "17:02", "Wrote unit tests for the API", "approved"),
            ("09:03", "17:00", "Client meeting and notes", "pending"),
            ("09:00", "16:30", "", "rejected"),
            ("08:55", "17:05", "Database schema review", "approved"),
            ("09:00", "17:00", "Set up the staging server", "approved"),
            ("09:10", "17:00", "Onboarding with the new team", "approved"),
            ("09:00", "17:00", "Read the codebase and docs", "pending"),
        };

        var day = today;
        foreach (var r in rows)
        {
            do { day = day.AddDays(-1); } while (day.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday);
            Add(new TimeLogEntry
            {
                Date = day,
                TimeIn = TimeOnly.Parse(r.In, C),
                TimeOut = TimeOnly.Parse(r.Out, C),
                Note = r.Note,
                Status = r.Status,
            });
        }
    }

    public void Clear()
    {
        Logs.Clear();
        Reports.Clear();
        Placement = new Placement();
        nextId = 1;
        nextReportId = 1;
    }
}
