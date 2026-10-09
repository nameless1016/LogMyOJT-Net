namespace LogMyOJT.Models;

// a generated hours report (weekly, monthly or full placement)
public class Report
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }

    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public string Label { get; set; } = "";
    public double Hours { get; set; }
    public string Csv { get; set; } = "";
    public string FileName { get; set; } = "";
}
