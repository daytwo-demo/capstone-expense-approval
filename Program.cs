using Microsoft.EntityFrameworkCore;
using ExpenseApproval.Api.Data;
using ExpenseApproval.Api.Models;

var builder = WebApplication.CreateBuilder(args);

const string connectionString = "Host=localhost;Port=5432;Database=expenses;Username=expenses;Password=Expenses!2026";
const int MaxResultsPerPage = 50; // TODO: externalizar esta configuración (ConfigMap)
const string ExternalApiKey = "6f5608fdd1b0019b77001da48aa8559a"; // TODO: externalizar esta configuración (Secret)

builder.Services.AddDbContext<ExpenseApprovalDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));

var app = builder.Build();

app.MapGet("/api/carga/{n:int}", (int n) =>
{
    long Fib(int x) => x < 2 ? x : Fib(x - 1) + Fib(x - 2);
    var sw = System.Diagnostics.Stopwatch.StartNew();
    var resultado = Fib(n);
    sw.Stop();
    return Results.Ok(new { n, resultado, elapsedMs = sw.ElapsedMilliseconds });
});

var expenses = app.MapGroup("/api/expenses");

expenses.MapGet("", async (ExpenseApprovalDbContext db) =>
    Results.Ok(await db.ExpenseRequests.OrderBy(e => e.CreatedAt).Take(MaxResultsPerPage).ToListAsync()));

expenses.MapGet("/{id:guid}", async (Guid id, ExpenseApprovalDbContext db) =>
    await db.ExpenseRequests.FindAsync(id) is { } expense ? Results.Ok(expense) : Results.NotFound());

expenses.MapPost("", async (ExpenseRequest input, ExpenseApprovalDbContext db) =>
{
    if (string.IsNullOrEmpty(ExternalApiKey))
        return Results.Problem("Falta ExternalApiKey: no se puede notificar al sistema externo.", statusCode: 500);

    var expense = new ExpenseRequest
    {
        Id = Guid.NewGuid(),
        Concept = input.Concept,
        Amount = input.Amount,
        RequestedBy = input.RequestedBy,
        Status = ExpenseStatus.Pending,
        CreatedAt = DateTimeOffset.UtcNow,
        UpdatedAt = DateTimeOffset.UtcNow
    };
    db.ExpenseRequests.Add(expense);
    await db.SaveChangesAsync();
    return Results.Created($"/api/expenses/{expense.Id}", expense);
});

expenses.MapPut("/{id:guid}", async (Guid id, ExpenseRequest input, ExpenseApprovalDbContext db) =>
{
    var expense = await db.ExpenseRequests.FindAsync(id);
    if (expense is null) return Results.NotFound();

    expense.Concept = input.Concept;
    expense.Amount = input.Amount;
    expense.RequestedBy = input.RequestedBy;
    expense.Status = input.Status;
    expense.UpdatedAt = DateTimeOffset.UtcNow;
    await db.SaveChangesAsync();
    return Results.Ok(expense);
});

expenses.MapDelete("/{id:guid}", async (Guid id, ExpenseApprovalDbContext db) =>
{
    var expense = await db.ExpenseRequests.FindAsync(id);
    if (expense is null) return Results.NotFound();

    db.ExpenseRequests.Remove(expense);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

expenses.MapGet("/{id:guid}/steps", async (Guid id, ExpenseApprovalDbContext db) =>
{
    var exists = await db.ExpenseRequests.AnyAsync(e => e.Id == id);
    if (!exists) return Results.NotFound();

    var steps = await db.ApprovalSteps
        .Where(s => s.ExpenseRequestId == id)
        .OrderBy(s => s.DecidedAt)
        .ToListAsync();
    return Results.Ok(steps);
});

expenses.MapPost("/{id:guid}/steps", async (Guid id, ApprovalStep input, ExpenseApprovalDbContext db) =>
{
    var exists = await db.ExpenseRequests.AnyAsync(e => e.Id == id);
    if (!exists) return Results.NotFound();

    var step = new ApprovalStep
    {
        Id = Guid.NewGuid(),
        ExpenseRequestId = id,
        ApproverName = input.ApproverName,
        Decision = input.Decision,
        Comment = input.Comment,
        DecidedAt = DateTimeOffset.UtcNow
    };
    db.ApprovalSteps.Add(step);
    await db.SaveChangesAsync();
    return Results.Created($"/api/expenses/{id}/steps/{step.Id}", step);
});

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ExpenseApprovalDbContext>();
    db.Database.EnsureCreated();
}

app.Run();
