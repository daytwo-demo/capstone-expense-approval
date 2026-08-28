namespace ExpenseApproval.Api.Models;

public enum ApprovalDecision { Approved, Rejected }

public class ApprovalStep
{
    public Guid Id { get; set; }
    public Guid ExpenseRequestId { get; set; }
    public string ApproverName { get; set; } = string.Empty;
    public ApprovalDecision Decision { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTimeOffset DecidedAt { get; set; }
}
