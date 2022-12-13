using CourseWebAPI.Data.Entities;
using CourseWebAPI.Data.Entities.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CourseWebAPI
{
    public class RefreshToken : IEntity
    {
        [Key]
        public string Token { get; set; }
        public string JwtId { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsUsed { get; set; }
        public bool Invalidated { get; set; }
        public string UserId { get; set; }
        [JsonIgnore]
        public User? User { get; set; }
    }
}
