using Microsoft.EntityFrameworkCore;
using Moon.Api.Domain;

namespace Moon.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<Address> Addresses => Set<Address>();

    public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(u => u.Name).HasMaxLength(200).IsRequired();
            entity.Property(u => u.Email).HasMaxLength(320).IsRequired();
            entity.Property(u => u.Cpf).HasMaxLength(11).IsRequired();
            entity.Property(u => u.PasswordHash).HasMaxLength(200);

            entity.HasIndex(u => u.Email).IsUnique();
            entity.HasIndex(u => u.Cpf).IsUnique();
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.Property(t => t.TokenHash).HasMaxLength(64).IsRequired();
            entity.Property(t => t.CreatedByIp).HasMaxLength(45);

            entity.HasIndex(t => t.TokenHash).IsUnique();
            entity.HasIndex(t => t.UserId);

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Address>(entity =>
        {
            entity.Property(a => a.Label).HasMaxLength(50).IsRequired();
            entity.Property(a => a.Recipient).HasMaxLength(200).IsRequired();
            entity.Property(a => a.Cep).HasMaxLength(8).IsRequired();
            entity.Property(a => a.Street).HasMaxLength(200).IsRequired();
            entity.Property(a => a.Number).HasMaxLength(20).IsRequired();
            entity.Property(a => a.Complement).HasMaxLength(200);
            entity.Property(a => a.Neighborhood).HasMaxLength(100).IsRequired();
            entity.Property(a => a.City).HasMaxLength(100).IsRequired();
            entity.Property(a => a.State).HasMaxLength(2).IsRequired();

            entity.HasIndex(a => a.UserId);

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PaymentMethod>(entity =>
        {
            entity.Property(p => p.Brand).HasMaxLength(30).IsRequired();
            entity.Property(p => p.LastFourDigits).HasMaxLength(4).IsRequired();
            entity.Property(p => p.HolderName).HasMaxLength(200).IsRequired();

            entity.HasIndex(p => p.UserId);

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
