using Api.Models.Solutions;
using Core.Types;

namespace Api.Services.Abstractions;

public interface ISolutionsService
{
    Task<Result<bool, Exception>> SaveAsync(SolutionCreateDto solution);
}
