﻿namespace ApiServices.Models.Requests;

public record ProductRequest
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public long CategoryId { get; set; }
}
