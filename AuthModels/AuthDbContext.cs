using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Adventure19.AuthModels;

public partial class AuthDbContext : DbContext
{
    private readonly IConfiguration _configuration;

    // Questo costruttore viene utilizzato dalla Dependency Injection
    public AuthDbContext(DbContextOptions<AuthDbContext> options, IConfiguration configuration)
        : base(options)
    {
        _configuration = configuration;
    }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Legge la connection string "AuthDbConnection" da appsettings.json
            var connectionString = _configuration.GetConnectionString("AuthDbConnection");
            optionsBuilder.UseSqlServer(connectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_User_Id");

            entity.ToTable("User");

            entity.HasIndex(e => e.FullName, "UQ_User_FullName").IsUnique();
            entity.HasIndex(e => e.Email, "UQ_User_Email").IsUnique();

            entity.Property(e => e.Id).HasColumnName("Id");
            entity.Property(e => e.FullName)
                  .HasColumnName("Full_Name")
                  .HasMaxLength(50)
                  .IsRequired();

            entity.Property(e => e.Email)
                  .HasMaxLength(100)
                  .IsRequired();

            entity.Property(e => e.Password)
                  .HasMaxLength(255)
                  .IsRequired();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
