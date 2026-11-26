using FinanceTrackerApi.Data;
using FinanceTrackerApi.Dtos;
using FinanceTrackerApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinanceTrackerApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly AppDbContext _context;

    // Injeção de Dependência: o ASP.NET Core entrega o DbContext configurado
    public TransactionsController(AppDbContext context)
    {
        _context = context;
    }

    // GET /api/transactions?type=Expense&category=Alimentação
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Transaction>>> GetAll(
        [FromQuery] TransactionType? type,
        [FromQuery] string? category)
    {
        var query = _context.Transactions.AsQueryable();

        if (type.HasValue)
        {
            query = query.Where(t => t.Type == type.Value);
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            var trimmedCategory = category.Trim().ToLower();
            query = query.Where(t => t.Category.ToLower() == trimmedCategory);
        }

        var result = await query.OrderByDescending(t => t.Date).ToListAsync();
        return Ok(result);
    }

    // GET /api/transactions/1
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Transaction>> GetById(int id)
    {
        var transaction = await _context.Transactions.FindAsync(id);
        if (transaction is null)
        {
            return NotFound(new { message = $"Transação com ID {id} não encontrada." });
        }

        return Ok(transaction);
    }

    // GET /api/transactions/summary
    [HttpGet("summary")]
    public async Task<ActionResult<FinanceSummaryResponse>> GetSummary()
    {
        // Se a base de dados estiver vazia, o ?? 0m evita erros de soma nula
        var totalIncome = await _context.Transactions
            .Where(t => t.Type == TransactionType.Income)
            .SumAsync(t => (decimal?)t.Amount) ?? 0m;

        var totalExpense = await _context.Transactions
            .Where(t => t.Type == TransactionType.Expense)
            .SumAsync(t => (decimal?)t.Amount) ?? 0m;

        var count = await _context.Transactions.CountAsync();

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
        var transaction = new Transaction
        {
            Description = request.Description.Trim(),
            Amount = request.Amount,
            Type = request.Type,
            Category = request.Category.Trim(),
            Date = request.Date,
            CreatedAt = DateTime.UtcNow
        };

        // Adiciona à tabela e grava no SQLite (o SQLite gera o Id automaticamente)
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = transaction.Id }, transaction);
    }

    // PUT /api/transactions/1
    [HttpPut("{id:int}")]
    public async Task<ActionResult<Transaction>> Update(int id, [FromBody] CreateTransactionRequest request)
    {
        var transaction = await _context.Transactions.FindAsync(id);
        if (transaction is null)
        {
            return NotFound(new { message = $"Transação com ID {id} não encontrada." });
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
        var transaction = await _context.Transactions.FindAsync(id);
        if (transaction is null)
        {
            return NotFound(new { message = $"Transação com ID {id} não encontrada." });
        }

        _context.Transactions.Remove(transaction);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}