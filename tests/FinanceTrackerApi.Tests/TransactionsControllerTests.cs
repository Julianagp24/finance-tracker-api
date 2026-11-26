using FinanceTrackerApi.Controllers;
using FinanceTrackerApi.Data;
using FinanceTrackerApi.Dtos;
using FinanceTrackerApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FinanceTrackerApi.Tests;

public class TransactionsControllerTests
{
    // Creates an isolated in-memory database for each test
    private static AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task Create_WithValidRequest_ReturnsCreatedAndPersistsToDatabase()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var controller = new TransactionsController(context);

        var request = new CreateTransactionRequest
        {
            Description = "Salary",
            Amount = 1500.00m,
            Type = TransactionType.Income,
            Category = "Job",
            Date = new DateTime(2026, 11, 1)
        };

        // Act
        var result = await controller.Create(request);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var createdTransaction = Assert.IsType<Transaction>(createdResult.Value);

        Assert.Equal("Salary", createdTransaction.Description);
        Assert.Equal(1500.00m, createdTransaction.Amount);
        Assert.Equal(TransactionType.Income, createdTransaction.Type);
        Assert.True(createdTransaction.Id > 0);

        // Verify it was actually saved in the database
        var dbTransaction = await context.Transactions.FindAsync(createdTransaction.Id);
        Assert.NotNull(dbTransaction);
        Assert.Equal("Salary", dbTransaction.Description);
    }

    [Fact]
    public async Task GetAll_WithFilters_ReturnsOnlyMatchingTransactions()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        context.Transactions.AddRange(
            new Transaction { Description = "Groceries", Amount = 50m, Type = TransactionType.Expense, Category = "Food", Date = new DateTime(2026, 11, 10) },
            new Transaction { Description = "Dinner", Amount = 30m, Type = TransactionType.Expense, Category = "Food", Date = new DateTime(2026, 11, 20) },
            new Transaction { Description = "Bus ticket", Amount = 5m, Type = TransactionType.Expense, Category = "Transport", Date = new DateTime(2026, 11, 15) }
        );
        await context.SaveChangesAsync();

        var controller = new TransactionsController(context);

        // Act: filter by Category = "food" and fromDate = 2026-11-12
        var result = await controller.GetAll(
            type: null,
            category: "food",
            fromDate: new DateTime(2026, 11, 12),
            toDate: null,
            page: 1,
            pageSize: 10
        );

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var pagedResult = Assert.IsType<PagedResult<Transaction>>(okResult.Value);

        Assert.Equal(1, pagedResult.TotalCount);
        Assert.Single(pagedResult.Items);
        Assert.Equal("Dinner", pagedResult.Items[0].Description);
    }

    [Fact]
    public async Task GetSummary_CalculatesCorrectIncomeExpenseAndBalance()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        context.Transactions.AddRange(
            new Transaction { Description = "Salary", Amount = 2000m, Type = TransactionType.Income, Category = "Work", Date = DateTime.UtcNow },
            new Transaction { Description = "Freelance", Amount = 500m, Type = TransactionType.Income, Category = "Work", Date = DateTime.UtcNow },
            new Transaction { Description = "Rent", Amount = 700m, Type = TransactionType.Expense, Category = "Housing", Date = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var controller = new TransactionsController(context);

        // Act
        var result = await controller.GetSummary();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var summary = Assert.IsType<FinanceSummaryResponse>(okResult.Value);

        Assert.Equal(2500m, summary.TotalIncome);
        Assert.Equal(700m, summary.TotalExpense);
        Assert.Equal(1800m, summary.Balance);
        Assert.Equal(3, summary.TransactionCount);
    }

    [Fact]
    public async Task GetById_WhenTransactionDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var controller = new TransactionsController(context);

        // Act
        var result = await controller.GetById(999);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task Delete_WhenExists_RemovesFromDatabaseAndReturnsNoContent()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var transaction = new Transaction
        {
            Description = "To Delete",
            Amount = 25m,
            Type = TransactionType.Expense,
            Category = "Misc",
            Date = DateTime.UtcNow
        };
        context.Transactions.Add(transaction);
        await context.SaveChangesAsync();

        var controller = new TransactionsController(context);

        // Act
        var result = await controller.Delete(transaction.Id);

        // Assert
        Assert.IsType<NoContentResult>(result);

        var existsInDb = await context.Transactions.FindAsync(transaction.Id);
        Assert.Null(existsInDb);
    }
}