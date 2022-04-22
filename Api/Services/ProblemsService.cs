using Api.Models.Problem;
using Api.Services.Abstractions;
using Core.Types;
using Core.Utilities;
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

    public async Task<Result<Problem, Exception>> CreateAsync(ProblemCreateDto problem)
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

        var executeResult = await Ops.RunCatchingAsync(async () =>
        {
            await _context.Problems.AddAsync(entity);
            await _context.SaveChangesAsync();
        });

        if (executeResult is None<bool, Exception> { Error: {} exception })
        {
            return new None<Problem, Exception>(exception);
        }

        return new Some<Problem, Exception>(entity);
    }

    public async Task<Result<Problem, Exception>> EditAsync(ProblemEditDto problem, bool isAdmin)
    {
        var entityQuery = _context.Problems.AsQueryable();

        var hasNewSolution = problem.Solution is not null;
        if (hasNewSolution)
        {
            entityQuery = entityQuery.Include(x => x.Solutions)
                .ThenInclude(x => x.SolutionResult)
                .ThenInclude(x => x.ResultValues);
        }

        var entity = await entityQuery
            .FirstOrDefaultAsync(x => x.Id == problem.Id && (isAdmin || x.AuthorId == problem.AuthorId));

        if (entity is null)
        {
            return new None<Problem, Exception>(new Exception("Operation not allowed. Not owner or not admin"));
        }

        entity.Type = problem.Type;
        entity.Title = problem.Title;
        entity.Description = problem.Description;
        entity.Input = problem.Input ?? entity.Input;

        var oldAuthorSolution = hasNewSolution ? entity.Solutions.First(s => s.IsAuthored) : null;

        if (hasNewSolution)
        {
            entity.Solutions = entity.Solutions
                .Where(s => !s.IsAuthored)
                .Concat(new []
                {
                    new Solution {
                        AuthorId = entity.AuthorId,
                        Source = problem.Solution!.Source,
                        SolutionResult = new SolutionResult
                        {
                            ResultValues = problem.Solution.Outputs
                                .Select(x => new ResultValue {Value = x})
                                .ToList()
                        },
                        IsAuthored = true,
                    }
                })
                .ToList();
        }

        var executeResult = await Ops.RunCatchingAsync(async () =>
        {
            if (hasNewSolution)
            {
                _context.ResultValues.RemoveRange(oldAuthorSolution.SolutionResult.ResultValues);
                _context.SolutionResults.Remove(oldAuthorSolution.SolutionResult);
                _context.Solutions.Remove(oldAuthorSolution);
            }

            await _context.SaveChangesAsync();
        });

        if (executeResult is None<bool, Exception> { Error: {} exception })
        {
            return new None<Problem, Exception>(exception);
        }

        return new Some<Problem, Exception>(entity);
    }

    public IEnumerable<ProblemTypeDescription> GetAllDescriptions() => ProblemTypeDescription.List;

    public async Task<ProblemTypeDescription> FetchMostRecentAsync(Guid userId)
    {
        var allSolutionsOrdered = _context.Solutions
            .OrderByDescending(x => x.CreatedOn);

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
        var executeResult = await Ops.RunCatchingAsync(async () =>
        {
            _context.Problems.Remove(problem);
            await _context.SaveChangesAsync();
        });

        if (executeResult is None<bool, Exception> { Error: {} exception })
        {
            return new None<bool, Exception>(exception);
        }

        return new Some<bool, Exception>(true);
    }

    public IQueryable<Problem> GetAll() => _context.Problems;
}
