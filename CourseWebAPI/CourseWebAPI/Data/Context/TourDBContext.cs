using CourseWebAPI.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CourseWebAPI.Data.Context
{
    public partial class TourDBContext : IdentityDbContext<User>
    {
        public virtual DbSet<RefreshToken> RefreshTokens { get; set; }


        public TourDBContext()
        {
        }

        public TourDBContext(DbContextOptions<TourDBContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.EnableSensitiveDataLogging();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            OnModelCreatingPartial(modelBuilder);

            base.OnModelCreating(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
