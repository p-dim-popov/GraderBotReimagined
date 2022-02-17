using Api.Models.Problem;

namespace Api.Models.Solutions;

public record SolutionsListItemRequest(
    ProblemTypeDescriptionDto? TypeDescription,
    Pagination? Pagination
);
