using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : DbContext
{

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        
    }

    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {

            entity.ToTable("users");
    
            modelBuilder.Entity<User>()
            .HasIndex(u => u.Login)
            .IsUnique();

            entity.HasKey(e => e.Id).HasName("id");
            entity.Property(e => e.Name).HasColumnName("name").IsRequired();
            entity.Property(e => e.Login).HasColumnName("login").IsRequired();
            entity.Property(e => e.Email).HasColumnName("email");
            entity.HasIndex(e => e.Login).IsUnique().HasDatabaseName("uq_users_login");;
            entity.Property(e => e.Password).HasColumnName("password").IsRequired();
            entity.Property(e => e.AvatarUrl).HasColumnName("avatar_url");
            entity.Property(e => e.Legend).HasColumnName("legend").IsRequired();
            entity.Property(e => e.ReputationScore).HasColumnName("reputation_score").IsRequired();
            entity.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
            entity.Property(e => e.RefreshToken).HasColumnName("refresh_token").IsRequired();
        });
    }   
}