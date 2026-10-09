namespace LogMyOJT.Models;

// a student who is logging OJT hours
public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Bio { get; set; } = "";
    public string? ProfileImage { get; set; }
    public bool DarkMode { get; set; }
    public bool EmailNotifications { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // a user has one placement, many time logs and many reports
    public Placement? Placement { get; set; }
    public List<TimeLogEntry> TimeLogs { get; set; } = new();
    public List<Report> Reports { get; set; } = new();

    public string FullName => $"{FirstName} {LastName}".Trim();
}
