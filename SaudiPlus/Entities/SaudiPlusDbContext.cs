using SaudiPlus.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using SaudiPlus.Interfaces.Repository;
using SaudiPlus.Entities.Notification;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace SaudiPlus.Entities;

public partial class SaudiPlusDbContext : IdentityDbContext<User, Role, Guid>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    public SaudiPlusDbContext(
        DbContextOptions<SaudiPlusDbContext> options,
        IHttpContextAccessor httpContextAccessor
    ) : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public DbSet<ForgetPassword> ForgetPassword { get; set; }
    public DbSet<Category> Category { get; set; }
    public DbSet<Banner> Banner { get; set; }
    public DbSet<City> City { get; set; }
    public DbSet<IndividualUser> IndividualUser { get; set; }
    public DbSet<BusinessUser> BusinessUsers { get; set; }
    public DbSet<Offer> Offers { get; set; }
    public DbSet<OfferLogo> OfferLogos { get; set; }
    public DbSet<UserOfferRedeem> UserOfferRedeem { get; set; }
    public DbSet<Branch> Branch { get; set; }
    public DbSet<SupportType> SupportType { get; set; }
    public DbSet<SupportTicket> SupportTicket { get; set; }
    public DbSet<SupportMessage> SupportMessage { get; set; }
    public DbSet<Policy> Policy { get; set; }
    public DbSet<Invoice> Invoice { get; set; }
    public DbSet<ContactUs> ContactUs { get; set; }
    public DbSet<BusinessPolicy> BusinessPolicy { get; set; }
    public DbSet<Partner> Partner { get; set; }
    public DbSet<Contract> Contract { get; set; }
    public DbSet<UserOfferToken> UserOfferToken { get; set; }
    public DbSet<Favourite> Favourite { get; set; }
    public DbSet<BaseNotification> Notification { get; set; }
    public DbSet<Wallet> Wallet { get; set; }
    public DbSet<SubAdminUser> SubAdminUser { get; set; }
    public DbSet<UserPermission> UserPermissions { get; set; }
    public DbSet<BusinessRequest> BusinessRequest { get; set; }
    public DbSet<Status> Status { get; set; }
    public DbSet<StatusInteraction> StatusInteraction { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<User>(entity =>
        {
            entity.Property(u => u.UserName).IsRequired(false);
        });

        builder.Entity<City>()
            .HasIndex(c => c.NameEn)
            .IsUnique();

        builder.Entity<City>()
            .HasIndex(c => c.NameAr)
            .IsUnique();

        builder.Entity<Category>()
            .HasIndex(c => c.NameEn)
            .IsUnique();

        builder.Entity<Category>()
            .HasIndex(c => c.NameAr)
            .IsUnique();

        builder.Entity<ForgetPassword>()
            .HasOne(f => f.User)
            .WithMany()
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<IndividualUser>()
            .HasOne(u => u.City)
            .WithMany(c => c.Users)
            .HasForeignKey(u => u.CityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Banner>()
            .HasOne(b => b.City)
            .WithMany(c => c.Banners)
            .HasForeignKey(b => b.CityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Banner>()
            .HasOne(b => b.Business)
            .WithMany(c => c.Banners)
            .HasForeignKey(b => b.BusinessId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Banner>()
            .HasOne(b => b.Category)
            .WithMany(c => c.Banners)
            .HasForeignKey(b => b.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Category>()
            .HasOne(c => c.City)
            .WithMany(c => c.Categories)
            .HasForeignKey(c => c.CityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<BusinessUser>()
            .HasOne(b => b.Category)
            .WithMany(c => c.BusinessUsers)
            .HasForeignKey(b => b.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Offer>()
            .HasOne(o => o.Business)
            .WithMany(b => b.Offers)
            .HasForeignKey(o => o.BusinessId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Offer>()
            .HasMany(o => o.Logos)
            .WithOne(l => l.Offer)
            .HasForeignKey(l => l.OfferId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<OfferLogo>()
            .Property(l => l.LogoUrl)
            .IsRequired();

        builder.Entity<Branch>()
            .HasOne(o => o.Business)
            .WithMany(b => b.Branches)
            .HasForeignKey(o => o.BusinessId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Branch>()
            .HasOne(o => o.City)
            .WithMany(b => b.Branches)
            .HasForeignKey(o => o.CityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<SupportTicket>()
            .HasOne(t => t.User)
            .WithMany(u => u.SupportTickets)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<SupportTicket>()
            .HasOne(t => t.SupportType)
            .WithMany()
            .HasForeignKey(t => t.SupportTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<SupportMessage>()
            .HasOne(m => m.Ticket)
            .WithMany(t => t.Messages)
            .HasForeignKey(m => m.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<SupportMessage>()
            .HasOne(m => m.User)
            .WithMany()
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Invoice>()
            .HasOne(i => i.User)
            .WithMany(u => u.Invoice)
            .HasForeignKey(i => i.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Invoice>()
            .HasOne(i => i.Business)
            .WithMany(u => u.Invoice)
            .HasForeignKey(i => i.BusinessId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Invoice>()
            .HasOne(i => i.Branch)
            .WithMany(u => u.Invoice)
            .HasForeignKey(i => i.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Invoice>()
           .HasOne(i => i.Offer)
           .WithMany()
           .HasForeignKey(i => i.OfferId)
           .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<BusinessPolicy>()
           .HasOne(o => o.Business)
           .WithMany(b => b.Policy)
           .HasForeignKey(o => o.BusinessId)
           .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<UserOfferRedeem>()
            .HasOne(uor => uor.User)
            .WithMany(user => user.RedeemedOffers)
            .HasForeignKey(uor => uor.UserId);

        builder.Entity<UserOfferRedeem>()
            .HasOne(uor => uor.Offer)
            .WithMany(offer => offer.RedeemedUsers)
            .HasForeignKey(uor => uor.OfferId);

        builder.Entity<Partner>()
            .HasOne(p => p.Category)
            .WithMany(c => c.Partners)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Partner>()
            .HasOne(p => p.BusinessUser)
            .WithOne(bu => bu.Partner)
            .HasForeignKey<Partner>(p => p.BusinessUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Contract>()
            .HasOne(c => c.BusinessUser)
            .WithOne(b => b.Contract)
            .HasForeignKey<Contract>(c => c.BusinessId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<UserOfferToken>()
            .HasOne(uot => uot.User)
            .WithMany()
            .HasForeignKey(uot => uot.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<UserOfferToken>()
            .HasOne(uot => uot.Branch)
            .WithMany()
            .HasForeignKey(uot => uot.BranchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Favourite>()
            .HasOne(fb => fb.User)
            .WithMany(u => u.Favorites)
            .HasForeignKey(fb => fb.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Favourite>()
            .HasOne(fb => fb.Business)
            .WithMany(b => b.FavoritedByUsers)
            .HasForeignKey(fb => fb.BusinessUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<BaseNotification>()
            .HasDiscriminator<string>("NotificationType")
            .HasValue<IndividualNotification>("Individual")
            .HasValue<GroupNotification>("Group")
            .HasValue<GeneralNotification>("General");

        builder.Entity<IndividualNotification>()
            .HasOne(n => n.User)
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<GroupNotification>()
            .HasMany(n => n.Users)
            .WithMany()
            .UsingEntity(j =>
            {
                j.ToTable("GroupNotificationUsers");
                j.HasOne(typeof(GroupNotification)).WithMany().HasForeignKey("GroupNotificationId").OnDelete(DeleteBehavior.NoAction);
                j.HasOne(typeof(IndividualUser)).WithMany().HasForeignKey("UsersId").OnDelete(DeleteBehavior.NoAction);
            });

        builder.Entity<GroupNotification>()
            .Navigation(n => n.Users)
            .UsePropertyAccessMode(PropertyAccessMode.Property);

        builder.Entity<Wallet>()
            .HasOne(w => w.BusinessUser)
            .WithMany()
            .HasForeignKey(w => w.BusinessId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<UserPermission>()
           .ToTable("UserPermissions")
           .HasOne(up => up.User)
           .WithMany()
           .HasForeignKey(up => up.UserId);

        builder.Entity<StatusInteraction>()
            .HasOne(si => si.Status)
            .WithMany(s => s.Interactions)
            .HasForeignKey(si => si.StatusId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<StatusInteraction>()
            .HasOne(si => si.IndividualUser)
            .WithMany(u => u.StatusInteractions)
            .HasForeignKey(si => si.IndividualUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Status>()
            .HasOne(s => s.Business)
            .WithMany(b => b.Status)
            .HasForeignKey(s => s.BusinessId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Status>()
            .HasOne(s => s.Offer)
            .WithMany(o => o.Status)
            .HasForeignKey(s => s.OfferId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<StatusInteraction>()
            .HasOne(i => i.Status)
            .WithMany(s => s.Interactions)
            .HasForeignKey(i => i.StatusId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    public override int SaveChanges()
    {
        UpdateAuditableEntities();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        UpdateAuditableEntities();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateAuditableEntities()
    {
        foreach (var entry in ChangeTracker.Entries<IAuditedEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreationTime = DateTime.UtcNow;
                    break;

                case EntityState.Modified:
                    entry.Entity.LastModificationTime = DateTime.UtcNow;
                    break;
            }
        }
    }
}
