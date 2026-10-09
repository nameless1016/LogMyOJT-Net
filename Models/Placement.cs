namespace LogMyOJT.Models;

// the company and schedule a student is doing their OJT at
public class Placement
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }

    public string CompanyName { get; set; } = "";
    public string CompanyAddress { get; set; } = "";
    public string SupervisorName { get; set; } = "";
    public string SupervisorEmail { get; set; } = "";
    public string SupervisorPhone { get; set; } = "";
    public double RequiredHours { get; set; } = 486;
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string ApprovalCode { get; set; } = "";

    public bool IsSet => !string.IsNullOrWhiteSpace(CompanyName) && StartDate is not null && EndDate is not null;
}
