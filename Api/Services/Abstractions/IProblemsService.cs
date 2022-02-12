using Api.Models.Problem;
using Core.Types;
using Data.Models;
using Data.Models.Enums;

namespace Api.Services.Abstractions;

public interface IProblemsService
{
    Task<Result<bool, Exception>> CreateAsync(ProblemCreateDto problem);

    IEnumerable<ProblemTypeDescription> GetAllDescriptions();

    Task<ProblemTypeDescription> FetchMostRecentAsync(Guid id);

    IQueryable<Problem> GetFilteredByType(ProblemType type);

    IQueryable<Problem> GetFilteredById(Guid id);

    Task<Result<bool, Exception>> DeleteAsync(Problem problem);
}
