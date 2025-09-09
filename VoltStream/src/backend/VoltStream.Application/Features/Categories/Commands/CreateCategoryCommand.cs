﻿namespace VoltStream.Application.Features.Categories.Commands;

using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using VoltStream.Application.Commons.Exceptions;
using VoltStream.Application.Commons.Extensions;
using VoltStream.Application.Commons.Interfaces;
using VoltStream.Domain.Entities;

public record CreateCategoryCommand(string Name) : IRequest<long>;

public class CreateCategoryCommandHandler(
    IAppDbContext context,
    IMapper mapper) : IRequestHandler<CreateCategoryCommand, long>
{
    public async Task<long> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var categoryExists = await context.Categories
            .AnyAsync(p => p.NormalizedName == request.Name.ToNormalized(), cancellationToken);

        if (categoryExists)
            throw new AlreadyExistException(nameof(Category));

        var category = mapper.Map<Category>(request);
        context.Categories.Add(category);
        await context.SaveAsync(cancellationToken);
        return category.Id;
    }
}