using Microsoft.EntityFrameworkCore;
using static System.Net.Mime.MediaTypeNames;

namespace AlphaLogistics.API.Model
{
    public class AlphaLogisticsContext: DbContext
    {
        public virtual DbSet<ProductMaster> ProductMasters { get; set; }

        public virtual DbSet<OrderMaster> OrderMasters { get; set; }

        public virtual DbSet<CartMaster> CartMasters { get; set; }
        public virtual DbSet<OrderItems> OrderItems { get; set; }
        public virtual DbSet<UserMaster> UserMasters { get; set; }
        public virtual DbSet<RoleMaster> RoleMasters { get; set; }
        public virtual DbSet<ProductImages> ProductImages { get; set; }
        public virtual DbSet<VendorMaster> VendorMasters { get; set; }
        public virtual DbSet<CategoryMaster> CategoryMasters { get; set; }
        public virtual DbSet<SubCategoryMaster> SubCategoryMasters { get; set; }


         public AlphaLogisticsContext(DbContextOptions<AlphaLogisticsContext> options)
         : base(options)
         {

         }
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseNpgsql(
                "Host=zoumaapp.com;Port=5432;Database=AlphaLogisticsDb;Username=postgres;Password=zoumapg!@#admin;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships
            modelBuilder.Entity<UserMaster>()
           .HasOne(u => u.VendorMaster) // This navigation property doesn't exist yet!
           .WithOne(v => v.UserMaster)
           .HasForeignKey<VendorMaster>(v => v.UserId)
           .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserMaster>()
                .HasOne(u => u.RoleMaster)
                .WithMany()
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserMaster>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<UserMaster>()
                .HasIndex(u => u.UserName)
                .IsUnique();

            modelBuilder.Entity<VendorMaster>()
                .HasIndex(v => v.Email)
                .IsUnique();

            modelBuilder.Entity<VendorMaster>()
                .HasIndex(v => v.Phone)
                .IsUnique();


            //Product relationship Mapping
            modelBuilder.Entity<ProductMaster>()
               .HasOne(p => p.VendorMaster)
               .WithMany(v => v.ProductMasters)
               .HasForeignKey(p => p.VendorId)
               .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductMaster>()
                .HasOne(p => p.SubCategoryMaster)
                .WithMany()
                .HasForeignKey(p => p.SubCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure ProductImages relationship
            modelBuilder.Entity<ProductImages>()
                .HasOne(pi => pi.ProductMaster)
                .WithMany()
                .HasForeignKey(pi => pi.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Category-SubCategory relationship
            modelBuilder.Entity<CategoryMaster>()
                .HasMany(c => c.SubCategoryMasters)
                .WithOne(sc => sc.CategoryMaster)
                .HasForeignKey(sc => sc.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
