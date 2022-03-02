using Api.Models.Problem;
using Api.Services.Abstractions;
using Core.Types;
using Data.DbContexts;
using Data.Models;
using Data.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public class ProblemsService: IProblemsService
{
    private readonly AppDbContext _context;

    public ProblemsService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool, Exception>> CreateAsync(ProblemCreateDto problem)
    {
        var entity = new Problem
        {
            AuthorId = problem.AuthorId,
            Type = problem.Type,
            Title = problem.Title,
            Description = problem.Description,
            Input = problem.Input,
            Solutions = new List<Solution> {
                new()
                {
                    AuthorId = problem.AuthorId,
                    Source = problem.Solution.Source,
                    SolutionResult = new SolutionResult
                    {
                        ResultValues = problem.Solution.Outputs
                            .Select(x => new ResultValue { Value = x })
                            .ToList()
                    },
                    IsAuthored = true,
                },
            }
        };

        try
        {
            await _context.Problems.AddAsync(entity);
            await _context.SaveChangesAsync();
            return new Some<bool, Exception>(true);
        }
        catch (Exception e)
        {
            return new None<bool, Exception>(e);
        }
    }

    public IEnumerable<ProblemTypeDescription> GetAllDescriptions() => ProblemTypeDescription.List;

    public async Task<ProblemTypeDescription> FetchMostRecentAsync(Guid userId)
    {
        var allSolutionsOrdered = _context.Solutions
            .OrderBy(x => x.CreatedOn);

        var problemType = (userId != Guid.Empty
                              ? await allSolutionsOrdered
                                  .Where(x => x.AuthorId == userId)
                                  .Select(x => x.Problem.Type as ProblemType?)
                                  .FirstOrDefaultAsync()
                              : null)
                          ?? await allSolutionsOrdered
                              .Select(x => x.Problem.Type)
                              .FirstOrDefaultAsync();

        return ProblemTypeDescription.List
            .FirstOrDefault(
                x => x.Type == problemType,

                // This should never happen when we have at least one solution
                ProblemTypeDescription.List.First()
            );
    }

    public IQueryable<Problem> GetFilteredByType(ProblemType type) => _context.Problems
        .Where(x => x.Type == type);

    public IQueryable<Problem> GetFilteredById(Guid id) => _context.Problems
        .Where(x => x.Id == id);

    public async Task<Result<bool, Exception>> DeleteAsync(Problem problem)
    {
        try
        {
            _context.Problems.Remove(problem);
            await _context.SaveChangesAsync();
            return new Some<bool, Exception>(true);
        }
        catch (Exception e)
        {
            return new None<bool, Exception>(e);
        }
    }
}
