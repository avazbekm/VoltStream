﻿namespace ApiServices.Models.Requests;

public record SaleRequest
{
    public long Id { get; set; }
    public DateTime Date { get; set; }
    public long? CustomerId { get; set; }
    public long CurrencyId { get; set; }
    public int RollCount { get; set; }
    public decimal Length { get; set; }
    public decimal Amount { get; set; }
    public long? CustomerOperationId { get; set; }
    public bool IsApplied { get; set; }
    public decimal? Discount { get; set; }
    public string Description { get; set; } = string.Empty;
    public List<SaleItemRequest> Items { get; set; } = default!;
}