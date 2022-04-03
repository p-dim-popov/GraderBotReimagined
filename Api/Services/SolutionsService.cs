using Api.Models.Solutions;
using Api.Services.Abstractions;
using Core.Types;
using Core.Utilities;
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

    public async Task<Result<Guid, Exception>> CreateAsync(SolutionCreateDto solution)
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

        var executeResult = await Ops.RunCatchingAsync(async () =>
        {
            await _context.Solutions.AddAsync(entity);
            await _context.SaveChangesAsync();
        });

        if (executeResult is None<bool, Exception> { Error: {} exception })
        {
            return new None<Guid, Exception>(exception);
        }

        return new Some<Guid, Exception>(entity.Id);
    }

    public IQueryable<Solution> GetFilteredById(Guid id) => _context.Solutions.Where(x => x.Id == id);

    public IQueryable<Solution> GetAll()
        => _context.Solutions;
}
