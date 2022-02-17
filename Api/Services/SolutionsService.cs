using Api.Models.Solutions;
using Api.Services.Abstractions;
using Core.Types;
using Data.DbContexts;
using Data.Models;

namespace Api.Services;

public class SolutionsService: ISolutionsService
{
    private readonly AppDbContext _context;

    public SolutionsService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool, Exception>> SaveAsync(SolutionCreateDto solution)
    {
        var entity = new Solution
        {
            AuthorId = solution.AuthorId,
            Source = solution.Source,
            ProblemId = solution.ProblemId,
            SolutionResult = new SolutionResult
            {
                ResultValues = solution.Result
                    .Select(x => new ResultValue{Value = x.Output, IsSuccess = x.CorrectOutput is null})
                    .ToList()
            }
        };

        try
        {
            await _context.Solutions.AddAsync(entity);
            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            return new ErrorResult<bool, Exception>(e);
        }

        return new SuccessResult<bool, Exception>(true);
    }

    public IQueryable<Solution> GetFilteredById(Guid id) => _context.Solutions.Where(x => x.Id == id);

    public IQueryable<Solution> GetAll()
        => _context.Solutions;
}
