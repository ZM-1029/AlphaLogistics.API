using Microsoft.EntityFrameworkCore;
using static System.Net.Mime.MediaTypeNames;

namespace AlphaLogistics.API.Model
{
    public class AlphaLogisticsContext: DbContext
    {
        public virtual DbSet<ProductMaster> ProductMasters { get; set; }
        public virtual DbSet<ProductCombo> ProductCombos { get; set; }
        public virtual DbSet<OrderStatusHistory> OrderStatusHistory { get; set; }
        public virtual DbSet<OrderMaster> OrderMasters { get; set; }

        public virtual DbSet<CartMaster> CartMasters { get; set; }
        public virtual DbSet<OrderItems> OrderItems { get; set; }
        public virtual DbSet<UserMaster> UserMasters { get; set; }
        public virtual DbSet<RoleMaster> RoleMasters { get; set; }
        public virtual DbSet<DocumentMaster> DocumentMasters { get; set; }
        public virtual DbSet<ProductImages> ProductImages { get; set; }
        public virtual DbSet<VendorMaster> VendorMasters { get; set; }
        public virtual DbSet<CategoryMaster> CategoryMasters { get; set; }
        public virtual DbSet<SubCategoryMaster> SubCategoryMasters { get; set; }
        public virtual DbSet<PradeshMaster> PradeshMasters { get; set; }


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

            modelBuilder.Entity<ProductCombo>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("ProductCombo_pkey");

                entity.ToTable("ProductCombo");

                entity.Property(e => e.Id).HasIdentityOptions(null, null, null, 9999999999999L, null, null);
               
            });

            // Configure relationships
            modelBuilder.Entity<UserMaster>()
             .HasOne(u => u.RoleMaster)
             .WithMany()
             .HasForeignKey(u => u.RoleId)
             .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserMaster>()
             .HasOne(u => u.VendorMaster)
             .WithOne(v => v.UserMaster)
             .HasForeignKey<VendorMaster>(v => v.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VendorMaster>()
                .HasOne(v => v.CreatedByUser)
                .WithMany()
                .HasForeignKey(v => v.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false); 

          
            modelBuilder.Entity<VendorMaster>()
                .HasOne(v => v.UpdatedByUser)
                .WithMany()
                .HasForeignKey(v => v.UpdatedBy)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false); 

            // VendorMaster -> ProductMaster relationship (One-to-Many)
            modelBuilder.Entity<VendorMaster>()
                .HasMany(v => v.ProductMasters)
                .WithOne(p => p.VendorMaster)
                .HasForeignKey(p => p.VendorId)
                .OnDelete(DeleteBehavior.Cascade);

            // VendorMaster -> DocumentMaster relationship (One-to-Many)
            modelBuilder.Entity<VendorMaster>()
                .HasMany(v => v.Documents)
                .WithOne(d => d.VendorMaster)
                .HasForeignKey(d => d.VendorId)
                .OnDelete(DeleteBehavior.Cascade);


            // ProductMaster -> ProductImages relationship (One-to-Many)
            modelBuilder.Entity<ProductMaster>()
                .HasMany(p => p.ProductImages)
                .WithOne(pi => pi.ProductMaster)
                .HasForeignKey(pi => pi.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // ProductMaster -> SubCategoryMaster relationship
            modelBuilder.Entity<ProductMaster>()
                .HasOne(p => p.SubCategoryMaster)
                .WithMany()
                .HasForeignKey(p => p.SubCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // ProductMaster -> CartMaster relationship (One-to-Many)
            modelBuilder.Entity<ProductMaster>()
                .HasMany(p => p.CartMasters)
                .WithOne(c => c.ProductMaster)
                .HasForeignKey(c => c.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // CartMaster -> UserMaster relationship
            modelBuilder.Entity<CartMaster>()
                .HasOne(c => c.UserMaster)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // CategoryMaster -> SubCategoryMaster relationship (One-to-Many)
            modelBuilder.Entity<CategoryMaster>()
                .HasMany(c => c.SubCategoryMasters)
                .WithOne(sc => sc.CategoryMaster)
                .HasForeignKey(sc => sc.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // SubCategoryMaster -> CategoryMaster relationship
            modelBuilder.Entity<SubCategoryMaster>()
                .HasOne(sc => sc.CategoryMaster)
                .WithMany(c => c.SubCategoryMasters)
                .HasForeignKey(sc => sc.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // Add unique constraints
            modelBuilder.Entity<UserMaster>()
                .HasIndex(u => u.Email)
                .IsUnique();

/*            modelBuilder.Entity<UserMaster>()
                .HasIndex(u => u.UserName)
                .IsUnique();*/

            modelBuilder.Entity<VendorMaster>()
                .HasIndex(v => v.PAN)
                .IsUnique();

            modelBuilder.Entity<VendorMaster>()
                .HasIndex(v => v.UserId)
                .IsUnique(); // One user can have only one vendor profile

            // Prevent duplicate cart items for same user and product
            modelBuilder.Entity<CartMaster>()
                .HasIndex(c => new { c.UserId, c.ProductId })
                .IsUnique();

            // Optional: Set default values
            modelBuilder.Entity<UserMaster>()
                .Property(u => u.IsActive)
                .HasDefaultValue(true);

            modelBuilder.Entity<VendorMaster>()
                .Property(v => v.IsActive)
                .HasDefaultValue(true);

            modelBuilder.Entity<VendorMaster>()
                .Property(v => v.IsApproved)
                .HasDefaultValue(false);

            modelBuilder.Entity<VendorMaster>()
                .Property(v => v.CustomerType)
                .HasDefaultValue("Basic");

            modelBuilder.Entity<ProductMaster>()
                .Property(p => p.IsActive)
                .HasDefaultValue(true);

            modelBuilder.Entity<CartMaster>()
                .Property(c => c.CreatedAt)
                .HasDefaultValueSql("NOW()");
            modelBuilder.Entity<CartMaster>()
                .Property(c => c.CreatedAt)
                .HasDefaultValueSql("NOW()");

            modelBuilder.Entity<UserMaster>()
                .Property(u => u.CreatedAt)
                .HasDefaultValueSql("NOW()");

            modelBuilder.Entity<VendorMaster>()
                .Property(v => v.CreatedAt)
                .HasDefaultValueSql("NOW()");

            modelBuilder.Entity<ProductMaster>()
                .Property(p => p.CreatedAt)
                .HasDefaultValueSql("NOW()");

            modelBuilder.Entity<DocumentMaster>()
                .Property(d => d.UploadedAt)
                .HasDefaultValueSql("NOW()");
        }
    }
}
