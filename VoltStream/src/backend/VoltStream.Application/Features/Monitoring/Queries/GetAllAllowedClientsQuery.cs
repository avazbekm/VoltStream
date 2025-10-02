﻿namespace VoltStream.Application.Features.Monitoring.Queries;

using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VoltStream.Application.Commons.Interfaces;
using VoltStream.Application.Features.Monitoring.DTOs;

public record GetAllAllowedClientsQuery() : IRequest<IReadOnlyCollection<AllowedClientDto>>;

public class GetAllAllowedClientQueryHandler(
    IAppDbContext context,
    IMapper mapper)
    : IRequestHandler<GetAllAllowedClientsQuery, IReadOnlyCollection<AllowedClientDto>>
{
    public async Task<IReadOnlyCollection<AllowedClientDto>> Handle(GetAllAllowedClientsQuery request, CancellationToken cancellationToken)
    {
        var s = mapper.Map<IReadOnlyCollection<AllowedClientDto>>(await context.AllowedClients
                 .Where(w => !w.IsDeleted)
                 .ToListAsync(cancellationToken));

        return s;
    }
}
