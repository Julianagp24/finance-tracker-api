using System.Security.Claims;
using FinanceTrackerApi.Controllers;
using FinanceTrackerApi.Data;
using FinanceTrackerApi.Dtos;
using FinanceTrackerApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FinanceTrackerApi.Tests;

public class TransactionsControllerTests
{
    private const int TestUserId = 1;

    // Creates an isolated in-memory database for each test
    private static AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    // Creates a controller with an authenticated user (UserId = 1)
    private static TransactionsController CreateController(AppDbContext context, int userId = TestUserId)
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, "test@example.com")
        ], "TestAuth"));

        return new TransactionsController(context)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            }
        };
    }

    [Fact]
    public async Task Create_WithValidRequest_ReturnsCreatedAndPersistsToDatabase()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var controller = CreateController(context);

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
        Assert.Equal(TestUserId, createdTransaction.UserId);
        Assert.True(createdTransaction.Id > 0);

        // Verify it was actually saved in the database
        var dbTransaction = await context.Transactions.FindAsync(createdTransaction.Id);
        Assert.NotNull(dbTransaction);
        Assert.Equal("Salary", dbTransaction.Description);
        Assert.Equal(TestUserId, dbTransaction.UserId);
    }

    [Fact]
    public async Task GetAll_WithFilters_ReturnsOnlyMatchingTransactionsForCurrentUser()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        context.Transactions.AddRange(
            new Transaction { UserId = TestUserId, Description = "Groceries", Amount = 50m, Type = TransactionType.Expense, Category = "Food", Date = new DateTime(2026, 11, 10) },
            new Transaction { UserId = TestUserId, Description = "Dinner", Amount = 30m, Type = TransactionType.Expense, Category = "Food", Date = new DateTime(2026, 11, 20) },
            new Transaction { UserId = TestUserId, Description = "Bus ticket", Amount = 5m, Type = TransactionType.Expense, Category = "Transport", Date = new DateTime(2026, 11, 15) },
            // Another user's transaction (must NOT be returned)
            new Transaction { UserId = 999, Description = "Dinner", Amount = 100m, Type = TransactionType.Expense, Category = "Food", Date = new DateTime(2026, 11, 20) }
        );
        await context.SaveChangesAsync();

        var controller = CreateController(context);

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
        Assert.Equal(30m, pagedResult.Items[0].Amount); // Not the 100m from user 999!
    }

    [Fact]
    public async Task GetSummary_CalculatesCorrectIncomeExpenseAndBalanceForCurrentUser()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        context.Transactions.AddRange(
            new Transaction { UserId = TestUserId, Description = "Salary", Amount = 2000m, Type = TransactionType.Income, Category = "Work", Date = DateTime.UtcNow },
            new Transaction { UserId = TestUserId, Description = "Freelance", Amount = 500m, Type = TransactionType.Income, Category = "Work", Date = DateTime.UtcNow },
            new Transaction { UserId = TestUserId, Description = "Rent", Amount = 700m, Type = TransactionType.Expense, Category = "Housing", Date = DateTime.UtcNow },
            // Another user's transaction (must NOT be included in summary)
            new Transaction { UserId = 999, Description = "Secret Salary", Amount = 10000m, Type = TransactionType.Income, Category = "Work", Date = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var controller = CreateController(context);

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
        var controller = CreateController(context);

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
            UserId = TestUserId,
            Description = "To Delete",
            Amount = 25m,
            Type = TransactionType.Expense,
            Category = "Misc",
            Date = DateTime.UtcNow
        };
        context.Transactions.Add(transaction);
        await context.SaveChangesAsync();

        var controller = CreateController(context);

        // Act
        var result = await controller.Delete(transaction.Id);

        // Assert
        Assert.IsType<NoContentResult>(result);

        var existsInDb = await context.Transactions.FindAsync(transaction.Id);
        Assert.Null(existsInDb);
    }
}