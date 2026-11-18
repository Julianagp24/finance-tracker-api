using System.ComponentModel.DataAnnotations;
using FinanceTrackerApi.Models;

namespace FinanceTrackerApi.Dtos;

public class CreateTransactionRequest
{
    [Required(ErrorMessage = "A descrição é obrigatória.")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "A descrição deve ter entre 2 e 200 caracteres.")]
    public string Description { get; set; } = string.Empty;

    [Range(0.01, 999999999.99, ErrorMessage = "O valor deve ser maior que zero.")]
    public decimal Amount { get; set; }

    [EnumDataType(typeof(TransactionType), ErrorMessage = "O tipo deve ser Income ou Expense.")]
    public TransactionType Type { get; set; }

    [Required(ErrorMessage = "A categoria é obrigatória.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "A categoria deve ter entre 2 e 100 caracteres.")]
    public string Category { get; set; } = string.Empty;

    [Required]
    public DateTime Date { get; set; }
}