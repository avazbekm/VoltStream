﻿namespace VoltStream.Application.Features.Categories.Queries;

using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using VoltStream.Application.Commons.Exceptions;
using VoltStream.Application.Commons.Interfaces;
using VoltStream.Application.Features.Categories.DTOs;
using VoltStream.Domain.Entities;

public record GetCategoryByIdQuery(long Id) : IRequest<CategoryDto>;

public class GetCategoryByIdQueryHandler(
    IAppDbContext context,
    IMapper mapper)
    : IRequestHandler<GetCategoryByIdQuery, CategoryDto>
{
    public async Task<CategoryDto> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
        => mapper.Map<CategoryDto>(await context.Categories
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken))
        ?? throw new NotFoundException(nameof(Category), nameof(request.Id), request.Id);
}