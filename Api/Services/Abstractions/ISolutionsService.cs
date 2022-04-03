using Api.Models.Solutions;
using Core.Types;
using Data.Models;

namespace Api.Services.Abstractions;

public interface ISolutionsService
{
    Task<Result<Guid, Exception>> CreateAsync(SolutionCreateDto solution);

    IQueryable<Solution> GetFilteredById(Guid id);

    IQueryable<Solution> GetAll();
}
