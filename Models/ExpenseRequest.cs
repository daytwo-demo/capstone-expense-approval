namespace ExpenseApproval.Api.Models;

public enum ExpenseStatus { Pending, Approved, Rejected }

public class ExpenseRequest
{
    public Guid Id { get; set; }
    public string Concept { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string RequestedBy { get; set; } = string.Empty;
    public ExpenseStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
