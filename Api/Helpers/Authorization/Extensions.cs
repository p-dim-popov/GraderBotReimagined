using System.Security.Claims;

namespace Api.Helpers.Authorization;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetId(this ClaimsPrincipal user)
        => user.Claims
                .FirstOrDefault(x => x.Type == "id") switch
            {
                { Value: {} id } => Guid.Parse(id),
                _ => Guid.Empty,
            };
}
