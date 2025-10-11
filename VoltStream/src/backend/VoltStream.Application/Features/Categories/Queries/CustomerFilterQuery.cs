﻿namespace VoltStream.Application.Features.Categories.Queries;

using AutoMapper;
using MediatR;
using VoltStream.Application.Commons.Extensions;
using VoltStream.Application.Commons.Interfaces;
using VoltStream.Application.Commons.Models;
using VoltStream.Application.Features.Categories.DTOs;

public record CategoryFilterQuery : FilteringRequest, IRequest<IReadOnlyCollection<CategoryDto>>;

public class CategoryFilterQueryHandler(
    IAppDbContext context,
    IPagingMetadataWriter writer,
    IMapper mapper) : IRequestHandler<CategoryFilterQuery, IReadOnlyCollection<CategoryDto>>
{
    public async Task<IReadOnlyCollection<CategoryDto>> Handle(CategoryFilterQuery request, CancellationToken cancellationToken)
        => mapper.Map<IReadOnlyCollection<CategoryDto>>(await context.Categories
            .ToPagedListAsync(request, writer, cancellationToken));
}
