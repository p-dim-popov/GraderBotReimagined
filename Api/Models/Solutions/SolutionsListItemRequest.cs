using Api.Models.Problem;

namespace Api.Models.Solutions;

public record Filters(
    string[]? AuthorsEmails,
    string[]? ProblemAuthorsEmails,
    Guid? ProblemId
);

public record SolutionsListItemRequest(
    BriefProblemTypeDescription? TypeDescription,
    Pagination? Pagination,
    Filters? Filters
);
