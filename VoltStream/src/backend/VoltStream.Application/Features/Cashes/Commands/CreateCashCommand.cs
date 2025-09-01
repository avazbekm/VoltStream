﻿namespace VoltStream.Application.Features.Cashes.Commands;

using AutoMapper;
using MediatR;
using VoltStream.Application.Commons.Interfaces;
using VoltStream.Domain.Entities;

public record CreateCashCommand(decimal UzsBalance, decimal UsdBalance, decimal Kurs) : IRequest<long>;

public class CreateCashCommandHandler(
    IAppDbContext context,
    IMapper mapper) : IRequestHandler<CreateCashCommand, long>
{
    public async Task<long> Handle(CreateCashCommand request, CancellationToken cancellationToken)
    {

        var cash = mapper.Map<Cash>(request);
        context.Cashes.Add(cash);
        await context.SaveAsync(cancellationToken);
        return cash.Id;
    }
}