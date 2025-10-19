﻿namespace ApiServices.Interfaces;

using ApiServices.Models;
using ApiServices.Models.Requests;
using ApiServices.Models.Responses;
using Refit;

public interface IPaymentApi
{
    [Post("/payments")]
    Task<Response<long>> CreateAsync([Body] PaymentRequest request);

    [Put("/payments​")]
    Task<Response<bool>> UpdateAsync([Body] PaymentRequest request);

    [Delete("/payments​/{id}")]
    Task<Response<bool>> DeleteAsync(long id);

    [Get("/payments/{id}")]
    Task<Response<PaymentResponse>> GetByIdAsync(long id);

    [Get("/payments")]
    Task<Response<List<PaymentResponse>>> GetAllAsync();

    [Post("/payments/filter")]
    Task<Response<List<PaymentResponse>>> Filter(FilteringRequest request);
}