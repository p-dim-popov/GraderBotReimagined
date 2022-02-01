using System.Security.Claims;

namespace Api.Helpers.Authorization;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetId(this ClaimsPrincipal user)
        => Guid.Parse(user.Claims
            .FirstOrDefault(x => x.Type == "id")!.Value);
}
