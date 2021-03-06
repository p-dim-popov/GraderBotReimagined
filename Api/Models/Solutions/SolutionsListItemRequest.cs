using Api.Models.Problem;

namespace Api.Models.Solutions;

public record SolutionsListItemRequest(
    BriefProblemTypeDescription? TypeDescription,
    Pagination? Pagination
);
