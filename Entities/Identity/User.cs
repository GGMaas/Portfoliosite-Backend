using Microsoft.AspNetCore.Identity;

namespace Portfoliosite_Backend.Entities.Identity;

public class User : IdentityUser
{
    public String? name { get; set; }
}