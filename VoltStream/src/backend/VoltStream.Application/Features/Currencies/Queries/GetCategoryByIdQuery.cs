﻿namespace VoltStream.Application.Features.Currencies.Queries;

using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using VoltStream.Application.Commons.Exceptions;
using VoltStream.Application.Commons.Interfaces;
using VoltStream.Application.Features.Currencies.DTOs;
using VoltStream.Domain.Entities;

public record GetCurrencyByIdQuery(long Id) : IRequest<CurrencyDto>;

public class GetCurrencyByIdQueryHandler(
    IAppDbContext context,
    IMapper mapper)
    : IRequestHandler<GetCurrencyByIdQuery, CurrencyDto>
{
    public async Task<CurrencyDto> Handle(GetCurrencyByIdQuery request, CancellationToken cancellationToken)
        => mapper.Map<CurrencyDto>(await context.Currencies
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken))
        ?? throw new NotFoundException(nameof(Currency), nameof(request.Id), request.Id);
}