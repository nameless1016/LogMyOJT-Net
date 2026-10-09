namespace LogMyOJT.Models;

// one clock in / clock out record
public class TimeLogEntry
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }

    public DateOnly Date { get; set; }
    public TimeOnly TimeIn { get; set; }
    public TimeOnly? TimeOut { get; set; }
    public string Note { get; set; } = "";
    public LogStatus Status { get; set; } = LogStatus.Pending;

    public bool IsRunning => TimeOut is null;

    public double Hours => TimeOut is null
        ? 0
        : Math.Round(Math.Max(0, (TimeOut.Value.ToTimeSpan() - TimeIn.ToTimeSpan()).TotalHours), 1);
}
