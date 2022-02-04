using Api.Models.Problem;
using Core.Types;

namespace Api.Services.Abstractions;

public interface IProblemsService
{
    Task<Result<bool, Exception>> CreateAsync(ProblemCreateDto problem);

    IEnumerable<ProblemTypeDescription> GetAllDescriptions();

    Task<ProblemTypeDescription> FetchMostRecentAsync(Guid id);
}
