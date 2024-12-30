using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using web1.Models;

namespace web1.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<OrderStatus> OrderStatuses { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Banner> Banners { get; set; }
        

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Seed admin users
            var hasher = new PasswordHasher<ApplicationUser>();
            var adminUsers = new List<ApplicationUser>
            {
                new ApplicationUser
                {
                    Id = "1",
                    UserName = "admin1@example.com",
                    NormalizedUserName = "ADMIN1@EXAMPLE.COM",
                    Email = "admin1@example.com",
                    NormalizedEmail = "ADMIN1@EXAMPLE.COM",
                    EmailConfirmed = true,
                    FullName = "Admin One",
                    Address = "Admin Address 1",
                    PhoneNumber = "0123456789"
                },
                new ApplicationUser
                {
                    Id = "2",
                    UserName = "admin2@example.com",
                    NormalizedUserName = "ADMIN2@EXAMPLE.COM",
                    Email = "admin2@example.com",
                    NormalizedEmail = "ADMIN2@EXAMPLE.COM",
                    EmailConfirmed = true,
                    FullName = "Admin Two",
                    Address = "Admin Address 2",
                    PhoneNumber = "0123456788"
                },
                new ApplicationUser
                {
                    Id = "3",
                    UserName = "admin3@example.com",
                    NormalizedUserName = "ADMIN3@EXAMPLE.COM",
                    Email = "admin3@example.com",
                    NormalizedEmail = "ADMIN3@EXAMPLE.COM",
                    EmailConfirmed = true,
                    FullName = "Admin Three",
                    Address = "Admin Address 3",
                    PhoneNumber = "0123456787"
                },
                new ApplicationUser
                {
                    Id = "4",
                    UserName = "admin4@example.com",
                    NormalizedUserName = "ADMIN4@EXAMPLE.COM",
                    Email = "admin4@example.com",
                    NormalizedEmail = "ADMIN4@EXAMPLE.COM",
                    EmailConfirmed = true,
                    FullName = "Admin Four",
                    Address = "Admin Address 4",
                    PhoneNumber = "0123456786"
                }
            };

            foreach (var admin in adminUsers)
            {
                admin.PasswordHash = hasher.HashPassword(admin, "Admin123!");
                builder.Entity<ApplicationUser>().HasData(admin);
            }

            // Seed admin role
            builder.Entity<IdentityRole>().HasData(
                new IdentityRole
                {
                    Id = "1",
                    Name = "Admin",
                    NormalizedName = "ADMIN"
                }
            );

            // Assign admin role to admin users
            foreach (var admin in adminUsers)
            {
                builder.Entity<IdentityUserRole<string>>().HasData(
                    new IdentityUserRole<string>
                    {
                        UserId = admin.Id,
                        RoleId = "1"
                    }
                );
            }
        }
    }
} 