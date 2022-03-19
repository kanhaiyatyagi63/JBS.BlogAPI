using JBS.DataLayer.Abstracts;
using JBS.DataLayer.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace JBS.DataLayer;

public class DataContext : IdentityDbContext<AppUser, AppRole, string, IdentityUserClaim<string>,
    AppUserRole, IdentityUserLogin<string>,
    IdentityRoleClaim<string>, IdentityUserToken<string>>
{
    private readonly IUserContextService _userContextService;

    public DataContext(DbContextOptions options,
        IUserContextService userContextService) : base(options)
    {
        _userContextService = userContextService;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        //Set Rules for AppUser properties
        var applicationUserEntity = builder.Entity<AppUser>();
        applicationUserEntity.Property(x => x.CreatedDate).HasColumnType("DATETIME");
        applicationUserEntity.Property(x => x.UpdatedDate).HasColumnType("DATETIME");
        applicationUserEntity.Property(x => x.UserId).ValueGeneratedOnAdd();
        applicationUserEntity.Property(p => p.UserId).Metadata.SetAfterSaveBehavior(Microsoft.EntityFrameworkCore.Metadata.PropertySaveBehavior.Ignore);


        //Set Rules for ApplicationRole properties
        var applicationRoleEntity = builder.Entity<AppRole>();

        applicationRoleEntity.Property(x => x.CreatedDate).HasColumnType("DATETIME");
        applicationRoleEntity.Property(x => x.UpdatedDate).HasColumnType("DATETIME");

        builder.ApplyConfigurationsFromAssembly(GetType().Assembly);
        base.OnModelCreating(builder);
        var applicationUserRole = builder.Entity<AppUserRole>();
        applicationUserRole.HasOne(ur => ur.Role)
               .WithMany(r => r.UserRoles)
               .HasForeignKey(ur => ur.RoleId)
               .IsRequired();

        applicationUserRole.HasOne(ur => ur.User)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.UserId)
            .IsRequired();

    }

    public override int SaveChanges()
    {
        SetAuditProperties();
        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        SetAuditProperties();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public async override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        SetAuditProperties();
        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    public async override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetAuditProperties();
        return await base.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> CustomSaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetAuditProperties();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void SetAuditProperties()
    {
        var now = DateTime.UtcNow;
        string userId = _userContextService?.GetUserId() ?? String.Empty;
        ChangeTracker.DetectChanges();
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
            {
                continue;
            }

            if (entry.State == EntityState.Added && entry.Metadata.FindProperty("CreatedDate") != null)
            {
                entry.Property("CreatedDate").CurrentValue = now;
                entry.Property("UpdatedDate").CurrentValue = now;
                entry.Property("CreatedBy").CurrentValue = userId;
                entry.Property("UpdatedBy").CurrentValue = userId;
            }

            if (entry.State == EntityState.Modified && entry.Metadata.FindProperty("UpdatedDate") != null)
            {
                entry.Property("UpdatedDate").CurrentValue = now;
                entry.Property("UpdatedBy").CurrentValue = userId;
            }
        }
    }
}
