using FinanceTrackerApi.Dtos;
using FinanceTrackerApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace FinanceTrackerApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    // Armazenamento temporário em memória (na Fase 2 substituímos por SQLite + EF Core)
    private static readonly List<Transaction> _transactions = [];
    private static int _nextId = 1;

    // GET /api/transactions?type=Expense&category=Alimentação
    [HttpGet]
    public ActionResult<IEnumerable<Transaction>> GetAll(
        [FromQuery] TransactionType? type,
        [FromQuery] string? category)
    {
        IEnumerable<Transaction> query = _transactions;

        if (type.HasValue)
        {
            query = query.Where(t => t.Type == type.Value);
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(t => t.Category.Equals(category.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        var result = query.OrderByDescending(t => t.Date).ToList();
        return Ok(result);
    }

    // GET /api/transactions/1
    [HttpGet("{id:int}")]
    public ActionResult<Transaction> GetById(int id)
    {
        var transaction = _transactions.FirstOrDefault(t => t.Id == id);
        if (transaction is null)
        {
            return NotFound(new { message = $"Transação com ID {id} não encontrada." });
        }

        return Ok(transaction);
    }

    // GET /api/transactions/summary
    [HttpGet("summary")]
    public ActionResult<FinanceSummaryResponse> GetSummary()
    {
        var totalIncome = _transactions
            .Where(t => t.Type == TransactionType.Income)
            .Sum(t => t.Amount);

        var totalExpense = _transactions
            .Where(t => t.Type == TransactionType.Expense)
            .Sum(t => t.Amount);

        var summary = new FinanceSummaryResponse(
            TotalIncome: totalIncome,
            TotalExpense: totalExpense,
            Balance: totalIncome - totalExpense,
            TransactionCount: _transactions.Count
        );

        return Ok(summary);
    }

    // POST /api/transactions
    [HttpPost]
    public ActionResult<Transaction> Create([FromBody] CreateTransactionRequest request)
    {
        var transaction = new Transaction
        {
            Id = _nextId++,
            Description = request.Description.Trim(),
            Amount = request.Amount,
            Type = request.Type,
            Category = request.Category.Trim(),
            Date = request.Date,
            CreatedAt = DateTime.UtcNow
        };

        _transactions.Add(transaction);

        return CreatedAtAction(nameof(GetById), new { id = transaction.Id }, transaction);
    }

    // PUT /api/transactions/1
    [HttpPut("{id:int}")]
    public ActionResult<Transaction> Update(int id, [FromBody] CreateTransactionRequest request)
    {
        var transaction = _transactions.FirstOrDefault(t => t.Id == id);
        if (transaction is null)
        {
            return NotFound(new { message = $"Transação com ID {id} não encontrada." });
        }

        transaction.Description = request.Description.Trim();
        transaction.Amount = request.Amount;
        transaction.Type = request.Type;
        transaction.Category = request.Category.Trim();
        transaction.Date = request.Date;

        return Ok(transaction);
    }

    // DELETE /api/transactions/1
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var transaction = _transactions.FirstOrDefault(t => t.Id == id);
        if (transaction is null)
        {
            return NotFound(new { message = $"Transação com ID {id} não encontrada." });
        }

        _transactions.Remove(transaction);
        return NoContent();
    }
}