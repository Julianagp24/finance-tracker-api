using System.ComponentModel.DataAnnotations;
using FinanceTrackerApi.Models;

namespace FinanceTrackerApi.Dtos;

public class CreateTransactionRequest
{
    [Required(ErrorMessage = "Description is required.")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Description must be between 2 and 200 characters.")]
    public string Description { get; set; } = string.Empty;

    [Range(0.01, 999999999.99, ErrorMessage = "Amount must be greater than zero.")]
    public decimal Amount { get; set; }

    [EnumDataType(typeof(TransactionType), ErrorMessage = "Type must be either Income or Expense.")]
    public TransactionType Type { get; set; }

    [Required(ErrorMessage = "Category is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Category must be between 2 and 100 characters.")]
    public string Category { get; set; } = string.Empty;

    [Required]
    public DateTime Date { get; set; }
}