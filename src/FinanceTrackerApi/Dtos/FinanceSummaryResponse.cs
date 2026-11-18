namespace FinanceTrackerApi.Dtos;

public record FinanceSummaryResponse(
    decimal TotalIncome,
    decimal TotalExpense,
    decimal Balance,
    int TransactionCount
);