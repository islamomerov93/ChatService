namespace ChatService.Data
{
    using System.Data;

    using ChatService.Data.Models;

    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid,
        IdentityUserClaim<Guid>, IdentityUserRole<Guid>, IdentityUserLogin<Guid>, IdentityRoleClaim<Guid>,
        IdentityUserToken<Guid>>
    {
        public ApplicationUser currentUser { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationDbContext"/> class.
        /// </summary>
        /// <param name="options">
        /// The options.
        /// </param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options: options)
        {
        }

        public DbSet<Chat> Chats { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<NotificationUser> NotificationUsers { get; set; }
        public DbSet<ChatUser> ChatUsers { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ChatUser>()
                .HasKey(x => x.Id);

            builder.Entity<NotificationUser>()
                .HasKey(x => new { x.NotificationId, x.ChatUserId });

            builder.Entity< IdentityRoleClaim<Guid>>(buildAction: entity => { entity.ToTable(name: "RoleClaims"); });

            builder.Entity<IdentityRole<Guid>>(buildAction: entity => { entity.ToTable(name: "Roles"); });
            builder.Entity<IdentityUserRole<Guid>>(buildAction: entity => { entity.ToTable(name: "UserRoles"); });

            builder.Entity<IdentityUserClaim<Guid>>(buildAction: entity => { entity.ToTable(name: "UserClaims"); });

            builder.Entity<IdentityUserLogin<Guid>>(buildAction: entity => { entity.ToTable(name: "UserLogins"); });

            builder.Entity<IdentityUserToken<Guid>>(buildAction: entity => { entity.ToTable(name: "UserTokens"); });

            builder.Entity<ApplicationUser>(
                buildAction: entity =>
                    {
                        entity.ToTable(name: "Users");

                        entity.Property(propertyExpression: e => e.FirstName)
                            .HasColumnType(typeName: SqlDbType.NVarChar.ToString())
                            .HasMaxLength(maxLength: 50)
                            .IsRequired();

                        entity.Property(propertyExpression: e => e.LastName)
                            .HasColumnType(typeName: SqlDbType.NVarChar.ToString())
                            .HasMaxLength(maxLength: 50)
                            .IsRequired();

                        entity.HasIndex(indexExpression: e => e.UserName)
                            .IsUnique();

                        entity.HasIndex(indexExpression: e => e.Email)
                            .IsUnique();
                    });
        }
    }
}
