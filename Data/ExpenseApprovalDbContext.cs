using Microsoft.EntityFrameworkCore;
using ExpenseApproval.Api.Models;

namespace ExpenseApproval.Api.Data;

public class ExpenseApprovalDbContext : DbContext
{
    public ExpenseApprovalDbContext(DbContextOptions<ExpenseApprovalDbContext> options) : base(options) { }

    public DbSet<ExpenseRequest> ExpenseRequests => Set<ExpenseRequest>();
    public DbSet<ApprovalStep> ApprovalSteps => Set<ApprovalStep>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApprovalStep>()
            .HasOne<ExpenseRequest>()
            .WithMany()
            .HasForeignKey(s => s.ExpenseRequestId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
