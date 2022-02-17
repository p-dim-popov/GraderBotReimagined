using System.ComponentModel.DataAnnotations;

namespace Api.Models;

public record Pagination(
    [Required]
    int? Page,

    [Required]
    int? PageSize
);
