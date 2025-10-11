﻿namespace VoltStream.Application.Features.WarehouseStocks.DTOs;

using VoltStream.Application.Features.Products.DTOs;

public record WarehouseStockDto
{
    public long Id { get; set; }
    public decimal RollCount { get; set; }  // rulon soni
    public decimal LengthPerRoll { get; set; }  //rulon uzunligi
    public decimal TotalLength { get; set; } // jami uzunlik
    public decimal UnitPrice { get; set; }
    public decimal DiscountRate { get; set; }
    public long WarehouseId { get; set; }
    public ProductForWarehouseDto Product { get; set; } = default!;
};
