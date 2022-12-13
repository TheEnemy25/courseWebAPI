using CourseWebAPI.Data.Entities.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace CourseWebAPI.Data.Entities
{
    public class User : IdentityUser, IEntity
    {
        public string? Name { get; set; }
        public ICollection<RefreshToken>? RefreshTokens { get; set; }
    }
}
