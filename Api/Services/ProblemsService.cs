using Api.Models.Problem;
using Api.Services.Abstractions;
using Core.Types;
using Data.DbContexts;
using Data.Models;

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
            Data = problem.Source,
        };

        try
        {
            await _context.Problems.AddAsync(entity);
            await _context.SaveChangesAsync();
            return new SuccessResult<bool, Exception>(true);
        }
        catch (Exception e)
        {
            return new ErrorResult<bool, Exception>(e);
        }
    }

    public IEnumerable<ProblemTypeDescription> GetAllDescriptions() => ProblemTypeDescription.List;
}
