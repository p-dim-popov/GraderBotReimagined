using Api.Models.Solutions;
using Core.Types;
using Data.Models;

namespace Api.Services.Abstractions;

public interface ISolutionsService
{
    Task<Result<bool, Exception>> SaveAsync(SolutionCreateDto solution);

    IQueryable<Solution> GetFilteredById(Guid id);
}
