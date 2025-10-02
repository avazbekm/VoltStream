﻿namespace VoltStream.Application.Features.Monitoring.Commands;

using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using VoltStream.Application.Commons.Exceptions;
using VoltStream.Application.Commons.Interfaces;
using VoltStream.Domain.Entities;

public record UpdateAllowedClientCommand(
    long Id,
    string IpAddress,
    string? DeviceName,
    bool IsActive)
    : IRequest<bool>;

public class UpdateAllowedClientCommandHandler(
    IAppDbContext context,
    IMapper mapper)
    : IRequestHandler<UpdateAllowedClientCommand, bool>
{
    public async Task<bool> Handle(UpdateAllowedClientCommand request, CancellationToken cancellationToken)
    {
        var client = await context.AllowedClients.FirstOrDefaultAsync(wh => wh.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(AllowedClient), nameof(request.Id), request.Id);
        Console.WriteLine($"DeviceName: {request.DeviceName}");

        mapper.Map(request, client);
        return await context.SaveAsync(cancellationToken) > 0;
    }
}
