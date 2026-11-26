using System.Security.Claims;
using FinanceTrackerApi.Data;
using FinanceTrackerApi.Dtos;
using FinanceTrackerApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinanceTrackerApi.Controllers;

[Authorize] // Requires a valid JWT Bearer token for ALL endpoints in this controller
[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly AppDbContext _context;

    public TransactionsController(AppDbContext context)
    {
        _context = context;
    }

    // Helper to extract the authenticated user's ID from JWT Claims
    private int GetCurrentUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(claim, out var userId))
        {
            return userId;
        }

        throw new UnauthorizedAccessException("User ID claim is missing or invalid.");
    }

    // GET /api/transactions?type=Expense&category=Food&fromDate=2026-01-01&toDate=2026-12-31&page=1&pageSize=10
    [HttpGet]
    public async Task<ActionResult<PagedResult<Transaction>>> GetAll(
        [FromQuery] TransactionType? type,
        [FromQuery] string? category,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var userId = GetCurrentUserId();
        var query = _context.Transactions.Where(t => t.UserId == userId);

        if (type.HasValue)
        {
            query = query.Where(t => t.Type == type.Value);
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            var trimmedCategory = category.Trim().ToLower();
            query = query.Where(t => t.Category.ToLower() == trimmedCategory);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(t => t.Date >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(t => t.Date <= toDate.Value);
        }

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(t => t.Date)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return Ok(new PagedResult<Transaction>(items, totalCount, page, pageSize, totalPages));
    }

    // GET /api/transactions/1
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Transaction>> GetById(int id)
    {
        var userId = GetCurrentUserId();
        var transaction = await _context.Transactions
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

        if (transaction is null)
        {
            return NotFound(new { message = $"Transaction with ID {id} was not found." });
        }

        return Ok(transaction);
    }

    // GET /api/transactions/summary
    [HttpGet("summary")]
    public async Task<ActionResult<FinanceSummaryResponse>> GetSummary()
    {
        var userId = GetCurrentUserId();
        var userTransactions = _context.Transactions.Where(t => t.UserId == userId);

        var totalIncome = await userTransactions
            .Where(t => t.Type == TransactionType.Income)
            .SumAsync(t => (decimal?)t.Amount) ?? 0m;

        var totalExpense = await userTransactions
            .Where(t => t.Type == TransactionType.Expense)
            .SumAsync(t => (decimal?)t.Amount) ?? 0m;

        var count = await userTransactions.CountAsync();

        var summary = new FinanceSummaryResponse(
            TotalIncome: totalIncome,
            TotalExpense: totalExpense,
            Balance: totalIncome - totalExpense,
            TransactionCount: count
        );

        return Ok(summary);
    }

    // POST /api/transactions
    [HttpPost]
    public async Task<ActionResult<Transaction>> Create([FromBody] CreateTransactionRequest request)
    {
        var userId = GetCurrentUserId();

        var transaction = new Transaction
        {
            UserId = userId,
            Description = request.Description.Trim(),
            Amount = request.Amount,
            Type = request.Type,
            Category = request.Category.Trim(),
            Date = request.Date,
            CreatedAt = DateTime.UtcNow
        };

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = transaction.Id }, transaction);
    }

    // PUT /api/transactions/1
    [HttpPut("{id:int}")]
    public async Task<ActionResult<Transaction>> Update(int id, [FromBody] CreateTransactionRequest request)
    {
        var userId = GetCurrentUserId();
        var transaction = await _context.Transactions
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

        if (transaction is null)
        {
            return NotFound(new { message = $"Transaction with ID {id} was not found." });
        }

        transaction.Description = request.Description.Trim();
        transaction.Amount = request.Amount;
        transaction.Type = request.Type;
        transaction.Category = request.Category.Trim();
        transaction.Date = request.Date;

        await _context.SaveChangesAsync();

        return Ok(transaction);
    }

    // DELETE /api/transactions/1
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetCurrentUserId();
        var transaction = await _context.Transactions
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

        if (transaction is null)
        {
            return NotFound(new { message = $"Transaction with ID {id} was not found." });
        }

        _context.Transactions.Remove(transaction);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}